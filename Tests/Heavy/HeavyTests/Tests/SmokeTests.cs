using System;
using Xunit;
using ControlledByTests.Domain;
using Philadelphia.Server.Common;
using Philadelphia.Testing.DotNetCore;
using Philadelphia.Testing.DotNetCore.Selenium;
using Xunit.Abstractions;

namespace HeavyTests.Tests {
    public class SmokeTests {
        private readonly Action<string> _logger;
        const string someName = "somenamehere";
        
        public SmokeTests(ITestOutputHelper logger) {
            _logger = logger.WriteLine;
        }
        
        [Fact]
        public void HelloWorldTest_CyclesThroughWholeProgram() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                MagicsForTests.ClientSideFlows.HelloWorld, (assertX, server, browser) => {
                
                assertX.DialogIsVisibleInBrowser("Hello there");
                
                browser
                    .FindElementByXPath(XPathBuilder.Dialog("Hello there").InBody("/input[@type='text']"))
                    .SendKeys(someName);
                        
                assertX.NoServiceCallsMadeOnServer();
                
                browser
                    .FindElementByXPath(XPathBuilder.Dialog("Hello there").HasEnabledButtonAction("OK"))
                    .Click();
                
                assertX.DialogIsVisibleInBrowser("Server reply");
                
                assertX.InvocationsMadeOnServerAre(x => x.ResType == ResourceType.RegularPostService, () => {
                    var onBefore = FilterInvocation.OfMethod((IHelloWorldService x) => x.SayHello(someName));
                    var actualCall = ServiceCall.OfMethod((IHelloWorldService x) => x.SayHello(someName));
                    var onAfter = FilterInvocation.ExpectOnConnectionAfterFor(onBefore);

                    return new CsChoice<ServiceCall,FilterInvocation>[]{onBefore,actualCall,onAfter};
                });
                
                assertX.MatchesXPathInBrowser(
                    XPathBuilder
                        .Dialog("Server reply")
                        .HasReadOnlyLabel($"Hello {someName}. How are you?"));
            });
        }

        [Fact]
        public void StartsServer_GetsItsStartConfirmation() {
            new HeavyTestRunner(_logger).RunServerAndExecute(
                _ => {/*nothing here as I just want to sure that server confirmed its start*/});
        }
    }
}
