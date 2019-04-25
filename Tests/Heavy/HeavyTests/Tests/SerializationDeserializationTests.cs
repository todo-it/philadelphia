using System;
using System.Globalization;
using System.Threading;
using ControlledByTests.Domain;
using OpenQA.Selenium.Remote;
using Philadelphia.Common;
using Philadelphia.Testing.DotNetCore;
using Philadelphia.Testing.DotNetCore.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace HeavyTests.Tests {
    public class SerializationDeserializationTests {
        private static bool PollWait(Func<bool> condition, int polls = 5, int milliseconds = 500) {
            var interval = milliseconds / polls;
            for (var i = 0; i < polls; i++) {
                if (condition()) {
                    return true;
                }

                Thread.Sleep(interval);
            }

            return false;
        }
        private void TestSerialization(
            MagicsForTests.ClientSideFlows flow,
            Func<AssertX, ControlledServerController, RemoteWebDriver, (ServiceCall expectedCall, string expectedClientResultValue)> getExpectations) =>
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(flow,
                (assertX, server, browser) => {
                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestBtnId)
                        .Click();

                    Assert.True(PollWait(() => !browser.FindElementById(MagicsForTests.ResultSpanId).Text.Then(string.IsNullOrEmpty)));
                    var (expectedCall, expectedClientResultValue) = getExpectations(assertX, server, browser);
                    assertX.ServiceCallsMadeOnServerAre(expectedCall);
                    Assert.Equal(expectedClientResultValue, browser.FindElementById(MagicsForTests.ResultSpanId).Text);
                });

        private void TestSerialization(MagicsForTests.ClientSideFlows flow, ServiceCall expectedCall, string expectedClientResultValue) =>
            TestSerialization(flow, (x, y, z) => (expectedCall, expectedClientResultValue));

        private readonly Action<string> _logger;

        public SerializationDeserializationTests(ITestOutputHelper logger) {
            _logger = logger.WriteLine;
        }

        [Fact]
        public void TestInt() =>
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_Int,
                ServiceCall.OfMethod((ISerDeserService x) => x.ProcessInt(MagicsForTests.Serialization.Int.DefaultClientVal)),
                (MagicsForTests.Serialization.Int.DefaultClientVal +
                 MagicsForTests.Serialization.Int.ServerAdd).ToString(CultureInfo.InvariantCulture));

        [Fact]
        public void TestDateTimeUtc() {
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_DateTimeUtc,
                ServiceCall.OfMethod((ISerDeserService x) => x.ProcessDateTime(MagicsForTests.Serialization.DateTime.DefaultClientVal, true)),
                MagicsForTests.Serialization.MidDate(
                    MagicsForTests.Serialization.DateTime.DefaultClientVal,
                    MagicsForTests.Serialization.DateTime.ServerAdd).ToStringYyyyMmDdHhMm());
        }

        [Fact]
        public void TestDateTimeLocal() {
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_DateTimeLocal,
                (assert, server, browser) => {
                    var tzOffset =
                        Convert.ToInt32(
                        browser.ExecuteScript(
                            $"return new Date(new Date('{MagicsForTests.Serialization.DateTime.DefaultTypedVal}').toUTCString()).getTimezoneOffset() + ''"
                        ));
                    var receivedByServer =
                        MagicsForTests.Serialization.DateTime.DefaultClientVal.AddMinutes(tzOffset);

                    return (
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessDateTime(receivedByServer, false)),
                        MagicsForTests.Serialization.MidDate(
                            receivedByServer,
                            MagicsForTests.Serialization.DateTime.ServerAdd).ToStringYyyyMmDdHhMm());
                });
        }

        [Fact]
        public void TestString() =>
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_String,
                ServiceCall.OfMethod((ISerDeserService x) => x.ProcessString(MagicsForTests.Serialization.String.DefaultTypedVal)),
                (MagicsForTests.Serialization.String.DefaultTypedVal +
                 MagicsForTests.Serialization.String.ServerAdd));

        [Fact]
        public void TestDecimal() =>
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_Decimal,
                ServiceCall.OfMethod((ISerDeserService x) => x.ProcessDecimal(MagicsForTests.Serialization.Decimal.DefaultClientVal)),
                (MagicsForTests.Serialization.Decimal.DefaultClientVal +
                 MagicsForTests.Serialization.Decimal.ServerAdd).ToString(CultureInfo.InvariantCulture));

        [Fact]
        public void TestLong() =>
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_Long,
                ServiceCall.OfMethod((ISerDeserService x) => x.ProcessLong(MagicsForTests.Serialization.Long.DefaultClientVal)),
                (MagicsForTests.Serialization.Long.DefaultClientVal +
                 MagicsForTests.Serialization.Long.ServerAdd).ToString(CultureInfo.InvariantCulture));

    }
}
