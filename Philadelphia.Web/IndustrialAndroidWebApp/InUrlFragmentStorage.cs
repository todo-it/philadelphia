using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class InUrlFragmentStorage : IStorage {
        private readonly IDictionary<string,string> _db = new Dictionary<string, string>();

        public InUrlFragmentStorage() {
            DeserializeFromLocationFragment();
        }

        public string GetStringOrNull(string key) {
            string val;

            if (_db.TryGetValue(key, out val)) {
                return val;
            }

            return null;
        }

        public void Set(string key, string value) {
            _db[key] = value;
            SerializeBackToLocationFragment();
        }

        private void DeserializeFromLocationFragment() {
            var location = Window.Instance.Location.Href;
            var fragmentCharLoc = location.IndexOf('#');

            if (fragmentCharLoc < 0) {
                return;
            }

            location = location.Substring(fragmentCharLoc + 1);

            var keysThenValues = JSON.Parse<string[]>(Window.DecodeURIComponent(location));
            Logger.Debug(GetType(), "deserialized {0} keysandvalues", keysThenValues.Length);

            if (!keysThenValues.Any()) {
                return;
            }

            if (keysThenValues.Length % 2 != 0) {
                Logger.Error(GetType(), "deserialized {0} keysandvalues is not an even number", keysThenValues.Length);
                throw new Exception("got unexpected odd elements count");
            }

            var valIdxAt = keysThenValues.Length / 2;

            for (var i = 0; i < valIdxAt; i++) {
                Logger.Debug(GetType(), "deserializing {0} -> {1}", keysThenValues[i], keysThenValues[valIdxAt + i]);
                _db.Add(keysThenValues[i], keysThenValues[valIdxAt + i]);
            }
        }

        private void SerializeBackToLocationFragment() {
            var location = Window.Instance.Location.Href;
            var fragmentCharLoc = location.IndexOf('#');

            var serialized = JSON.Stringify(_db.Keys.Concat(_db.Values).ToArray());

            if (fragmentCharLoc < 0) {
                Window.Instance.Location.Href = location + "#" + Window.EncodeURIComponent(serialized);
            } else {
                Window.Instance.Location.Href = location.Substring(0, fragmentCharLoc+1) + Window.EncodeURIComponent(serialized);
            }
        }

        public void Clear() {
            _db.Clear();
            SerializeBackToLocationFragment();
        }
    }
}
