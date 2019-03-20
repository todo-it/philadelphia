using System.Threading.Tasks;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public delegate void Validate<in T>(T newValue, ISet<string> errors);

    public interface IReadWriteValue<ValueT> : IReadOnlyValue<ValueT> {
        event Validate<ValueT> Validate;

        void Reset(bool isUserChange = false, object sender = null);
        Task<Unit> DoChange(ValueT newValue, bool isUserChange, object sender=null, bool mayBeRejectedByValidation=true); //due to user input or programmatic
    }
}
