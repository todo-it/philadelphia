using System;
using System.Diagnostics.CodeAnalysis;

namespace ControlledByTests.Domain {
    public static class MagicsForTests {
        public const string TestChoiceParamName = "testName";
        public const string ValueToSend = "valueToSend";

        public const string ResultSpanId = "result";
        public const string ResultSpanReadyValue = "done";
        public const string RunClientSideTestBtnId = "runTest";

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum ClientSideFlows {
            HelloWorld,
            SerializationTest_Int,
            SerializationTest_String,
            SerializationTest_DateTimeUtc,
            SerializationTest_DateTimeLocal,
            SerializationTest_Long,
            SerializationTest_Decimal,
        }

        public static class Serialization {
            public class Scenario<T> {
                public readonly string DefaultTypedVal;
                public readonly T DefaultClientVal;
                public readonly T ServerAdd;

                public Scenario(
                    string defaultTypedVal,
                    T defaultClientVal,
                    T serverAdd) {

                    DefaultTypedVal = defaultTypedVal;
                    DefaultClientVal = defaultClientVal;
                    ServerAdd = serverAdd;
                }
            }

            public static readonly Scenario<int> Int = 
                new Scenario<int>("123", 123, 5);

            public static readonly Scenario<string> String = 
                new Scenario<string>("Boom", "Boom", "Abracadabra");

            public static readonly Scenario<long> Long = 
                new Scenario<long>("366", 366L, 112L);

            public static readonly Scenario<decimal> Decimal = 
                new Scenario<decimal>("13.3", 13.3M, 2.4M);

            public static readonly Scenario<DateTime> DateTime =
                new Scenario<DateTime>("2001-02-03 04:05:06", new DateTime(2001, 2, 3, 4, 5, 6), new DateTime(2001, 1, 1, 1, 1, 1));

            public static DateTime MidDate(DateTime x1, DateTime x2) {
                var diff = x2 - x1;
                var move = diff.TotalMilliseconds / 2;
                return x1.AddMilliseconds(move);
            }
        }
    }
}
