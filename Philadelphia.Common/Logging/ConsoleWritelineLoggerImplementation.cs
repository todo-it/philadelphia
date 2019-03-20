using System;

namespace Philadelphia.Common {
    public class ConsoleWritelineLoggerImplementation : ILoggerImplementation {
        private void LogImpl(string level, Type sender, string message, object[] args) {
            Console.WriteLine(
                DateTime.UtcNow.ToString("o") + 
                " " +
                this.FlattenSafe(level, sender, message, args));
        }

        public void Error(Type sender, string message, params object[] args) {
            LogImpl("ERROR", sender, message, args);
        }

        public void Info(Type sender, string message, params object[] args) {
            LogImpl("INFO", sender, message, args);
        }

        public void Debug(Type sender, string message, params object[] args) {
            LogImpl("DEBUG", sender, message, args);
        }
    }
}
