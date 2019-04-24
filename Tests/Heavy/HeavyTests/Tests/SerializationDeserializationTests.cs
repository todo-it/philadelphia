using System;
using System.Threading;
using ControlledByTests.Domain;
using Philadelphia.Common;
using Philadelphia.Testing.DotNetCore;
using Philadelphia.Testing.DotNetCore.Selenium;
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
                MagicsForTests.Serialization.Int.Flow, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.Serialization.Int.TypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessInt(MagicsForTests.Serialization.Int.TypedVal)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        (
                            MagicsForTests.Serialization.Int.TypedVal +
                            MagicsForTests.Serialization.Int.ServerAddVal+
                            MagicsForTests.Serialization.Int.ClientAddVal) +"");
                });
        }

        [Fact]
        public void TestDateTimeUtc() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                MagicsForTests.Serialization.DateTime.FlowUtc, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.Serialization.DateTime.ClientTypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => 
                            x.ProcessDateTime(MagicsForTests.Serialization.DateTime.ClientVal, true)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        MagicsForTests.Serialization.DateTime.ClientVal
                            .AddDays(MagicsForTests.Serialization.DateTime.ServerAddDays)
                            .AddDays(MagicsForTests.Serialization.DateTime.ClientAddDays)
                            .ToStringYyyyMmDdHhMm()
                        );
                });
        }
        
        [Fact]
        public void TestDateTimeLocal() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                MagicsForTests.Serialization.DateTime.FlowLocal, (assertX, server, browser) => {
                    var tzOffset = Convert.ToInt32(
                        browser.ExecuteScript(
                            $"return new Date(new Date('{MagicsForTests.Serialization.DateTime.ClientTypedVal}').toUTCString()).getTimezoneOffset() + ''"));

                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.Serialization.DateTime.ClientTypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    var serversExpectedParamVal 
                        = MagicsForTests.Serialization.DateTime.ClientVal.AddMinutes(tzOffset);
                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => 
                            x.ProcessDateTime(serversExpectedParamVal, false)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        MagicsForTests.Serialization.DateTime.ClientVal
                            .AddMinutes(tzOffset)
                            .AddDays(MagicsForTests.Serialization.DateTime.ServerAddDays)
                            .AddDays(MagicsForTests.Serialization.DateTime.ClientAddDays)
                            .ToStringYyyyMmDdHhMm()
                    );
                });
        }

        [Fact]
        public void TestString() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                MagicsForTests.Serialization.String.Flow, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.Serialization.String.TypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessString(MagicsForTests.Serialization.String.TypedVal)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        (
                            MagicsForTests.Serialization.String.TypedVal +
                            MagicsForTests.Serialization.String.ServerAddSuffix+
                            MagicsForTests.Serialization.String.ClientAddSuffix) +"");
                });
        }

        [Fact]
        public void TestLong() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                MagicsForTests.Serialization.Long.Flow, (assertX, server, browser) => {
                    browser
                        .FindElementByXPath(XPathBuilder.Custom("//input"))
                        .ClearFluent()
                        .SendKeys(MagicsForTests.Serialization.Long.TypedVal +"\t");
                    
                    Thread.Sleep(Philadelphia.Web.Magics.ValidationTriggerDelayMilisec*2);

                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessLong(MagicsForTests.Serialization.Long.ClientVal)));

                    assertX.MatchesXPathInBrowser(
                        XPathBuilder.Custom($"//span[@id='{MagicsForTests.ResultSpanId}' and text() = '{MagicsForTests.ResultSpanReadyValue}']"));

                    assertX.InputHasValue(
                        XPathBuilder.Custom("//input"),
                        (
                            MagicsForTests.Serialization.Long.ClientVal +
                            MagicsForTests.Serialization.Long.ServerAdd+
                            MagicsForTests.Serialization.Long.ClientAdd) +"");
                });
        }
    }
}
