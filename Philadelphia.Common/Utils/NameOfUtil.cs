namespace Philadelphia.Common {
    public static class NameOfUtil {
        //TODO bridgedotnet 16.7 incorrectly gives nameof(propname) as "propname"
        public static string FixPropertyName(string rawPropertyName) {
            return rawPropertyName.Substring(0,1).ToUpper() + rawPropertyName.Substring(1);
        }
    }
}
