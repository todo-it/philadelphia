using System.Threading.Tasks;

namespace Philadelphia.Common {
    public static class ReadWriteValueExtensions {
        public static async Task<Unit> Revalidate<T>(this IReadWriteValue<T> self, bool isUserChange = false, object sender = null) {
            return await self.DoChange(self.Value, isUserChange, sender);
        }
    
        public static void AddValidatorAndRevalidate<T>(this IReadWriteValue<T> self, Validate<T> validator) {
            self.Validate += validator;
            self.DoChange(self.Value, false, self);
        }

        /// <summary>shorthand for DoChange() that is: programmatic AND is allowed to store invalid values</summary>
        public static Task<Unit> DoProgrammaticChange<T>(this IReadWriteValue<T> self, T newValue) {
            return self.DoChange(newValue, false, null, false);
        }
    }
}
