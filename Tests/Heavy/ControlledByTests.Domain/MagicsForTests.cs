using System;

namespace ControlledByTests.Domain {
    public static class MagicsForTests {
        public const string TestChoiceParamName = "testName";

        public const string ResultSpanId = "result";
        public const string ResultSpanReadyValue = "done";

        public enum ClientSideFlows {
            HelloWorld,
            SerDeser_Int,
            SerDeser_String,
            SerDeser_DateTimeUtc,
            SerDeser_DateTimeLocal,
            SerDeser_Long,
        }

        public static class Serialization {
            public static class Int {
                public const ClientSideFlows Flow = ClientSideFlows.SerDeser_Int;
                public const int TypedVal = 123;
                public const int ClientAddVal = 3;
                public const int ServerAddVal = 5;
            }

            public static class DateTime {
                public const ClientSideFlows FlowLocal = ClientSideFlows.SerDeser_DateTimeLocal;
                public const ClientSideFlows FlowUtc = ClientSideFlows.SerDeser_DateTimeUtc;
                public static System.DateTime ClientVal = new System.DateTime(2001,2,3, 4,5,6);
                public static string ClientTypedVal = "2001-02-03 04:05:06";
                public const int ClientAddDays = 3;
                public const int ServerAddDays = 5;
            }

            public static class String {
                public const ClientSideFlows Flow = ClientSideFlows.SerDeser_String;
                public const string TypedVal = "Boom";
                public const string ServerAddSuffix = "Abracadabra";
                public const string ClientAddSuffix = "HocusPocus";
            }

            public static class Long {
                public const ClientSideFlows Flow = ClientSideFlows.SerDeser_Long;
                public const string TypedVal = "366";
                public const long ClientVal = 366L;
                public const long ServerAdd = 112L;
                public const long ClientAdd = 501L;
            }
        }
    }
}
