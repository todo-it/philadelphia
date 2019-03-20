using System;

namespace Philadelphia.Web {
    public interface ICellValueExporter {
        void Export(string value);
        void Export(int value);
        void Export(DateTime value);
        void Export(decimal value);
        void Export(bool value);
    }
}
