using System;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class DataGridSettings {
        public static Func<DatagridContent,Task<FileModel>> ConvertJsonToXlsxOperation { get; private set; }

        public static void Init(Func<DatagridContent,Task<FileModel>> convertJsonToXlsxOperation) {
            ConvertJsonToXlsxOperation = convertJsonToXlsxOperation;
        }
    }
}
