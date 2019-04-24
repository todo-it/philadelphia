namespace Philadelphia.Testing.DotNetCore {
    public interface ICodec {
        T Decode<T>(string txt);
        string Encode<T>(T obj);
    }
}
