using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public class ComputedValueOnDependancyChanged<T> : IReadOnlyValue<T> {
        private readonly Func<Tuple<T,IEnumerable<string>>> _computeValueAndErrors;
        public T Value { get; private set;}
        public IEnumerable<string> Errors {get; private set; }

        private readonly List<Func<IEnumerable<string>>> _dependencies = new List<Func<IEnumerable<string>>>();
		
        public event ValueChangedRich<T> Changed;

        public ComputedValueOnDependancyChanged(Func<Tuple<T,IEnumerable<string>>> computeValueAndErrors, Action<ComputedValueOnDependancyChanged<T>> dependencies) {
            _computeValueAndErrors = computeValueAndErrors;
            var valueAndErrors = computeValueAndErrors();
            Value = valueAndErrors.Item1;
            Errors = valueAndErrors.Item2;
            dependencies(this);
        }
        
        public void Observes<U>(IReadOnlyValue<U> other) {
            _dependencies.Add(() => other.Errors);

            void Handler(bool isUserAction) {
                var old = Value;

                var valueAndErrors = _computeValueAndErrors();
                Value = valueAndErrors.Item1;
                Errors = valueAndErrors.Item2;

                var errCopy = Errors.ToList();
                Logger.Debug(GetType(), "ComputedValueOnDependancyChanged oldValue={0} newValue={1} errors={2}", old, Value, errCopy.PrettyToString());

                Changed?.Invoke(this, old, Value, errCopy, isUserAction);
            }

            // REVIEW: this looks like recursive call, because handler calls event inside it's body... or am I not getting somethiung ...
            other.Changed += (sender, value, newValue, errors, change) => Handler(change);

            Handler(false); //trigger validation / initialization

            Logger.Debug(GetType(), "ComputedValueOnDependancyChanged subscribed to {0} values", _dependencies.Count);
        }
    }
}
