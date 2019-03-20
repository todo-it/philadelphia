using System;
using System.Collections.Generic;
using System.Globalization;
using Philadelphia.Common;

// ReSharper disable InconsistentNaming

// Question: why not make I18nImpl, ItranslationProvider and all I18N related interfaces a nested interfaces of I18n static class?
namespace Philadelphia.Common {
    public static class I18n {
        // locking is used to assure atomicity of null checking inside TranslInstance and LocInstance
        private static volatile I18nImpl _translInstance;
        private static volatile ILocalizationImpl _locInstance;
        private static readonly object _lck = new object();

        private static I18nImpl TranslInstance {
            get {
				var cpy = _translInstance;
                if (cpy != null) {
                    return cpy;
                }
                lock (_lck) {
                    if (_translInstance == null) {
                        _translInstance = _translProvider();
                    }
                    return _translInstance;
                }
            }
        }
        
        private static ILocalizationImpl LocInstance {
            get {
                var cpy = _locInstance;
                if (cpy != null) {
                    return cpy;
                }
                lock (_lck) {
                    if (_locInstance == null) {
                        _locInstance = _locProvider();
                    }
                    return _locInstance;
                }
            }
        }

        private static Func<I18nImpl> _translProvider = () => new NoTranslationI18n();
        private static Func<ILocalizationImpl> _locProvider = () => new ToStringLocalization();
        
        public static void ConfigureImplementation(Func<I18nImpl> translProvider) {
            _translProvider = translProvider;
            lock (_lck) {
                _translInstance = null;
            }
        }

        public static void ConfigureImplementation(Func<ILocalizationImpl> locProvider) {
            _locProvider = locProvider;
            lock (_lck) {
                _locInstance = null;
            }
        }
        
        //not exposed as regular field so that users don't access to abuse it
        public static void ProcessCurrentImplementation(Action<I18nImpl> process) => 
            process(TranslInstance);

        public static string Localize(bool input, BoolFormat format) => 
            LocInstance.Localize(input, format);

        public static string Translate(string input) => 
            TranslInstance.Translate(input);

        public static string TranslateForLang(string input, CultureInfo lang) => 
            TranslInstance.TranslateForLang(input, lang.Name);

        public static string TranslateMaybeLang(string input, CultureInfo lang=null) => 
            lang == null 
                ? TranslInstance.Translate(input)
                : TranslInstance.TranslateForLang(input, lang.Name);

        public static string Localize(decimal input, DecimalFormat format) => 
            LocInstance.Localize(input, format);

        public static string Localize(int input) => 
            LocInstance.Localize(input);

        public static string Localize(DateTime input, DateTimeFormat format) => 
            LocInstance.Localize(input, format);

        public static decimal ParseDecimal(string s) => 
            LocInstance.ParseDecimal(s);

        public static decimal ParseDecimalWithoutLoss(string s, int precision) => 
            LocInstance.ParseDecimalWithoutLoss(s, precision);

        public static int ParseInt(string s) => 
            LocInstance.ParseInt(s);

        public static DayOfWeek GetFirstDayOfWeek() => 
            LocInstance.GetFirstDayOfWeek();

        public static string GetFullMonthName(int monthNo) => 
            LocInstance.GetFullMonthName(monthNo);

        public static string GetWeekDayAcronym(DayOfWeek dow) => 
            LocInstance.GetWeekDayAcronym(dow);

        public static IEnumerable<Tuple<string, DateTimeElement?>> GetDateTimeFormatForPrecision(DateTimeFormat inp) {
            //FIXME hardcoded Polish values...

            switch (inp) {
                case DateTimeFormat.Y:
                    return new [] {new Tuple<string, DateTimeElement?>(null, DateTimeElement.Year)};

                case DateTimeFormat.YM:
                    return new [] {
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Month),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Year)
                    };

                case DateTimeFormat.DateOnly:
                    return new [] {
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Day),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Month),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Year)
                    };

                case DateTimeFormat.YMDhm:
                    return new [] {
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Day),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Month),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Year),
                        new Tuple<string, DateTimeElement?>(" ", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Hour),
                        new Tuple<string, DateTimeElement?>(":", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Minute)
                    };

                case DateTimeFormat.YMDhms:
                    return new [] {
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Day),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Month),
                        new Tuple<string, DateTimeElement?>(".", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Year),
                        new Tuple<string, DateTimeElement?>(" ", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Hour),
                        new Tuple<string, DateTimeElement?>(":", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Minute),
                        new Tuple<string, DateTimeElement?>(":", null),
                        new Tuple<string, DateTimeElement?>(null, DateTimeElement.Second)
                    };

                default: throw new Exception("unsupported DateTimeFormat");
            }
        }
    }
}
