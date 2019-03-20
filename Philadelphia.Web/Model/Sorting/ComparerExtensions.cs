using System.Collections.Generic;

namespace Philadelphia.Web {
    public static class ComparerExtensions {
        public static IComparer<T> Reorder<T>(this IComparer<T> ascending, SortOrder order) {
            switch(order) {
                case SortOrder.Desc:
                    return new InvertingAdapterComparer<T>(ascending);
                default:
                    return ascending; //no better idea
            }
        }
    }
}
