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
                        ServiceCall.OfMethod((ISerDeserService x) => 
                            x.ProcessDateTime(MagicsForTests.SerDeser_DateTime_ClientVal, true)));

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
        
        [Fact]
        public void TestDateTimeLocal() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                ClientSideFlows.SerDeser_DateTimeLocal, (assertX, server, browser) => {
                    var tzOffset = Convert.ToInt32(
                        browser.ExecuteScript(
                            $"return new Date(new Date('{MagicsForTests.SerDeser_DateTime_ClientTypedVal}').toUTCString()).getTimezoneOffset() + ''"));

                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.SerDeser_DateTime_ClientTypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    var serversExpectedParamVal 
                        = MagicsForTests.SerDeser_DateTime_ClientVal.AddMinutes(tzOffset);
                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => 
                            x.ProcessDateTime(serversExpectedParamVal, false)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        MagicsForTests.SerDeser_DateTime_ClientVal
                            .AddMinutes(tzOffset)
                            .AddDays(MagicsForTests.SerDeser_DateTime_ServerAddDays)
                            .AddDays(MagicsForTests.SerDeser_DateTime_ClientAddDays)
                            .ToStringYyyyMmDdHhMm()
                    );
                });
        }

        [Fact]
        public void TestString() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                ClientSideFlows.SerDeser_String, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.SerDeser_String_TypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessString(MagicsForTests.SerDeser_String_TypedVal)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        (
                            MagicsForTests.SerDeser_String_TypedVal +
                            MagicsForTests.SerDeser_String_ServerAddSuffix+
                            MagicsForTests.SerDeser_String_ClientAddSuffix) +"");
                });
        }
    }
}
