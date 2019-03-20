using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public delegate void BeforeValueChangeSimple<in T>(T newValue, bool isUserInput, Action<ISet<string>> preventPropagation);
    
    public interface IBeforeChangeCapableValueView<out ValueT> {
        event BeforeValueChangeSimple<ValueT> BeforeChange;
    }
}
