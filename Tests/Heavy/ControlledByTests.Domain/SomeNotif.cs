namespace ControlledByTests.Domain {
    public class SomeNotif {
        public int Num {get; set;}
        public string Prop {get; set;}
        
        public override bool Equals(object obj) {
            if (!(obj is SomeNotif)) {
                return false;
            }
            
            var o = (SomeNotif)obj;
            return 
                Num == o.Num &&
                (Prop == null && o.Prop == null || Prop != null && Prop.Equals(o.Prop));
        }

        public override int GetHashCode() {
            return Num + (Prop?.GetHashCode() ?? 0);
        }
        
        public override string ToString() {
            return $"<SomeNotif Num={Num} Prop={Prop}>";
        }
    }
}
