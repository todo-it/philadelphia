using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace Philadelphia.Common {
    public class ToStringLocalization : ILocalizationImpl,ICurrentCultureSwitchedListener {
        private CultureInfo _currentCulture;
        private char _decPointChar;
        private char[] _goodDecimalChars;
        private const char NonbreakableSpaceChar = '\xA0';
        private const string NonbreakableSpaceStr = "\xA0";

        public ToStringLocalization(string forcedCulture=null) {
            _currentCulture = 
                forcedCulture == null ? CultureInfo.InvariantCulture : new CultureInfo(forcedCulture);
        
            _decPointChar = Convert.ToChar(_currentCulture.NumberFormat.NumberDecimalSeparator);
            Logger.Debug(GetType(), "current culture = {0} decPointChar={1}", _currentCulture, _decPointChar);
            
            UpdateGoodDecimalChars();
        }

        private void UpdateGoodDecimalChars() {
            _goodDecimalChars = new [] {
                '0','1','2','3','4','5','6','7','8','9',
                '-',' ', NonbreakableSpaceChar,
                _currentCulture.NumberFormat.NumberGroupSeparator[0],
                _currentCulture.NumberFormat.NumberDecimalSeparator[0] };
        }

        public string Localize(bool input, BoolFormat format) {
            switch (format) {
                case BoolFormat.UnicodeBallotBox: return LocalizationUtil.BoolToUnicodeCheckbox(input);
                case BoolFormat.YesNo: return input ? I18n.Translate("Yes") : I18n.Translate("No");
                case BoolFormat.TrueFalse: return input ? I18n.Translate("True") : I18n.Translate("False");
                default: throw new Exception("unsupported BoolFormat");
            }
        }

        public string Localize(decimal input, DecimalFormat format) {
            var roundRule = MidpointRounding.AwayFromZero;
            switch (format) {
                case DecimalFormat.AsWeightInGrams: return Math.Round(input, 3, roundRule).ToString("n3", _currentCulture);
                case DecimalFormat.AsWeightInHectograms: return Math.Round(input, 1, roundRule).ToString("n1", _currentCulture);
                case DecimalFormat.AsMoney: return Math.Round(input, 2, roundRule).ToString("n2", _currentCulture);
                case DecimalFormat.AsNumber: return Math.Round(input, 2, roundRule).ToString("n2", _currentCulture);
                case DecimalFormat.WithFiveDecPlaces: return Math.Round(input, 5, roundRule).ToString("n5", _currentCulture);
                
                case DecimalFormat.WithZeroDecPlaces: return Math.Round(input, 0, roundRule).ToString("n0", _currentCulture);
                case DecimalFormat.WithFourDecPlaces: return Math.Round(input, 4, roundRule).ToString("n4", _currentCulture);

                default: throw new ArgumentException("Localize decimal unknown mode");
            }
        }

        public string Localize(int input) {
            return input.ToString();
        }
        
        private bool SupposedlyDecimalContainsUnknownChars(string s) {
            return s.Any(x => !_goodDecimalChars.Contains(x));
        }

        public decimal ParseDecimal(string s) {
            var inp = s
                .Replace(NonbreakableSpaceStr, "") //nonbreakable space
                .Replace(" ", "");

            if (SupposedlyDecimalContainsUnknownChars(s)) {
                throw new FormatException(I18n.Translate("Illegal characters in number"));
            }

            //still some Bridge bugs
            inp = inp
                .Replace(_currentCulture.NumberFormat.NumberGroupSeparator, "")
                .Replace(_currentCulture.NumberFormat.NumberDecimalSeparator, ".");
            return Convert.ToDecimal(inp, CultureInfo.InvariantCulture);
        }

        public decimal ParseDecimalWithoutLoss(string s, int precision) {
            var inp = s
                .Replace(NonbreakableSpaceStr, "") //nonbreakable space
                .Replace(" ", "");
   
            if (SupposedlyDecimalContainsUnknownChars(s)) {
                throw new FormatException(I18n.Translate("Illegal characters in number"));
            }

            //still some Bridge bugs
            inp = inp
                .Replace(_currentCulture.NumberFormat.NumberGroupSeparator, "")
                .Replace(_currentCulture.NumberFormat.NumberDecimalSeparator, ".");
            var beforeAndAfter = inp.Split('.'); //Chrome ignores xx in '<html lang=xx>' and overrides '<input type=number>' decimal character with one taken from browser's language/system locale
            
            if (beforeAndAfter.Length < 1) {
                throw new Exception("bug - got zero tokens");
            }

            if (precision < 0) {
                throw new Exception("bug - incorrect precision");
            }

            if (precision == 0) {
                if (beforeAndAfter.Length > 1) {
                    throw new Exception(I18n.Translate("Decimal part is not allowed"));
                }

                //beforeAndAfter.Length must be 1 since here

                return Convert.ToDecimal(beforeAndAfter[0], _currentCulture);
            }
            
            //precision must be > 0 since here

            if (beforeAndAfter.Length > 2) {
                throw new Exception(I18n.Translate("Incorrect number format"));
            }

            if (beforeAndAfter.Length == 1) {
                //no decimal part
                return Convert.ToDecimal(beforeAndAfter[0], _currentCulture);
            }

            var actualFactPrecision = beforeAndAfter[1].TrimEnd('0').Length;
            if (actualFactPrecision > precision) {
                throw new Exception(string.Format(
                    I18n.Translate("Error: allowed {0} digits after decimal point"), precision));
            }

            return Convert.ToDecimal(
                beforeAndAfter[0] + "." + beforeAndAfter[1], 
                CultureInfo.InvariantCulture);
        }

        public int ParseInt(string s) {
            return Convert.ToInt32(s, _currentCulture);
        }

        public DayOfWeek GetFirstDayOfWeek() {
            var result = _currentCulture.DateTimeFormat.FirstDayOfWeek;
            return (DayOfWeek)result; //needed in bridge.net
        }

        public string GetFullMonthName(int monthNo) {
            return new DateTime(2000, monthNo, 1).ToString("MMMM", _currentCulture);
        }

        public string GetWeekDayAcronym(DayOfWeek dow) {
            return _currentCulture.DateTimeFormat.AbbreviatedDayNames[(int)dow];
        }

        public string Localize(DateTime i, DateTimeFormat format) {
            switch (format) {
                case DateTimeFormat.DateOnly:
                    return string.Format("{0:0000}-{1:00}-{2:00}", i.Year, i.Month, i.Day);
                case DateTimeFormat.YMDhm:
                     return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}", i.Year, i.Month, i.Day, i.Hour, i.Minute);
                case DateTimeFormat.YMDhms:
                    return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}", i.Year, i.Month, i.Day, i.Hour, i.Minute, i.Second);
                case DateTimeFormat.YM:
                    return string.Format("{0:0000}-{1:00}", i.Year, i.Month);
                case DateTimeFormat.Y:
                    return string.Format("{0:0000}", i.Year);
                default: return i.ToString();
            }
        }

        public void OnSwitchedTo(string cultureName) {
            _currentCulture = new CultureInfo(cultureName);
            _decPointChar = Convert.ToChar(_currentCulture.NumberFormat.NumberDecimalSeparator);
            UpdateGoodDecimalChars();
        }
    }
}
