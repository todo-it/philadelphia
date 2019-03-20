using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class RecordGroupingUtil {

        public static IEnumerable<GroupDescr> GroupAllRecordsAsOneGroup<T>(IEnumerable<T> allItems) {
            var length = allItems.Count();

            if (length <= 0) {
                return new List<GroupDescr>();
            }
            return new List<GroupDescr> { new GroupDescr {
                KeyData = 0, //irrelevant
                UserFriendlyGroupName = I18n.Translate("Total"),
                FromPhsIdx = 0,
                TillPhsIdx = length - 1,
                FromVisIdx = 0,
                TillVisIdx = length } };
        }

        public static List<GroupDescr> GroupRecordsByKey<T>(
                IEnumerable<T> allItems, Func<T,object> buildGroupingKey, Func<GroupDescr,string> summaryProvider) {
            
            var result = new List<GroupDescr>();
            
            object curKey = null;
            GroupDescr cur = null;

            allItems.ForEachI((i,x) => {
                if (cur == null) {
                    curKey = buildGroupingKey(x);

                    cur = new GroupDescr {
                        FromPhsIdx = 0,
                        FromVisIdx = 0,
                        TillPhsIdx = -1 + 1,
                        TillVisIdx = 0 + 1,
                        KeyData = curKey };
                    result.Add(cur);

                    return;
                }

                var key = buildGroupingKey(x);
                if (key.Equals(curKey)) {
                    cur.TillPhsIdx++;
                    cur.TillVisIdx++;
                    return;
                }
                curKey = key;

                cur = new GroupDescr {
                    FromPhsIdx = i,
                    FromVisIdx = cur.TillVisIdx + 1,
                    TillPhsIdx = i,
                    TillVisIdx = cur.TillVisIdx + 2,
                    KeyData = key };
                result.Add(cur);
            });
            
            result.ForEach(x => x.UserFriendlyGroupName = summaryProvider(x));

            return result;
        }
    }
}
