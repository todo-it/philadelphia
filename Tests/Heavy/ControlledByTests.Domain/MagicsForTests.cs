using System;

namespace ControlledByTests.Domain {
    public class MagicsForTests {
        public const string TestChoiceParamName = "testName";

        public const string ResultSpanId = "result";
        public const string ResultSpanReadyValue = "done";

        public const int SerDeser_Int_TypedVal = 123;
        public const int SerDeser_Int_ClientAddVal = 3;
        public const int SerDeser_Int_ServerAddVal = 5;

        public static DateTime SerDeser_DateTime_ClientVal = new DateTime(2001,2,3, 4,5,6);
        public static string SerDeser_DateTime_ClientTypedVal = "2001-02-03 04:05:06";
        public const int SerDeser_DateTime_ClientAddDays = 3;
        public const int SerDeser_DateTime_ServerAddDays = 5;

        public const string SerDeser_String_TypedVal = "Boom";
        public const string SerDeser_String_ServerAddSuffix = "Abracadabra";
        public const string SerDeser_String_ClientAddSuffix = "HocusPocus";
    }
}
