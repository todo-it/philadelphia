namespace Philadelphia.Web {
    public enum DateTimePickerMode {
        Sole,
        From,
        To
    }

    public static class DateTimePickerModeExtensions {
        public static string AsCssClassName(this DateTimePickerMode self) {
            return $"DateTimePickerMode_{self.ToString()}";
        }
    }
}
