namespace Philadelphia.Common {
    public interface IProvider<T> {
        T Provide();
    }
}
