using System;
using Bridge.Html5;

namespace Philadelphia.Web {
    public static class HTMLTableElementExtensions {
        public static void SetCell(this HTMLTableElement self, int row, int col, Action<HTMLElement> setCell) {
            var tbody = self.TBodies[0];
            var rowsToAdd = row - tbody.Children.Length;

            while (rowsToAdd-- >= 0) {
                tbody.AppendChild(new HTMLTableRowElement());
            }

            var tr = tbody.GetChildAtOrNull(row);

            var colsToAdd = col - tr.Children.Length;

            while (colsToAdd-- >= 0) {
                tr.AppendChild(new HTMLTableDataCellElement());
            }

            setCell(tr.GetChildAtOrNull(col));
        }

        public static HTMLTableElement BuildTable() {
            var tbl = new HTMLTableElement();
            tbl.AppendChild(new HTMLElement("tbody"));
            return tbl;
        }
    }
}
