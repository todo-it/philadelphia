using System;
using Bridge;
using Philadelphia.Common;

// REVIEW: common implementation for all loggers is similar, thus it could be put into one place and used in all logger classes
namespace Philadelphia.Web {
    public class ForwardMatchingToConsoleLogLoggerImplementation : ILoggerImplementation {
        // REVIEW: this delegate is not used
        private readonly Func<string, bool> _discardMessageWhenFalse;
        private readonly DateTime _start = DateTime.UtcNow;

        [Template("console.log({msg:raw})")]
        private static extern void LogImpl(string msg);

        public ForwardMatchingToConsoleLogLoggerImplementation(Func<string,bool> discardMessageWhenFalse) {
            _discardMessageWhenFalse = discardMessageWhenFalse;
        }
        
        private void Log(Type sender, string level, string message, object[] args) {
            LogImpl(
                string.Format("{0:0000000} ", (DateTime.UtcNow - _start).TotalMilliseconds) + 
                this.FlattenSafe(level, sender, message, args));
        }

        public void Error(Type sender, string message, params object[] args) {
            Log(sender, "ERROR", message, args);
        }
        
        public void Info(Type sender, string message, params object[] args) {
            Log(sender, "INFO", message, args);
        }

        public void Debug(Type sender, string message, params object[] args) {
            Log(sender, "DEBUG", message, args);
        } 
    }
}
