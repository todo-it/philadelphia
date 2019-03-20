using System;

namespace Philadelphia.Common {
    public interface ILoggerImplementation {
        void Error(Type sender, string message, params object[] args);
        void Info(Type sender, string message, params object[] args);
        void Debug(Type sender, string message, params object[] args);
    }

    public static class LoggerImplementationExtensions {
        public static string FlattenSafe(
                this ILoggerImplementation self, string levelName, Type sender, string message, params object[] args) {
            
            try {
                if (args.Length <= 0) {
                    return string.Format("{0} {1} ", sender.FullName, levelName) + message;
                }
                return string.Format("{0} {1} ", sender.FullName, levelName) + string.Format(message, args);
            } catch(Exception ex) {
                return "[LOGGING ERROR] Failed to format message: " + message+" because of " + ex;
            }
        }
    }
}
