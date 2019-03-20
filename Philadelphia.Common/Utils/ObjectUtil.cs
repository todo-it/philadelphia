using System;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public static class ObjectUtil {

        /// <summary>
        /// awaitable is of type Task&lt;unknown&gt;
        /// </summary>
        public static async Task<object> AwaitForUnknownTask(object awaitable) {
            //seems that dynamic is nonavoidable here https://stackoverflow.com/questions/29766333/task-with-result-unknown-type
            
            return await (dynamic)awaitable;
        }
        
        public static O Map<I,O>(this I self, Func<I,O> mapper) {
            return mapper(self);
        }

        public static O MapIf<I,O>(
                this I self, Func<I,bool> shouldMap, Func<I,O> mapperForTrue, Func<I,O> mapperForFalse) {

            return shouldMap(self) ? mapperForTrue(self) : mapperForFalse(self);
        }

        public static U MapNullAs<T,U>(T mayBeNull, Func<T,U> mapper, U insteadOfNull) where T : class {
            return mayBeNull != null ? mapper(mayBeNull) : insteadOfNull;
        }

        public static T MapNullAs<T>(T? mayBeNull, T insteadOfNull) where T : struct {
            return mayBeNull.HasValue ? mayBeNull.Value : insteadOfNull;
        }

        public static U MapNullAs<T,U>(T? mayBeNull, Func<T,U> mapper, U insteadOfNull) where T : struct {
            return mayBeNull.HasValue ? mapper(mayBeNull.Value) : insteadOfNull;
        }
    }
}
