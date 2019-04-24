using System;
using System.Diagnostics.CodeAnalysis;

namespace ControlledByTests.Domain {
    public static class MagicsForTests {
        public const string TestChoiceParamName = "testName";

        public const string ResultSpanId = "result";
        public const string ResultSpanReadyValue = "done";

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
            public static class Int {
                public const ClientSideFlows Flow = ClientSideFlows.SerializationTest_Int;
                public const int TypedVal = 123;
                public const int ClientAddVal = 3;
                public const int ServerAddVal = 5;
            }

            public static class DateTime {
                public const ClientSideFlows FlowLocal = ClientSideFlows.SerializationTest_DateTimeLocal;
                public const ClientSideFlows FlowUtc = ClientSideFlows.SerializationTest_DateTimeUtc;
                public static System.DateTime ClientVal = new System.DateTime(2001,2,3, 4,5,6);
                public static string ClientTypedVal = "2001-02-03 04:05:06";
                public const int ClientAddDays = 3;
                public const int ServerAddDays = 5;
            }

            public static class String {
                public const ClientSideFlows Flow = ClientSideFlows.SerializationTest_String;
                public const string TypedVal = "Boom";
                public const string ServerAddSuffix = "Abracadabra";
                public const string ClientAddSuffix = "HocusPocus";
            }

            public static class Long {
                public const ClientSideFlows Flow = ClientSideFlows.SerializationTest_Long;
                public const string TypedVal = "366";
                public const long ClientVal = 366L;
                public const long ServerAdd = 112L;
                public const long ClientAdd = 501L;
            }

            public static class Decimal {
                public const ClientSideFlows Flow = ClientSideFlows.SerializationTest_Decimal;
                public const string TypedVal = "366";
                public const long ClientVal = 366L;
                public const long ServerAdd = 112L;
                public const long ClientAdd = 501L;
            }
        }
    }
}
