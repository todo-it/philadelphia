using System.Collections.Generic;

namespace Philadelphia.Common {
    public delegate void ValueChangedRich<in T>(object sender, T oldValue, T newValue, IEnumerable<string> errors, bool isUserChange); //real change or overwrite or validation problems

    public interface IReadOnlyValue<out ValueT> {
        ValueT Value { get; }
        IEnumerable<string> Errors { get; }

        event ValueChangedRich<ValueT> Changed;
    }
}
