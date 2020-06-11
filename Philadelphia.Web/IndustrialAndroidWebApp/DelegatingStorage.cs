namespace Philadelphia.Web {
    public class DelegatingStorage : IStorage {
        private readonly IStorage _adapted;
        private readonly string _keyPrefix;

        public DelegatingStorage(string keyPrefix, IStorage adapted) {
            _adapted = adapted;
            _keyPrefix = keyPrefix + ".";
        }

        public string GetStringOrNull(string key) => _adapted.GetStringOrNull(_keyPrefix + key);
        public void Set(string key, string value) => _adapted.Set(_keyPrefix+key, value);
        public void Clear() => _adapted.Clear();
    }
}
