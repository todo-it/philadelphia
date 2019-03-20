using System;

namespace Philadelphia.Common {
    public static class DoubleExtensions {
        public static bool AreApproximatellyTheSame(this double self, double other, double epsilon = Double.Epsilon) {
            //FIXME: study and if clear implement https://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
            var diff = Math.Abs(self-other);
            return diff <= epsilon;
        }
    }
}
