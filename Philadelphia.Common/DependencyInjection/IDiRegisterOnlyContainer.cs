using System;

namespace Philadelphia.Common {
    public interface IDiRegisterOnlyContainer {
        void RegisterFactoryMethod<T>(Func<IDiResolveReleaseOnlyContainer,T> factoryMethod, LifeStyle style) where T: class;
        void RegisterAlias(Type key, Type actualType, LifeStyle style);
    }

    public static class DiRegisterOnlyContainerExtensions {
        public static void RegisterInstance<T>(this IDiRegisterOnlyContainer self, T inst, LifeStyle style) where T: class => 
            self.RegisterFactoryMethod(_ => inst, style);

        /// <summary>for F# as sadly, it currently cannot express this generic constraint.
        /// See https://github.com/fsharp/fslang-suggestions/issues/255 for more info </summary>
        /// <typeparam name="KeyT"></typeparam>
        /// <typeparam name="ValueT"></typeparam>
        /// <param name="self"></param>
        /// <param name="style"></param>
        public static void RegisterAlias<KeyT,ValueT>(this IDiRegisterOnlyContainer self, LifeStyle style) where ValueT : KeyT => 
            self.RegisterAlias(typeof(KeyT), typeof(ValueT), style);

        public static void Register<T>(this IDiRegisterOnlyContainer self, LifeStyle ls) where T : class => 
            self.RegisterAlias(typeof(T), typeof(T), ls);
    }
}
