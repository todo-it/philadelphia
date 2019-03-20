using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
	public static class DictionaryExtensions {
        public static void AddRange<KeyT,ValueT>(this IDictionary<KeyT,ValueT> self, IDictionary<KeyT,ValueT> itemsToAdd) {
			foreach (var t in itemsToAdd) {
				self.Add(t.Key, t.Value);
			}
		}

	    public static void AddRange<KeyT,ValueT>(this IDictionary<KeyT,ValueT> self, IEnumerable<Tuple<KeyT,ValueT>> itemsToAdd) {
	        foreach (var t in itemsToAdd) {
	            self.Add(t.Item1, t.Item2);
	        }
	    }

	    public static IDictionary<KeyT,ValueT> Create<KeyT,ValueT>(IEnumerable<Tuple<KeyT,ValueT>> inp) {
            var result = new Dictionary<KeyT,ValueT>();
	        inp.ForEach(x => result.Add(x.Item1, x.Item2));
	        return result;
        }

	    public static IDictionary<KeyT,ValueT> WithoutOtherKeysThan<KeyT,ValueT>(this IDictionary<KeyT,ValueT> self, IEnumerable<KeyT> keysToLeave) {
            var result = new Dictionary<KeyT,ValueT>();

	        keysToLeave.ForEach(x => {
	            if (self.TryGetValue(x, out var val)) {
                    result.Add(x, val);
                }
	        });

            return result;
	    }

	    public static TItem GetOrAdd<TKey, TItem>(this IDictionary<TKey, TItem> dict, TKey key, Func<TItem> factory) {
	        if (dict.TryGetValue(key, out var existing)) {
	            return existing;
	        }
	        else {
	            var @new = factory();
	            dict.Add(key, @new);
	            return @new;
	        }
	    }

	    public static void AddToList<TKey, TItem>(this IDictionary<TKey, List<TItem>> dict, TKey key, params TItem[] items) 
	        => dict.GetOrAdd(key, () => new List<TItem>()).AddRange(items);
	} 
}
