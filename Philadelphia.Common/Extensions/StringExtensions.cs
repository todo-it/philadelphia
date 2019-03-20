using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Philadelphia.Common;

namespace Philadelphia.Common {
    public static class StringExtensions {
        private static readonly Regex FromIso8601 = new Regex(@"^(\d{4})\-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})$");

        public static string EscapeHtml(string unsafeText) {
            return unsafeText
                .Replace("&", "&amp;")
                .Replace(">", "&gt;")
                .Replace("<", "&lt;");
        }

        // BUG: Datetime.TryParseExact fails to compile in Bridge
        // https://deck.net/e58376e7e4d67481d7bdaf31914514d8
        /// <summary>
        /// convert from format yyyy-MM-ddTHH:MM:ss
        /// </summary>
        /// <param name="inp"></param>
        /// <returns></returns>
        public static DateTime? FromIso8601LikeToDateOrNull(this string inp) {
            var matcher = FromIso8601.Match(inp);
            if (!matcher.Success) {
                return null;
            }

            return new DateTime(
                Convert.ToInt32(matcher.Groups[1].Value), 
                Convert.ToInt32(matcher.Groups[2].Value), 
                Convert.ToInt32(matcher.Groups[3].Value),
                Convert.ToInt32(matcher.Groups[4].Value), 
                Convert.ToInt32(matcher.Groups[5].Value), 
                Convert.ToInt32(matcher.Groups[6].Value)
            );
        }

        public static string TillLastOccurenceOfOrEverything(this string inp, string till) {
            var i = inp.LastIndexOf(till); //TODO StringComparison seems to be missing in bridge.net
            return i < 0 ? inp : inp.Substring(0, i);
        }

        public static string TillFirstOccurenceOfOrEverything(this string inp, string till) {
            var i = inp.IndexOf(till, StringComparison.InvariantCulture);
            return i < 0 ? inp : inp.Substring(0, i);
        }

        public static string TillFirstNewLineOrEverything(this string inp) {
			return TillFirstOccurenceOfOrEverything(inp, "\n");
		}
        
        [Obsolete("don't use it until bridge issue 3759 is resolved")]
        public static string MessageFormat(this string self, params object[] args) {
            return string.Format(self, args);
        }

        public static string Inverse(this string self) {
            return self.Reversed();
        }

        public static string Reversed(this string self) {
            var result = self.ToCharArray();
            Array.Reverse(result);
            return new string(result);
        }

        public static string TruncateIfLonger(this string self, int maxLength, bool withElipsis = false) {
            if (!withElipsis) {
                return self.Length > maxLength ? self.Substring(0, maxLength) : self;
            }
            
            return self.Length > maxLength ? self.Substring(0, maxLength-3)+"..." : self;
        }

        public static string PadEqualy(this string self, char padChar, int toLen) {
            var needed = toLen - self.Length;
            return new string(padChar, needed/2) + self + new string(padChar, needed - needed/2);
        }

        public static bool EqualsCaseInsensitive(this string self, string second) {
            if (self == null) {
                return second == null;
            }

            return self.Equals(second, StringComparison.OrdinalIgnoreCase);
        }
        
        public static bool ContainsCaseInsensitive(this string self, string second) {
            return self.ToLower().Contains(second.ToLower());
        }

        public static bool StartsWithCaseInsensitive(this string self, string second) {
            return self.ToLower().StartsWith(second.ToLower());
        }

        public static bool EndsWithCaseInsensitive(this string self, string second) {
            return self.ToLower().EndsWith(second.ToLower());
        }

        public static List<string> GetMatchedGroups(this string self, string regexp) {
            return Regex.Match(self, regexp).Groups.Cast<Group>().Select(x => x.Value).ToList();
        }
    }
}
