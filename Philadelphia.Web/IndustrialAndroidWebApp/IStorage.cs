using System;

namespace Philadelphia.Web {
    public interface IStorage {
        void Clear();
        string GetStringOrNull(string key);

        /**
         * adds or replaces value for key
         */
        void Set(string key, string value);
    }

    public static class StorageExtensions {
        public static T GetOrNull<T>(this IStorage self, string key, Func<string, T> deserialize) {
            var res = self.GetStringOrNull(key);

            if (res == null) {
                return default(T);
            }

            return deserialize(res);
        }
    }
}
