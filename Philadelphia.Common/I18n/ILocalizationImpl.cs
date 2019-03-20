using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface ILocalizationImpl {
        string Localize(bool input, BoolFormat format);
        string Localize(decimal input, DecimalFormat format);
        string Localize(int input);
        string Localize(DateTime input, DateTimeFormat format);
        decimal ParseDecimal(string s);
        decimal ParseDecimalWithoutLoss(string s, int precision);
        int ParseInt(string s);
        DayOfWeek GetFirstDayOfWeek();
        string GetFullMonthName(int monthNo);
        string GetWeekDayAcronym(DayOfWeek dow);
    }
}
