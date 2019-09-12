﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common
{
    public class PhillyContainer : IDiContainer {
        public interface IScopeProvider
        {
            IDiResolveReleaseOnlyContainer CreateScope();
        }

        class ResolvingInfo {
            public readonly LinkedList.Node<Type> KeyPath;
            private ResolvingInfo(LinkedList.Node<Type> path) => KeyPath = path;

            public ResolvingInfo AddKey(Type t) => 
                new ResolvingInfo(KeyPath.Add(t));

            public static ResolvingInfo Create(Type rootKey) => 
                new ResolvingInfo(LinkedList.Singleton(rootKey));

            public string KeyPathToString() =>
                KeyPath
                    .AsEnumerableHeadToTail()
                    .Select(k => k.FullName)
                    .Then(x => string.Join(" <- ", x));
        }

        class Implementation {
            public LifeStyle Life {get; }
            public Func<IDiResolveReleaseOnlyContainer, ResolvingInfo, object> Factory {get; }
            public string Name { get; }
            public bool SingletonPopulated {get; set;} //to support null values
            public object Singleton {get; set;}

            public Implementation(string name, LifeStyle life,
                Func<IDiResolveReleaseOnlyContainer, ResolvingInfo, object> factory) {
                Life = life;
                Factory = factory;
                Name = name;
            }
        }

        private readonly Dictionary<Type,List<Implementation>> _implementations = new Dictionary<Type, List<Implementation>>();

        private IReadOnlyList<Implementation> FindImplementationsFor(ResolvingInfo resolvingInfo) => 
            _implementations.TryGetValue(resolvingInfo.KeyPath.Head, out var impls) 
                ? impls 
                : new List<Implementation>();

        private static void CheckForCircularity(ResolvingInfo ri) {
            var length = ri.KeyPath.AsEnumerableHeadToTail().Count();
            var distinctLength = ri.KeyPath.AsEnumerableHeadToTail().Distinct().Count();

            if (length != distinctLength) {
                throw new Exception($"Circular references detected:\n{ri.KeyPathToString()}");
            }
        }

        private object ResolveImplementationUnsafe(Implementation impl, ResolvingInfo info) {
            CheckForCircularity(info);
            switch (impl.Life) {
                case LifeStyle.Transient:
                    return impl.Factory(this, info);

                case LifeStyle.Singleton when impl.SingletonPopulated:
                    return impl.Singleton;

                case LifeStyle.Singleton:
                    var outcome = impl.Factory(this, info);
                    impl.Singleton = outcome;
                    impl.SingletonPopulated = true;
                    return outcome;

                default:
                    throw new Exception($"unsupported LifeStyle {impl.Life}");
            }
        }

        private object ResolveImplementation(Implementation impl, ResolvingInfo resolvingInfo) {
            try {
                return ResolveImplementationUnsafe(impl, resolvingInfo);
            }
            catch (Exception e) {
                throw new Exception($"Failed resolving [{resolvingInfo.KeyPath.Head.FullName}]. Implementation: [{impl.Name}]", e);
            }
        }

        private object ResolveOne(ResolvingInfo ctx)
        {
            var impl = FindImplementationsFor(ctx).FirstOrDefault();
            if (impl == null)
            {
                throw new Exception($"key {ctx.KeyPath.Head.FullName} is not registered in container");
            }
            else
            {
                return ResolveImplementation(impl, ctx);
            }
        }

        private IReadOnlyList<object> ResolveAll(ResolvingInfo resolvingInfo) => 
            FindImplementationsFor(resolvingInfo).Select(reg => ResolveImplementation(reg, resolvingInfo)).ToList();

        private delegate object Resolver(ResolvingInfo ctx);

        private static object BuildUsingReflection(Type actualType, ResolvingInfo ctx, Resolver resolve) {
            var constrs = actualType.GetConstructors();
            if (constrs.Length > 1){
                throw new Exception($"Type {actualType.FullName} has more than one constructor");
            }
            if (constrs.Length == 0){
                throw new Exception($"Type {actualType.FullName} has no constructors");
            }
        
            var constr = constrs[0];
            var cparms = constr.GetParameters();
            var instParms = cparms
                .Select(p => resolve(ctx.AddKey(p.ParameterType)))
                .ToArray();
            var inst = constr.Invoke(instParms);
            return inst;
        }

        public void RegisterAlias(Type key, Type actualType, LifeStyle ls) {
            if (!key.IsAssignableFrom(actualType)) {
                throw new Exception($"Key type {key.FullName} is not assignable from actual type {actualType.FullName}");
            }

            if (ls == LifeStyle.Scoped) {
                throw new Exception("scope lifestyle is not supported");
            }

            _implementations.AddToList(key, new Implementation("constructor of " + key.FullName, ls, (c, ctx) => BuildUsingReflection(actualType, ctx, ResolveOne)));
        }

        public void RegisterFactoryMethod(Type keyType, Func<IDiResolveReleaseOnlyContainer,object> factoryMethod, LifeStyle ls) {
            if (ls == LifeStyle.Scoped) {
                throw new Exception("scope lifestyle is not supported");
            }

            _implementations.AddToList(keyType, new Implementation("factory of " + keyType.FullName, ls, (c, ctx) => factoryMethod(c)));
        }

        public object Resolve(Type t) => ResolveOne(ResolvingInfo.Create(t));

        public (bool success, object result) TryResolve(Type t) {
            var resolvingInfo = ResolvingInfo.Create(t);
            var impls = FindImplementationsFor(resolvingInfo);

            return impls.Count <= 0 
                ? (false, null) 
                : (true, ResolveImplementation(impls.First(), resolvingInfo));
        }

        public IEnumerable<object> ResolveAll(Type key) => ResolveAll(ResolvingInfo.Create(key));

        public void Release(object t) {
            //doesn't really do anything on the web (yet?)
        }

        public IDiResolveReleaseOnlyContainer CreateScope() {
            // Copy patter from MS and their IServiceProvider
            var (ok, service) = TryResolve(typeof(IScopeProvider));
            if (ok)
            {
                return ((IScopeProvider) service).CreateScope();
            }
            else
            {
                throw new Exception("Cannot create scope because no IScopeProvider was registered");
            }
        }

        public void Dispose() {
            //doesn't really do anything on the web
        }
    }
}
