using System;
using System.Collections.Generic;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class CellValueExporter : ICellValueExporter {
        private List<DatagridCell> _target;
        
        public void SetTarget(List<DatagridCell> target) {
            _target = target;
        }

        public void Export(string value) {
            _target.Add(new DatagridCell(value));
        }

        public void Export(int value) {
            _target.Add(new DatagridCell(value));
        }

        public void Export(DateTime value) {
            _target.Add(new DatagridCell(value));
        }

        public void Export(decimal value) {
            _target.Add(new DatagridCell(value));
        }

        public void Export(bool value) {
            _target.Add(new DatagridCell(value));
        }
    }
}
