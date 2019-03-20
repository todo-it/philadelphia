using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public enum DecimalFormat {
        WithTwoDecPlaces=1,
        AsMoney=1,

        WithThreeDecPlaces=2,
        AsWeightInGrams=2,

        WithOneDecPlace=3,
        AsWeightInHectograms=3,

        AsNumber=4,

        WithFiveDecPlaces=5,

        WithZeroDecPlaces=6,
        WithFourDecPlaces=7
    }

    public class DecimalFormatExtensions {
        public static DecimalFormat GetWithPrecision(int afterComma) {
            switch (afterComma) {
                    case 0: return DecimalFormat.WithZeroDecPlaces;
                    case 1: return DecimalFormat.WithOneDecPlace;
                    case 2: return DecimalFormat.WithTwoDecPlaces;
                    case 3: return DecimalFormat.WithThreeDecPlaces;
                    case 4: return DecimalFormat.WithFourDecPlaces;
                    case 5: return DecimalFormat.WithFiveDecPlaces;
                    default: throw new Exception($"unsupported precision {afterComma}");
            }
        }
    }
}
