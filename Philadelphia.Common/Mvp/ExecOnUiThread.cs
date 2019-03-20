using System;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    /// <summary>
    /// in multithreaded environment (in regular multithreaded .NET) it prevents race conditions.
    /// in singlethreaded C#-transpiled to-JavaScript it is impossible to have race condition.
    /// Inspired by http://stackoverflow.com/questions/12858501/is-it-possible-to-await-an-event-instead-of-another-async-method#12858633
    /// </summary>
    public class ExecOnUiThread {
        private static Func<Action,Task> _impl;
        private static Func<Func<Task>,Task> _asyncImpl; //C# tasks are already 'started' when they are produced

        public static async Task Exec(Action uiSensitiveCode) {
            if (_impl == null) {
                throw new ArgumentException("ExecOnUiThread.Exec failed because there's no implementation set");
            }

            await _impl(uiSensitiveCode);
        }

        public static async Task ExecAsync(Func<Task> uiSensitiveCode) {
            if (_impl == null) {
                throw new ArgumentException("ExecOnUiThread.Exec failed because there's no implementation set");
            }

            await _asyncImpl(uiSensitiveCode);
        }

        public static void SetImplementation(Func<Action,Task> impl, Func<Func<Task>,Task> asyncImpl) {
            _impl = impl;
            _asyncImpl = asyncImpl;
        }
    }
}
