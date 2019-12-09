using System;

namespace Philadelphia.Common {
    public static class DateTimeExtensions {
        public static string ToStringYyyyMmDdHhMm(this DateTime inp) {
            return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}", inp.Year, inp.Month, inp.Day, inp.Hour, inp.Minute);
        }

        public static string ToStringYyyyMmDdHhMmSs(this DateTime inp) {
            return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}", inp.Year, inp.Month, inp.Day, inp.Hour, inp.Minute, inp.Second);
        }

        public static DateTime WhenNull(this DateTime? inp, DateTime defaultValue) {
            return inp ?? defaultValue;
        }
        
        public static DateTime BuildMonth(this DateTime mnth) {
            return new DateTime(mnth.Year, mnth.Month, 1);
        }

        public static DateTime BuildLastMomentOfMonth(this DateTime mnth) {
            return BuildNextMonth(mnth).AddSeconds(-1);
        }

        public static DateTime BuildLastMomentOfDay(DateTime inp) {
            return inp.Date.AddDays(1).Date.AddSeconds(-1);
        }

        public static DateTime BuildLastMomentOfToday() {    
            return DateTime.Now.AddDays(1).Date.AddSeconds(-1);
        }

        public static DateTime BuildThisMonthOnLastDay() {
            return BuildNextMonth(DateTime.Now).AddSeconds(-1);
        }

        public static DateTime BuildThisMonth(DateTime? since = null) {
            var result = since ?? DateTime.Now;
            return new DateTime(result.Year, result.Month, 1);
        }
        
        public static DateTime BuildSoonestMidnight() {
            var result = DateTime.Now;
            return result.Date.AddDays(1);
        }
        
        public static DateTime BuildYesterdayMidnight() {
            return DateTime.Now.AddDays(-1).WithTime(0,0,0);
        }

        public static DateTime BuildFormerMonth(DateTime? since = null) {
            var result = since ?? DateTime.Now;
            return result.Month <= 1 ? new DateTime(result.Year-1, 12, 1) : new DateTime(result.Year, result.Month-1, 1);
        }

        public static DateTime BuildNextMonth(DateTime? since = null) {
            var result = since ?? DateTime.Now;
            return result.Month >= 12 ? new DateTime(result.Year+1, 1, 1) : new DateTime(result.Year, result.Month+1, 1);
        }

        public static int BuildLastDayOfMonth(this DateTime self) {
            return BuildNextMonth(self).AddDays(-1).Day;
        }
        
        public static DateTime SmallestDate() {
            return new DateTime(2000,1,1,0,0,0,0);
        }
        
        public static bool IsSameMonth(DateTime? fst, DateTime? snd) {
            return 
                fst.HasValue && snd.HasValue && 
                fst.Value.Year == snd.Value.Year && 
                fst.Value.Month == snd.Value.Month;
        }
        
        public static bool IsSameDay(DateTime fst, DateTime snd) {
            return 
                fst.Year == snd.Year && 
                fst.Month == snd.Month &&
                fst.Day == snd.Day;
        }

        public static bool IsToday(DateTime fst) {
            return IsSameDay(DateTime.Now, fst);
        }

        public static DateTime WithTime(this DateTime self, int hours, int minutes, int seconds) {
            return new DateTime(self.Year, self.Month, self.Day, hours, minutes, seconds);
        }

        public static bool IsCurrentMonth(this DateTime self) {
            var now = DateTime.Now;
            return self.Year == now.Year && self.Month == now.Month;
        }

        public static bool IsSameMonthAs(this DateTime self, DateTime other) {
            return self.Year == other.Year && self.Month == other.Month;
        }

        public static DateTime FromMilisecondsSinceUnixEpoch(long milisecs) {
            return new DateTime(1970, 1, 1).AddMilliseconds(milisecs);
        }

        public static long ToMilisecondsSinceUnixEpoch(this DateTime self) {
            return (long)(self - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}
