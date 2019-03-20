namespace Philadelphia.Common {
    public interface IByKeyValueAccessor<out FullT,KeyT> : IReadWriteValue<KeyT> {
        FullT FullValue { get; }
    }
}
