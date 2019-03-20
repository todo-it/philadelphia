using System;

namespace Philadelphia.Common {
    public class NullLoggerImplementation : ILoggerImplementation {
        public void Error(Type sender, string message, params object[] args) {}
        public void Info(Type sender, string message, params object[] args) {}
        public void Debug(Type sender, string message, params object[] args) {}
    }
}
