using System;
using System.Linq;
using Philadelphia.Common;
using Philadelphia.Tests.Client.Model;
using Philadelphia.Web;
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Local

namespace Philadelphia.Tests.Client.Tests
{
    public class DiContainerTests
    {
        private readonly Action<object> _trace;
        public DiContainerTests(Action<object> trace)
        {
            _trace = trace;
        }

        private void Trace<T>(T msg) => _trace(msg);

        public class SimpleDependency {
            public string SomeValue => "Blah";
        }

        public class SimpleService {
            public SimpleDependency Dependency { get; }
            public SimpleService(SimpleDependency dependency) {
                Dependency = dependency;
            }
        }

        [Fact]
        public void CanBuildWithParameterlessConstr() {
            var di = new DiContainer();
            di.Register<SimpleDependency>(LifeStyle.Transient);
            var itm = di.Resolve<SimpleDependency>();
            itm.Assert().NotNull();
            itm.SomeValue.Assert().Equal("Blah");
        }

        [Fact]
        public void CanBuildWithNonEmptyConstr() {
            var di = new DiContainer();
            di.Register<SimpleDependency>(LifeStyle.Transient);
            di.Register<SimpleService>(LifeStyle.Transient);
            var itm = di.Resolve<SimpleService>();
            itm.Assert().NotNull();
            itm.Dependency.Assert().NotNull();
            itm.Dependency.SomeValue.Assert().Equal("Blah");
        }

        [Fact]
        public void missing_dependency__error_thrown() {
            var di = new DiContainer();
            di.Register<SimpleService>(LifeStyle.Transient);
            Assert
                .ThrowsAny(() => di.Resolve<SimpleService>())
                .With(Trace)
                .With(x => x
                    .Message
                    .Assert()
                    .Equal("Failed resolving [Philadelphia.Tests.Client.Tests.DiContainerTests+SimpleService]. Implementation: [constructor of Philadelphia.Tests.Client.Tests.DiContainerTests+SimpleService]"))
                .With(x => x
                    .InnerException
                    .Assert()
                    .NotNull())
                .With(x => x
                    .InnerException.Message
                    .Assert()
                    .Equal("key Philadelphia.Tests.Client.Tests.DiContainerTests+SimpleDependency is not registered in container"));
                
        }

        public class PurportedSingleton {
            public static int InstancesCreated {get; private set; } = 0;

            public PurportedSingleton() {
                InstancesCreated++;
            }
        }
        
        public class PurportedTransient {
            public static int InstancesCreated {get; private set; } = 0;
            public int Id { get; }

            public PurportedTransient() {
                Id = InstancesCreated++;
            }
        }

        [Fact]
        public void IsSingletonLifeStyleRespected() {
            var di = new DiContainer();
            di.Register<PurportedSingleton>(LifeStyle.Singleton);
            
            var itm1 = di.Resolve<PurportedSingleton>();
            var itm2 = di.Resolve<PurportedSingleton>();
            var itm3 = di.Resolve<PurportedSingleton>();

            PurportedSingleton.InstancesCreated.Assert().Equal(1);
            itm1.Assert().Equal(itm2);
            itm3.Assert().Equal(itm2);
        }
        
        [Fact]
        public void IsTransientLifeStyleRespected() {
            var di = new DiContainer();
            di.Register<PurportedTransient>(LifeStyle.Transient);
            
            var itm1 = di.Resolve<PurportedTransient>();
            var itm2 = di.Resolve<PurportedTransient>();
            var itm3 = di.Resolve<PurportedTransient>();

            PurportedTransient.InstancesCreated.Assert().Equal(3);
            
            itm1.Assert().NotEqual(itm2);
            itm2.Assert().NotEqual(itm3);
            itm1.Assert().NotEqual(itm3);

            itm1.Assert().True(x => x.Id == 0, "itm1.Id == 0");
            itm2.Assert().True(x => x.Id == 1, "itm2.Id == 1");
            itm3.Assert().True(x => x.Id == 2, "itm2.Id == 2");
        }

        interface ISomeAbst {
            string MyName {get;}
        }
        class FirstImpl : ISomeAbst {
            public string MyName { get; }

            public FirstImpl() {
                MyName = "first";
            }
        }
        class OtherImpl : ISomeAbst {
            public string MyName { get; }

            public OtherImpl() {
                MyName = "other";
            }
        }

        [Fact]
        public void RegisterAliasIsWorking() {
            var di = new DiContainer();
            di.RegisterAlias<ISomeAbst,FirstImpl>(LifeStyle.Transient);
            
            var itm = di.Resolve<ISomeAbst>();
            
            itm.Assert().True(x => x.MyName == "first", "gets FirstImpl");
        }
        
        [Fact]
        public void RegisterManyAliasesIsWorking() {
            var di = new DiContainer();
            di.RegisterAlias<ISomeAbst,FirstImpl>(LifeStyle.Transient);
            di.RegisterAlias<ISomeAbst,OtherImpl>(LifeStyle.Transient);
            
            var itms = di.ResolveAll<ISomeAbst>().OrderBy(x => x.MyName).ToList();
            
            itms.Assert().True(x => x.Count == 2, "two items returned by ResolveAll");
            itms.Assert().True(x => x[0].MyName == "first", "result contains implementation FirstImpl");
            itms.Assert().True(x => x[1].MyName == "other", "result contains implementation OtherImpl");
        }

        [Fact]
        public void type_not_registered__exception_thrown()
        {
            var di = new DiContainer();

            Assert
                .ThrowsAny(() => di.Resolve<FirstImpl>())
                .With(Trace)
                .Map(x => x.Message)
                .Equal("key Philadelphia.Tests.Client.Tests.DiContainerTests+FirstImpl is not registered in container");
        }

        [Fact]
        public void type_registered_with_mapping__cant_use_impl_when_resolving()
        {
            var di = new DiContainer();
            di.RegisterAlias<ISomeAbst,FirstImpl>(LifeStyle.Transient);

            Assert
                .ThrowsAny(() => di.Resolve<FirstImpl>())
                .With(Trace)
                .Map(x => x.Message)
                .Equal("key Philadelphia.Tests.Client.Tests.DiContainerTests+FirstImpl is not registered in container");
        }

        public class SelfRef {
            public SelfRef(SelfRef r) { }
        }

        [Fact]
        public void references_itself__exception_thrown() {
            var di = new DiContainer();
            di.Register<SelfRef>(LifeStyle.Transient);

            Assert
                .ThrowsAny(() => di.Resolve<SelfRef>())
                .With(Trace);
        }

        public class AWithB {
            public AWithB(BWithA x) { }
        }

        public class BWithA {
            public BWithA(AWithB x) { }
        }

        [Fact]
        public void circular_references__exception_thrown() {
            var di = new DiContainer();
            di.Register<AWithB>(LifeStyle.Transient);
            di.Register<BWithA>(LifeStyle.Transient);
            Assert
                .ThrowsAny(() => di.Resolve<AWithB>())
                .With(Trace);
        }

        [Fact]
        public void circular_references__exception_thrown_2() {
            var di = new DiContainer();
            di.Register<AWithB>(LifeStyle.Transient);
            di.Register<BWithA>(LifeStyle.Transient);

            Assert
                .ThrowsAny(() => di.Resolve<BWithA>())
                .With(Trace);
        }

        // TODO: add error message asserts to circular reference tests
        // TODO: more circular check tests: more complex mappings, factories
        // TODO: factory exception throwing tests
    }
}