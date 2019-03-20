using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public static class LocalActionBuilder {
        public static LocalActionModel Build<T>(IActionView<T> view, Action onSuccess) {
            var result = new LocalActionModel(onSuccess);
            view.BindActionAndInitialize(result);
            return result;
        }

        public static LocalActionModel Build<T>(IActionView<T> view, Func<Task> localAction, Action onSuccess = null) {
            var result = new LocalActionModel(onSuccess, localAction);
            view.BindActionAndInitialize(result);
            return result;
        }
    }
}
