using System;

namespace Philadelphia.Common {
    public class ResultHolder<T> {
        public bool Success { get; }
        public Exception Error { get; }
        public string ErrorMessage { get; }
        public T Result { get; }

        private ResultHolder(bool isSuccess, string errorMessage, T resultValue, Exception ex) {
            Success = isSuccess;
            ErrorMessage = errorMessage;
            Error = ex;
            Result = resultValue;
        }

        public static ResultHolder<T> CreateSuccess(T resultValue) => new ResultHolder<T>(true, null, resultValue, null);
        public static ResultHolder<T> CreateFailure(string error, Exception ex = null, T resultValue= default(T)) => 
            new ResultHolder<T>(false, error, resultValue, ex);
        public override string ToString () => 
            string.Format ("[ResultHolder: Success={0}, Error={1}, ErrorMessage={2}, Result={3}]", Success, Error, ErrorMessage, Result);
    }
}
