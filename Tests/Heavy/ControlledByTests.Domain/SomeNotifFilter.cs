namespace ControlledByTests.Domain {
    public class SomeNotifFilter {
        public bool DontAcceptMe {get; set;}
        public bool AcceptPositive {get; set;}
        public bool AcceptNegative {get; set;}
        public bool AcceptEven {get; set;}
        public bool AcceptOdd {get; set;}

        public override bool Equals(object obj) {
            if (!(obj is SomeNotifFilter)) {
                return false;
            }
            
            var o = (SomeNotifFilter)obj;

            return 
                DontAcceptMe == o.DontAcceptMe &&
                AcceptPositive == o.AcceptPositive &&
                AcceptNegative == o.AcceptNegative &&
                AcceptEven == o.AcceptEven &&
                AcceptOdd == o.AcceptOdd;
        }

        public override int GetHashCode() {
            return 
                (DontAcceptMe ? 1 : 0) + 
                (AcceptPositive ? 1 : 0) + 
                (AcceptNegative ? 1 : 0) + 
                (AcceptEven ? 1 : 0) + 
                (AcceptOdd ? 1 : 0);
        }

        public override string ToString() {
            return $"<SomeNotifFilter DontAcceptMe={DontAcceptMe} AcceptPositive={AcceptPositive} AcceptNegative={AcceptNegative} AcceptEven={AcceptEven} AcceptOdd={AcceptOdd}>";
        }
    }
}
