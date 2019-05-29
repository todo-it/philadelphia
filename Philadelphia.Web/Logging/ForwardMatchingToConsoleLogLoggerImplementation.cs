using System;
using Bridge;
using Philadelphia.Common;

// REVIEW: common implementation for all loggers is similar, thus it could be put into one place and used in all logger classes
namespace Philadelphia.Web {
    public class ForwardMatchingToConsoleLogLoggerImplementation : ILoggerImplementation {
        // REVIEW: this delegate is not used
        private readonly Func<Type, bool> _maybeDiscardMessageWhenFalse;
        private readonly DateTime _start = DateTime.UtcNow;

        [Template("console.log({msg:raw})")]
        private static extern void LogImpl(string msg);
        
        public ForwardMatchingToConsoleLogLoggerImplementation(
                Func<Type,bool> discardMessageWhenFalseOrNull = null) {

            _maybeDiscardMessageWhenFalse = discardMessageWhenFalseOrNull;
        }
        
        private void Log(Type sender, string level, string message, object[] args) {
            var ign = _maybeDiscardMessageWhenFalse?.Invoke(sender);

            if (ign.HasValue && !ign.Value) {
                return;
            }

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
