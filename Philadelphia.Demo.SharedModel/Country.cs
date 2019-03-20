using System;

namespace Philadelphia.Demo.SharedModel {
    public enum Country {
        Germany,
        France,
        Canada,
        USA,
        SouthAfrica,
        Tunisia
    }

    public static class CountryExtensions {
        public static Continent GetContinent(this Country self) {
            switch (self) {
                case Country.Germany: return Continent.Europe;
                case Country.France: return Continent.Europe;
                case Country.Canada: return Continent.NorthAmerica;
                case Country.USA: return Continent.NorthAmerica;
                case Country.SouthAfrica: return Continent.Africa;
                case Country.Tunisia: return Continent.Africa;
                default: throw new Exception("unsupported country");
            }
        }
    }
}
