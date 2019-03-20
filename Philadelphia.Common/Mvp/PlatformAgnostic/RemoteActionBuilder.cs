using System;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public static class RemoteActionBuilder {
        public static RemoteActionModel<ResT> Build<WidgetT,ResT>(
                IActionView<WidgetT> view, Func<Task<ResT>> remOper, 
                Action<ResT> onSuccessOrNull = null, Action<ResultHolder<ResT>> onFailureOrNull = null) {

            var result = new RemoteActionModel<ResT>(remOper, onSuccessOrNull, onFailureOrNull);
            view.BindActionAndInitialize(result);
            return result;
        }
    }
}
