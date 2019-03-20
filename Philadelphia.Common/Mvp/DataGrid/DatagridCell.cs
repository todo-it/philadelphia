using System;

namespace Philadelphia.Common {
    public class DatagridCell {
        public string S;
        public int? I;
        public DateTime? Dt;
        public decimal? De;
        public bool? B;

        public DatagridCell() {}

        public DatagridCell(string s) {
            S = s;
        }
        
        public DatagridCell(int i) {
            I = i;
        }
        
        public DatagridCell(DateTime dt) {
            Dt = dt;
        }
        
        public DatagridCell(decimal de) {
            De = de;
        }
        
        public DatagridCell(bool b) {
            B = b;
        }
    }
}
