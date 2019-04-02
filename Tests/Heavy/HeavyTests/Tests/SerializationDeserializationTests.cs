using System;
using System.Threading;
using ControlledByTests.Api;
using ControlledByTests.Domain;
using HeavyTests.Helpers;
using Philadelphia.Common;
using Xunit;
using Xunit.Abstractions;

namespace HeavyTests.Tests {
    public class SerializationDeserializationTests {
        private readonly Action<string> _logger;
        
        public SerializationDeserializationTests(ITestOutputHelper logger) {
            _logger = logger.WriteLine;
        }

        [Fact]
        public void TestInt() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                ClientSideFlows.SerDeser_Int, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.SerDeser_Int_TypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessInt(MagicsForTests.SerDeser_Int_TypedVal)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        (
                            MagicsForTests.SerDeser_Int_TypedVal +
                            MagicsForTests.SerDeser_Int_ServerAddVal+
                            MagicsForTests.SerDeser_Int_ClientAddVal) +"");
                });
        }

        [Fact]
        public void TestDateTimeUtc() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                ClientSideFlows.SerDeser_DateTimeUtc, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.SerDeser_DateTime_ClientTypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessDateTime(MagicsForTests.SerDeser_DateTime_ClientVal, true)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        MagicsForTests.SerDeser_DateTime_ClientVal
                            .AddDays(MagicsForTests.SerDeser_DateTime_ServerAddDays)
                            .AddDays(MagicsForTests.SerDeser_DateTime_ClientAddDays)
                            .ToStringYyyyMmDdHhMm()
                        );
                });
        }
    }
}
