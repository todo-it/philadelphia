using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;
// ReSharper disable InconsistentNaming bridge.net 15.7 deserialization workaround - property+backing field needs same name just with different case

namespace Philadelphia.Demo.SharedModel {
    public enum SomeTraitType {
        SomeVariantA = 1,
        SomeVariantB,
        SomeVariantC,
        SomeVariantD,
        AnotherVariant1,
        AnotherVariant2,
        AnotherVariant3,
        AnotherVariant4,
        AnotherVariant5,
        AnotherVariant6,
        PossibilityA,
        PossibilityB,
        PossibilityC,
        PossibilityD,
        PossibilityE,
        PossibilityF,
        PossibilityG,
        PossibilityH,
        PossibilityI,
        PossibilityJ,
        PossibilityK,
        PossibilityL,
        SomePossibility1,
        SomePossibility2,
        SomePossibility3,
        SomePossibility4,
        SomePossibility5
    }

    public class SomeDto : IHasSubscribeable {
        private readonly Subscribeable _subscribeable = new Subscribeable();
        
        public ISubscribeable Subscribeable => _subscribeable;
        
        public int Id {get; set; }

        private string someText;
        public string SomeText {
            get { return someText; }
            set {
                someText = value; 
                _subscribeable.Notify(nameof(SomeText));
            }
        }

        private int someNumber;
        public int SomeNumber {
            get { return someNumber; }
            set { someNumber = value;
                _subscribeable.Notify(nameof(SomeNumber));
            }
        }
        
        private bool someBool;
        public bool SomeBool {
            get { return someBool; }
            set { someBool = value;
                _subscribeable.Notify(nameof(SomeBool));
            }
        }

        private SomeTraitType someTrait ;
        public SomeTraitType SomeTrait {
            get { return someTrait; }
            set { someTrait = value;
                _subscribeable.Notify(nameof(SomeTrait));
            }
        }
        
        public override string ToString() {
            return $"<SomeDto id={Id} SomeText={SomeText} SomeNumber={SomeNumber} SomeBool={SomeBool} SomeTrait={SomeTrait}>";
        }
    }
}
