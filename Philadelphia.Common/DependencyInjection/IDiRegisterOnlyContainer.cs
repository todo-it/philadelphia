using System;

namespace Philadelphia.Common {
    public interface IDiRegisterOnlyContainer {
        void RegisterFactoryMethod(Type key, Func<IDiResolveReleaseOnlyContainer, object> factoryMethod, LifeStyle style);
        void RegisterAlias(Type key, Type actualType, LifeStyle style);
    }

    public static class DiRegisterOnlyContainerExtensions {
        public static void RegisterFactoryMethod<T>(this IDiRegisterOnlyContainer self, 
            Func<IDiResolveReleaseOnlyContainer, T> factoryMethod, LifeStyle style = LifeStyle.Singleton) {
            self.RegisterFactoryMethod(typeof(T), container => factoryMethod(container), style);
        }

        public static void RegisterInstance<T>(this IDiRegisterOnlyContainer self, T inst, LifeStyle style = LifeStyle.Singleton) where T: class => 
            self.RegisterFactoryMethod(_ => inst, style);

        /// <summary>for F# as sadly, it currently cannot express this generic constraint.
        /// See https://github.com/fsharp/fslang-suggestions/issues/255 for more info </summary>
        /// <typeparam name="KeyT"></typeparam>
        /// <typeparam name="ValueT"></typeparam>
        /// <param name="self"></param>
        /// <param name="style"></param>
        public static void RegisterAlias<KeyT,ValueT>(this IDiRegisterOnlyContainer self, LifeStyle style = LifeStyle.Singleton) where ValueT : KeyT => 
            self.RegisterAlias(typeof(KeyT), typeof(ValueT), style);

        public static void Register<T>(this IDiRegisterOnlyContainer self, LifeStyle ls = LifeStyle.Singleton) where T : class => 
            self.RegisterAlias(typeof(T), typeof(T), ls);
    }
}
