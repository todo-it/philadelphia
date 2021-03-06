﻿using System;
using System.Globalization;
using ControlledByTests.Domain;
using OpenQA.Selenium.Remote;
using Philadelphia.Common;
using Philadelphia.Testing.DotNetCore;
using Philadelphia.Testing.DotNetCore.Selenium;
using Xunit;
using Xunit.Abstractions;

namespace HeavyTests.Tests {
    public class SerializationDeserializationTests {
        private delegate (ServiceCall expectedCall, string expectedClientResultValue) 
            GetExpectations(
                ClientServerAssert assert, ControlledServerController server, RemoteWebDriver browser);

        private void TestSerialization(MagicsForTests.ClientSideFlows flow, GetExpectations getExpectations) =>
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(flow,
                (assert, server, browser) => {
                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestBtnId)
                        .Click();

                    Assert.True(Poll.Wait(() => !browser.FindElementById(MagicsForTests.ResultSpanId).Text.Then(string.IsNullOrEmpty)));
                    var (expectedCall, expectedClientResultValue) = getExpectations(assert, server, browser);
                    assert.ServiceCallsMadeOnServerAre(expectedCall);
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
                ServiceCall.OfMethod((ISerDeserService x) => x.ProcessDateTime(MagicsForTests.Serialization.DateTimeUTC.DefaultClientVal, true)),
                MagicsForTests.Serialization.MidDate(
                    MagicsForTests.Serialization.DateTimeUTC.DefaultClientVal,
                    MagicsForTests.Serialization.DateTimeUTC.ServerAdd).ToStringYyyyMmDdHhMm());
        }

        [Fact]
        public void TestDateTimeLocal() {
            TestSerialization(
                MagicsForTests.ClientSideFlows.SerializationTest_DateTimeLocal,
                (assert, server, browser) => {
                    var tzOffset =
                        Convert.ToInt32(
                            browser.ExecuteScript(
                                $"return new Date(new Date('{MagicsForTests.Serialization.DateTimeLocal.DefaultTypedVal}').toUTCString()).getTimezoneOffset() + ''"
                            ));
                    var receivedByServer =
                        MagicsForTests.Serialization.DateTimeLocal.DefaultClientVal.AddMinutes(tzOffset);

                    return (
                        ServiceCall.OfMethod((ISerDeserService x) => x.ProcessDateTime(receivedByServer, false)),
                        MagicsForTests.Serialization.MidDate(
                            receivedByServer,
                            MagicsForTests.Serialization.DateTimeLocal.ServerAdd).ToStringYyyyMmDdHhMm());
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
