using System;

namespace Philadelphia.Web {
    public static class DayOfWeekExtensions {
        public static DayOfWeek GetNextDay(this DayOfWeek sinceDay) {
            switch (sinceDay) {
                case DayOfWeek.Monday: return DayOfWeek.Tuesday;
                case DayOfWeek.Tuesday: return DayOfWeek.Wednesday;
                case DayOfWeek.Wednesday: return DayOfWeek.Thursday;
                case DayOfWeek.Thursday: return DayOfWeek.Friday;
                case DayOfWeek.Friday: return DayOfWeek.Saturday;
                case DayOfWeek.Saturday: return DayOfWeek.Sunday;
                case DayOfWeek.Sunday: return DayOfWeek.Monday;
                default: throw new Exception("unsupported DayOfWeek");
            }
        }
    }
}
