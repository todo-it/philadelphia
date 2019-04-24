using System;
using System.Linq;
using Philadelphia.Common;

namespace Philadelphia.Testing.DotNetCore {
    /// <summary>simplified F#'s Choice type clone for C# - 2 params version</summary>
    public class CsChoice<F,S> {
        private readonly bool _first;
        private readonly object[] _vals;

        private CsChoice(bool useFirst, object first, object second) {
            _first = useFirst;
            _vals = new []{first, second};
        }

        public static CsChoice<F,S> Create(F inp) {
            return new CsChoice<F,S>(true, inp, null);
        }

        public static CsChoice<F,S> Create(S inp) {
            return new CsChoice<F,S>(false, null, inp);
        }

        public override bool Equals(object obj) {
            if (!(obj is CsChoice<F,S>)) {
                return false;
            }
            var o = (CsChoice<F,S>)obj;

            if (_first != o._first) {
                return false;
            }

            return _vals.IsTheSameAs(o._vals);
        }

        public override int GetHashCode() {
            return _vals.Sum(x => x.GetHashCode()) + (_first ? 1 : 0);
        }

        public override string ToString() {
            return $"<CsChoice {_vals.PrettyToString()}>";
        }

        public bool Is<X>() {
            return 
                typeof(X) == typeof(F) && _first || 
                typeof(X) == typeof(S) && !_first;
        }

        public X As<X>() {
            if (typeof(X) == typeof(F) && _first) {
                return (X)_vals[0];
            } 
            
            if (typeof(X) == typeof(S) && !_first) {
                return (X)_vals[1];
            }

            throw new Exception($"EitherOr<{typeof(F).FullName},{typeof(S).FullName}> cannot extract {typeof(X).FullName}");
        }
    }

    /// <summary>simplified F#'s Choice type clone for C# - 3 params version</summary>
    public class CsChoice<F,S,T> {
        private readonly int _useItem;
        private readonly object[] _vals;

        private CsChoice(int useItem, object first, object second, object third) {
            _useItem = useItem;
            _vals = new []{first, second, third};
        }

        public static CsChoice<F,S,T> Create(F inp) {
            return new CsChoice<F,S,T>(0, inp, null, null);
        }

        public static CsChoice<F,S,T> Create(S inp) {
            return new CsChoice<F,S,T>(1, null, inp, null);
        }
        
        public static CsChoice<F,S,T> Create(T inp) {
            return new CsChoice<F,S,T>(2, null, null, inp);
        }

        public override bool Equals(object obj) {
            if (!(obj is CsChoice<F,S,T>)) {
                return false;
            }
            var o = (CsChoice<F,S,T>)obj;

            if (_useItem != o._useItem) {
                return false;
            }

            return _vals.IsTheSameAs(o._vals);
        }

        public override int GetHashCode() {
            return _vals.Sum(x => x.GetHashCode()) + _useItem;
        }

        public override string ToString() {
            return $"<CsChoice {_vals.PrettyToString()}>";
        }

        public bool Is<X>() {
            return 
                typeof(X) == typeof(F) && _useItem == 0 || 
                typeof(X) == typeof(S) && _useItem == 1 || 
                typeof(X) == typeof(T) && _useItem == 2;
        }

        public X As<X>() {
            if (typeof(X) == typeof(F) && _useItem == 0) {
                return (X)_vals[0];
            } 
            
            if (typeof(X) == typeof(S) && _useItem == 1) {
                return (X)_vals[1];
            }
            
            if (typeof(X) == typeof(T) && _useItem == 2) {
                return (X)_vals[2];
            }

            throw new Exception($"EitherOr<{typeof(F).FullName},{typeof(S).FullName},{typeof(T).FullName}> cannot extract {typeof(X).FullName}");
        }
    }
}
