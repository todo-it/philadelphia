using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philadelphia.Common {
	public static class EnumerableExtensions {
        /// <summary>
        /// foreach passing both index and item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="operation"></param>
        public static void ForEachI<T>(this IEnumerable<T> self, Action<int,T> operation) {
            var i = 0;
			foreach (var t in self) {
				operation(i++, t);
			}
		}

	    public static bool AllI<T>(this IEnumerable<T> self, Func<int,T,bool> operation) {
	        var i = 0;
	        foreach (var t in self) {
	            if (!operation(i++, t)) {
	                return false;
	            }
	        }
            return true;
	    }
        
	    public static async Task ForEach<T>(this IEnumerable<T> self, Func<T,Task> operation) {
	        foreach (var t in self) {
	            await operation(t);
	        }
	    }

        public static void ForEach<T>(this IEnumerable<T> self, Action<T> operation) {
			foreach (var t in self) {
				operation(t);
			}
		}

        public static V ForEachWhileTrue<T,V>(this IEnumerable<T> self, Func<T,V> operation, V whenNoneMatched, Func<V,bool> shouldQuit) {
            foreach (var t in self) {
				var result = operation(t);

                if (shouldQuit(result)) {
                    return result;
                }
			}
            return whenNoneMatched;
		}

		// TODO: this seems to duplicate standard linq API - check if Linq version works in Bridge and if yes, use it.
        public static IEnumerable<O> SelectI<I,O>(this IEnumerable<I> self, Func<int,I,O> operation) {
            var i = 0;
            var result = new List<O>();

			foreach (var t in self) {
                result.Add(
                    operation(i++, t));
			}

            return result;
		}

		public static string PrettyToString<T>(this IEnumerable<T> self, Func<T,string> specialToString = null) {
            if (self == null) {
                return "null";
            }

		    Func<T,string> toStr = specialToString ?? (x => x == null ? "null" : x.ToString());
			var result = "";
			var c = 0;
			foreach (var t in self) {
				result += toStr(t) + ",";
				c++;
			}

			return "[" + (
                    c <= 0 ? 
                        result
                    : 
                        result.Substring(0, result.Length-1)
                ) + "]";
		}

        /// <summary>warning it is slow as it iterates all values and uses equals...</summary>
        public static int IndexOfUsingEquals<T>(this IEnumerable<T> self, T elem) {
            var i = 0;
            Func<T,bool> eq;
            
            if (elem == null) {
                eq = x => x == null;
            } else {
                eq = x => x.Equals(elem);
            }

			foreach (var t in self) {
                if (eq(t)) {
                    return i;
                }
				i++;
			}
            return -1;
		}
        
	    /// <summary> warning: naive / not efficient</summary>
	    public static bool HasTheSameContentAs<T>(this IEnumerable<T> self, params T[] other) {
	        return self.HasTheSameContentAs(other.AsEnumerable());
	    }
        
        /// <summary> warning: naive / not efficient</summary>
	    public static bool HasTheSameContentAs<T>(this IEnumerable<T> self, IEnumerable<T> other) {
            var s = self.ToList();
            var o = other.ToList();

	        if (s.Any(x => !o.Contains(x))) {
	            return false;
	        }
	        if (o.Any(x => !s.Contains(x))) {
	            return false;
	        }
	        return true;
	    }
        
	    public static bool HasTheSameOrderAndContentAs<T>(this IEnumerable<T> self, IEnumerable<T> other) {
	        var s = self.ToList();
	        var o = other.ToList();

            if (s.Count != o.Count) {
	            return false;
	        }

	        for (var i=0; i<s.Count; i++) {
	            if (!object.Equals(s[i], o[i])) {
	                return false;
	            }
	        }
            
	        return true; //for empty coll
	    }

        public static ISet<T> ToSet<T>(this IEnumerable<T> self) {
            return new HashSet<T>(self);
        }

	    public static ISet<OutT> ToSet<InpT,OutT>(this IEnumerable<InpT> self, Func<InpT,OutT> conv) {
	        return new HashSet<OutT>(self.Select(conv));
	    }
        
        public static bool ContainsAny<T>(this IEnumerable<T> self, IEnumerable<T> other) {
            var tmp = self.ToList();
            return other.Any(x => tmp.Contains(x));
        }

	    public static IDictionary<K,V> ToDictionary<K,V>(this IEnumerable<Tuple<K,V>> self) {
	        return self.ToDictionary(x => x.Item1, x => x.Item2);
	    }

        /// <summary> added as there's no easy way to avoid EnumerableInstance'</summary>
        public static IEnumerable<T> AsIEnumerable<T>(this IEnumerable<T> self) {
            return self;
        }

        public static bool ContainsAny<T>(this IEnumerable<T> self, params T[] other) {
            var tmp = self.ToList();
            return other.Any(x => tmp.Contains(x));
        }

        public static T SingleOrFail<T>(
            this IEnumerable<T> self, string errMsgNoElems, string errMsgMoreThanOne) {

            var res = self.Take(2).ToList();
            if (res.Count <= 0) {
                throw new Exception(errMsgNoElems);
            }
            if (res.Count > 1) {
                throw new Exception(errMsgMoreThanOne);
            }
            return res[0];
        }
    } 
}
