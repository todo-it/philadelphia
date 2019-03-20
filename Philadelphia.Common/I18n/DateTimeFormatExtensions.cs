using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public static class DateTimeFormatExtensions {
        public static string GetRegexp(this DateTimeFormat format) {
            switch (format) {
                case DateTimeFormat.DateOnly: return @"^\d{4}\-\d{2}-\d{2}$";
                case DateTimeFormat.YMDhm: return @"^\d{4}\-\d{2}\-\d{2} \d{2}:\d{2}$";
                case DateTimeFormat.YMDhms: return @"^\d{4}\-\d{2}\-\d{2} \d{2}:\d{2}:\d{2}$";
                case DateTimeFormat.YM: return @"^\d{4}\-\d{2}$";
                case DateTimeFormat.Y:return @"^\d{4}$";
                default: throw new ArgumentException("BuildDateTimePicker got unexpected format");
            }
        }
        
        public static string AsCssClassName(this DateTimeFormat self) {
            return $"precision_{self.ToString()}";
        }
        
        public static string GetUserFriendlyPattern(this DateTimeFormat format) {
            switch (format) {
                case DateTimeFormat.DateOnly: return I18n.Translate("YYYY-MM-DD");
                case DateTimeFormat.YMDhm: return I18n.Translate("YYYY-MM-DD hh:mm");
                case DateTimeFormat.YMDhms: return I18n.Translate("YYYY-MM-DD hh:mm:ss");
                case DateTimeFormat.YM: return I18n.Translate("YYYY-MM");
                case DateTimeFormat.Y: return I18n.Translate("YYYY");
                default: throw new ArgumentException("BuildDateTimePicker got unexpected format");
            }
        }
    }
}
