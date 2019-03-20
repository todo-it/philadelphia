using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public class AggregatedErrorsValue<T> : IReadOnlyValue<T> {
        private readonly Func<AggregatedErrorsValue<T>,T> _computation;
        public T Value { get; private set;}
        public IEnumerable<string> Errors => DependencyErrors();

        private readonly List<Func<IEnumerable<string>>> _dependencies = new List<Func<IEnumerable<string>>>();
		
        public event ValueChangedRich<T> Changed;

        public AggregatedErrorsValue(T initialValue, Func<AggregatedErrorsValue<T>,T> computation, Action<AggregatedErrorsValue<T>> dependencies) {
            _computation = computation;
            Value = initialValue;
            dependencies(this);
        }

        private List<string> DependencyErrors() {
            IEnumerable<string> result = null;
            foreach (var dep in _dependencies) {
                var other = dep();
                result = result?.Concat(other) ?? other;
            }
            return result?.ToList() ?? new List<string>();
        }

        public void Observes<U>(IReadOnlyValue<U> other) {
            _dependencies.Add(() => other.Errors);

            void Handler(object sender, U oldValue, U newValue, IEnumerable<string> _, bool isUserAction) {
                var old = Value;
                var errors = DependencyErrors();
                Value = _computation(this);

                Logger.Debug(GetType(), "AggregatedErrorsValue oldValue={0} newValue={1} errors={2}", old, Value, errors.PrettyToString());

                Changed?.Invoke(this, old, Value, errors, isUserAction);
            }

            other.Changed += Handler;

            Handler(this, other.Value, other.Value, other.Errors, false); //trigger validation / initialization

            Logger.Debug(GetType(), "AggregatedErrorsValue subscribed to {0} values", _dependencies.Count);
        }
    }
}
