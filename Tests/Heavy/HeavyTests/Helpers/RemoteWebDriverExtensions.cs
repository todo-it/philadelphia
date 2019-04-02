using System;
using System.Runtime.CompilerServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace HeavyTests.Helpers {
    public static class RemoteWebDriverExtensions {
        private static readonly TimeSpan DefaultDomTimeout = TimeSpan.FromMilliseconds(5000);

        //HACK this is only for tests convenience
        //wrap into tuple to have reference
        private static readonly ConditionalWeakTable<RemoteWebDriver,Tuple<TimeSpan>> _defaultTimeout 
            = new ConditionalWeakTable<RemoteWebDriver, Tuple<TimeSpan>>();
        private static readonly object _defaultTimeoutLck = new object();

        public static void SetDefaultDomTimeout(this RemoteWebDriver self, TimeSpan maxWait) {
            lock (_defaultTimeoutLck) {
                _defaultTimeout.Add(self, Tuple.Create(maxWait));
            }
        }
        
        private static TimeSpan? GetDefaultDomTimeout(RemoteWebDriver self) {
            lock (_defaultTimeoutLck) {
                Tuple<TimeSpan> res;
                if (_defaultTimeout.TryGetValue(self, out res)) {
                    return res.Item1;
                }
                return null;
            }
        }

        public static IWebElement FindElementByXPath(
                this RemoteWebDriver self, XPathBuilder xpath, TimeSpan? maxWait = null) {

            //inspired by https://stackoverflow.com/questions/20798752/how-can-i-ask-the-selenium-webdriver-to-wait-for-few-seconds-after-sendkey
            var to = maxWait ?? GetDefaultDomTimeout(self) ?? DefaultDomTimeout;
            var waiter = new WebDriverWait(self, to);
            
            return waiter.Until(x => {
                try {
                    return ((RemoteWebDriver) x).FindElementByXPath(xpath.AsString());
                } catch(Exception) {
                    return null;
                }
            });
        }
    }
}
