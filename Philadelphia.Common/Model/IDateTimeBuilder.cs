using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface IDateTimeBuilder {
        DateTime Build(
            int year, int? month=null, int? day=null, 
            int? hour=null, int? minute=null, int? seconds=null);
    }

    public static class DateTimeBuilderExtensions {
        public static DateTime BuildFrom(
                this IDateTimeBuilder self, DateTime i, DateTimeFormat frm) {

            switch (frm) {
                case DateTimeFormat.DateOnly: 
                    return self.Build(i.Year, i.Month, i.Day);
                case DateTimeFormat.Y: 
                    return self.Build(i.Year);
                case DateTimeFormat.YM: 
                    return self.Build(i.Year, i.Month);
                case DateTimeFormat.YMDhm: 
                    return self.Build(i.Year, i.Month, i.Day, i.Hour, i.Minute);
                case DateTimeFormat.YMDhms:
                    return self.Build(i.Year, i.Month, i.Day, i.Hour, i.Minute, i.Second);
                default: throw new Exception("unknown DateTimeFormat");
            }
        }
    }

    public enum DefaultTime {
        BeginOfDay,
        EndOfDay,
        Custom
    }

    /// <summary>simple implementation for web (no proper locking)</summary>
    public class LocalDateTimeBuilder : IDateTimeBuilder {
        private readonly DefaultTime _defTime;
        private readonly Func<DateTime, DateTime> _adaptDate;
        private static LocalDateTimeBuilder _instanceBod,_instanceEod;

        public static IDateTimeBuilder InstanceBeginOfDay { get {
                if (_instanceBod == null) {
                    _instanceBod = new LocalDateTimeBuilder(DefaultTime.BeginOfDay, null);
                }
                return _instanceBod;
            } }
        
        public static IDateTimeBuilder InstanceEndOfDay { get {
            if (_instanceEod == null) {
                _instanceEod = new LocalDateTimeBuilder(DefaultTime.EndOfDay, null);
            }
            return _instanceEod;
        } }

        public static LocalDateTimeBuilder Custom(Func<DateTime,DateTime> adaptDate) {
            return new LocalDateTimeBuilder(DefaultTime.Custom, adaptDate);
        }

        private LocalDateTimeBuilder(DefaultTime defTime, Func<DateTime,DateTime> adaptDate) {
            _defTime = defTime;
            _adaptDate = adaptDate;
        }
        
        public DateTime Build(
                int year, int? month = null, int? day = null, 
                int? hour = null, int? minute = null, int? seconds = null) {

            switch (_defTime) {
                case DefaultTime.Custom:
                    return _adaptDate(!hour.HasValue 
                        ? new DateTime(year, month ?? 1, day ?? 1)
                        : new DateTime(year, month ?? 1, day ?? 1, 
                            hour.Value, minute ?? 0, seconds ?? 0));

                case DefaultTime.BeginOfDay:
                    return !hour.HasValue 
                        ? new DateTime(year, month ?? 1, day ?? 1)
                        : new DateTime(year, month ?? 1, day ?? 1, 
                            hour.Value, minute ?? 0, seconds ?? 0);

                case DefaultTime.EndOfDay:
                    return !hour.HasValue 
                        ? new DateTime(year, month ?? 1, day ?? 1, 23, 59, 25)
                        : new DateTime(year, month ?? 1, day ?? 1, 
                            hour.Value, minute ?? 59, seconds ?? 59);

                default: throw new Exception("unknown DefaultTime");
            }
        }
    }
}
