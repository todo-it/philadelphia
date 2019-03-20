using System;

namespace Philadelphia.Common {
    /// <summary>
    /// design inspired by Log4j. As JS is singlethreaded thus multithreading is no problem. 
    /// De facto global shared implementation can be easily swapped at start of program and in runtime (f.e. for tests)
    /// </summary>
    public static class Logger {
        private static ILoggerImplementation _impl = new NullLoggerImplementation();

        public static void ConfigureImplementation(ILoggerImplementation impl) {
            _impl = impl;
        }

        public static void Error(Type sender, string message, params object[] args) {
            _impl.Error(sender, message, args);
        }

        public static void Info(Type sender, string message, params object[] args) {
            _impl.Info(sender, message, args);
        }

        public static void Debug(Type sender, string message, params object[] args) {
            _impl.Debug(sender, message, args);
        }
    }
}
