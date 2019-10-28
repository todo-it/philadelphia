using System;

namespace Philadelphia.Common {
    public interface IDiRegisterOnlyContainer {
        /// <param name="style">null style means: use container's default LifeStyle</param>
        void RegisterFactoryMethod(Type key, Func<IDiResolveReleaseOnlyContainer, object> factoryMethod, LifeStyle? style = null);

        /// <param name="style">null style means: use container's default LifeStyle</param>
        void RegisterAlias(Type key, Type actualType, LifeStyle? style = null);
    }

    public static class DiRegisterOnlyContainerExtensions {
        public static void RegisterFactoryMethod<T>(this IDiRegisterOnlyContainer self, 
                Func<IDiResolveReleaseOnlyContainer, T> factoryMethod, LifeStyle? style = null) =>
            self.RegisterFactoryMethod(typeof(T), container => factoryMethod(container), style);

        public static void RegisterInstance<T>(
                this IDiRegisterOnlyContainer self, T inst, LifeStyle? style = null) where T: class => 
            self.RegisterFactoryMethod(_ => inst, style);

        /// <summary>sadly it is needed for F# as it currently cannot express such generic constraint.
        /// See https://github.com/fsharp/fslang-suggestions/issues/255 for more info </summary>
        public static void RegisterAlias<KeyT,ValueT>(
                this IDiRegisterOnlyContainer self, LifeStyle? style = null) where ValueT : KeyT =>
            self.RegisterAlias(typeof(KeyT), typeof(ValueT), style);

        public static void Register<T>(
                this IDiRegisterOnlyContainer self, LifeStyle? style = null) where T : class => 
            self.RegisterAlias(typeof(T), typeof(T), style);
    }
}
