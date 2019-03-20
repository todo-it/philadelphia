using System;
using System.Threading.Tasks;
using Bridge.Html5;

namespace Philadelphia.Web {
    /// <summary>
    /// CRUD like modify service method on server side is assumed to receive: 
    /// -id of entity to modify
    /// -property name to modify
    /// -property value as json string (=new value of property)
    /// 
    /// This helper class is provided to adapt such raw operation to be more type safe. 
    /// It does it so by means of LinqExpression extracting field to be mutated.
    /// To be used on single entity forms
    /// </summary>
    public class FixedIdUpdateServiceMethodCaller<ContT> {
        private readonly Func<int> _idProvider;
        private readonly Func<int, string, string, Task<ContT>> _saveOperation;

        public FixedIdUpdateServiceMethodCaller(Func<int, string, string, Task<ContT>> saveOperation, Func<int> idProvider) {
            _saveOperation = saveOperation;
            _idProvider = idProvider;
        }

        public Task<ContT> SaveField<DataT>(string fieldName, DataT value) {
            return _saveOperation(
                _idProvider(), 
                fieldName, 
                JSON.Stringify(value));
        }
    }
}
