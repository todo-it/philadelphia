using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Philadelphia.Web {
    public class MethodCaller {
        public static FixedPropertyUpdateServiceMethodCaller<ContT,DataT> ForFixedProperty<ContT,DataT>(
            Func<int, string, string, Task<ContT>> saveOperation,
            Expression<Func<ContT,DataT>> getField) {

            return new FixedPropertyUpdateServiceMethodCaller<ContT,DataT>(saveOperation, getField);
        }
    }
}
