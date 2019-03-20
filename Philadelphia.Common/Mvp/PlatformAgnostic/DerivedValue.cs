using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public class DerivedValue<T> : IReadOnlyValue<T> {
        private readonly T _errorValue;
        private readonly Func<ResultHolder<T>> _computation;
        public T Value { get; private set;}
        public IEnumerable<string> Errors { get; private set;}
        public event ValueChangedRich<T> Changed;

        public DerivedValue(
            T initialValue, T errorValue,
            Func<ResultHolder<T>> computation, Action<DerivedValue<T>> computedOnChangeOf) {

            _errorValue = errorValue;
            _computation = computation;
            Value = initialValue;
            computedOnChangeOf(this);
        }

        public void Observes<U>(IObservableCollection<U> other) {
            // REVIEW: recursive?
            void Handler(int insertAt, U[] inserted, U[] removed) {
                var res = _computation();
                Logger.Debug(GetType(), "DerivedValue computation[2] result={0}", res);

                var old = Value;
                Errors = !res.Success ? new[] {res.ErrorMessage} : new string[] { };
                Value = res.Success ? res.Result : _errorValue;

                Logger.Debug(GetType(), "DerivedValue computation[2] oldValue={0} newValue={1} errors={2}", old, Value, Errors.PrettyToString());
                Changed?.Invoke(this, old, Value, Errors, false); //TODO don't know if user or programmatic
            }

            other.Changed += Handler;

            Handler(0, new U[0], new U[0]); //trigger validation / initialization

            Logger.Debug(GetType(), "DerivedValue subscribed to values");
        }

        public void Observes<U>(IReadOnlyValue<U> other) {
            // REVIEW: recursive?
            void Handler(bool isUserAction) {
                var res = _computation();
                Logger.Debug(GetType(), "DerivedValue computation[1] result={0}", res);

                var old = Value;
                Errors = !res.Success ? new[] {res.ErrorMessage} : new string[] { };
                Value = res.Success ? res.Result : _errorValue;

                Logger.Debug(GetType(), "DerivedValue computation[1] oldValue={0} newValue={1} errors={2}", old, Value, Errors.PrettyToString());

                Changed?.Invoke(this, old, Value, Errors, isUserAction);
            }

            other.Changed += (sender, value, newValue, errors, change) => Handler(change);

            Handler(false); //trigger validation / initialization

            Logger.Debug(GetType(), "DerivedValue subscribed to value");
        }
    }
}
