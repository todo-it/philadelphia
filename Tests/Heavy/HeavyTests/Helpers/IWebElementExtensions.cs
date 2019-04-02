using OpenQA.Selenium;

namespace HeavyTests.Helpers {
    public static class IWebElementExtensions {
        public static bool IsMatched(this IWebElement self) {
            return !self.Size.IsEmpty;
        }

        public static IWebElement ClearFluent(this IWebElement self) {
            self.Clear();
            return self;
        }
    }
}
