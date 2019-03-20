using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Philadelphia.Tests.Client.Model
{
    public class TestModelFactory
    {
        class Fact
        {
            public readonly MethodInfo Method;
            public readonly FactAttribute Attribute;
            public Fact(MethodInfo method, FactAttribute attribute)
            {
                Method = method;
                Attribute = attribute;
            }
        }

        class Fixture
        {
            public readonly Type Type;
            public readonly IReadOnlyCollection<Fact> Facts;
            public Fixture(Type type, IReadOnlyCollection<Fact> facts)
            {
                Type = type;
                Facts = facts;
            }
        }

        private static IReadOnlyCollection<Fact> GetFacts(Type fixture) =>
            (from m in fixture.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                let att = (FactAttribute) m.GetCustomAttributes(typeof(FactAttribute)).FirstOrDefault()
                where att != null
                select new Fact(m, att)).ToList();
                
        private static IReadOnlyCollection<Fixture> FindFixtures(Assembly asm) =>
            (from t in asm.GetTypes()
                let f = GetFacts(t)
                where f.Count > 0
                select new Fixture(t, f))
            .ToList();

        private static object CreateFixture(Type fixture, TestModel.TestSession session)
        {
            if (fixture.IsGenericTypeDefinition)
                throw new Exception($"Class {fixture.FullName} cannot be generic");

            var constructor = fixture.GetConstructors().Assert(false).Single().Item;
            var parameters = constructor.GetParameters();
            
            if (parameters.Length == 0)
                return constructor.Invoke();

            if (parameters.Length == 1)
            {
                var parameterType = parameters[0].ParameterType;
                if (parameterType == typeof(TestModel.TestSession)) return constructor.Invoke(session);
                if (parameterType == typeof(Action<object>))
                {   
                    var trace = new Action<object>(session.WriteTrace);
                    return constructor.Invoke(trace);
                }
            }

            throw new Exception($"Class {fixture.FullName} does not have constructor that can be used. Please use parameterless or with single parameter of one of the types: Action<string>, TestSession");
        }

        private static IEnumerable<Type> EnumerateParents(Type t)
        {
            while (true)
            {
                yield return t;
                if (t.DeclaringType == null) yield break;
                t = t.DeclaringType;
            }
        }

        private static string FixtureName(Type fixture) => 
            EnumerateParents(fixture)
            .Reverse()
            .Select(x => x.Name)
            .ToJoinedString(".");

        private static string TestName(MethodInfo test) => 
            test.Name
            .Replace("__", " -> ")
            .Replace("_", " ");

        private static string TestName(Type fixture, MethodInfo test) => 
            $"{FixtureName(fixture)}: {TestName(test)}";

        [SuppressMessage("ReSharper", "ConvertToLambdaExpression")]
        private static TestModel Create(Type fixture, MethodInfo test, FactAttribute fact) {
            var name = TestName(fixture, test);
            TestModel Create(TestModel.Test body) => TestModel.Create(name, body, fact.ExpectAssertionException);
            TestModel Error(string msg) => throw new Exception(msg);
            
            if (test.GetGenericArguments().Length > 0)
                return Error("Test method cannot be generic");
            if (test.GetParameters().Length > 0)
                return Error("Test method cannot have parameters");
            
            return Create(s => {
                var fixtureInstance = CreateFixture(fixture, s);
                test.Invoke(fixtureInstance);
            });
        }

        public static IReadOnlyCollection<TestModel> CreateFromAssembly(Assembly asm) => 
            (from fixture in FindFixtures(asm)
             from fact in fixture.Facts
             select Create(fixture.Type, fact.Method, fact.Attribute)).ToList();
    }
}