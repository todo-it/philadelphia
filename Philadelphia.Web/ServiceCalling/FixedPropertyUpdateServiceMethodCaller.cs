using System;
using System.Linq.Expressions;
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
    /// To be used on datagrid
    /// </summary>
    public class FixedPropertyUpdateServiceMethodCaller<ContT,DataT> {
        private readonly Func<int, string, string, Task<ContT>> _saveOperation;
        private readonly string _propertyName;

        public FixedPropertyUpdateServiceMethodCaller(
            Func<int, string, string, Task<ContT>> saveOperation,
            Expression<Func<ContT,DataT>> getField) {

            _saveOperation = saveOperation;

            var member = getField.Body as MemberExpression;

            if (member == null) {
                throw new ArgumentException("getField expression is not of expected type MemberExpression");
            }

            _propertyName = member.Member.Name;
        }
        
        public Task<ContT> SaveField(int entityId, DataT value) {
            return _saveOperation(
                entityId, 
                _propertyName, 
                JSON.Stringify(value));
        }
    }
}
