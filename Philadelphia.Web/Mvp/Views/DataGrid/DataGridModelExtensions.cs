namespace Philadelphia.Web {
    public static class DataGridModelExtensions {
        public static DataGridModelPresenter<RecordT> Bind<RecordT>(this DataGridModel<RecordT> self, ITableView view) {
            return new DataGridModelPresenter<RecordT>(self, view);
        }

        public static DataGridModelPresenter<RecordT> BindAndInitialize<RecordT>(this DataGridModel<RecordT> self, ITableView view) {
            var result = self.Bind(view);
            result.Initialize();
            return result;
        }
    }
}
