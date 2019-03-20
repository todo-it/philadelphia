using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class SortOrderExtensions {   
        /// <summary>sort order initially is unspecified. Once user changed it it keeps cycling between ASC&amp;DESC</summary>
        public static SortOrder CalculateNext(this SortOrder inp) {
            switch(inp) {
                case SortOrder.Unsupported:
                    return SortOrder.Unsupported;
                case SortOrder.Unspecified:
                    return SortOrder.Asc;
                case SortOrder.Asc:
                    return SortOrder.Desc;
                case SortOrder.Desc:
                    return SortOrder.Asc;
                default:
                    Logger.Error(typeof(SortOrderExtensions), "Unsupported value of SortOrder");
                    return SortOrder.Unspecified;
            }
        }
        
        public static string GetIconText(this SortOrder inp) {
            switch(inp) {
                case SortOrder.Unsupported:
                    return " ";
                case SortOrder.Unspecified:
                    return Magics.FontAwesomeSortOrderUnspecified;
                case SortOrder.Asc:
                    return Magics.FontAwesomeSortOrderAsc;
                case SortOrder.Desc:
                    return Magics.FontAwesomeSortOrderDesc;
                default:
                    Logger.Error(typeof(SortOrderExtensions), "Unsupported value of SortOrder");
                    return Magics.FontAwesomeSortOrderUnspecified;
            }
        }
    }
}
