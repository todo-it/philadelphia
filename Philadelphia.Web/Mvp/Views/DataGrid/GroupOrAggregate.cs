using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public enum GroupOrAggregate {
        Group = 1,
        Aggregate = 2
    }

    public static class GroupOrAggregateExtensions {
        public static string GetUserFriendlyName(this GroupOrAggregate self) {
            switch (self) {
                case GroupOrAggregate.Group: return I18n.Translate("Group");
                case GroupOrAggregate.Aggregate: return I18n.Translate("Aggregate");
                default: throw new Exception("not supported");
            }
        }
    }
}
