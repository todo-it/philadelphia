namespace Philadelphia.Common {
    public interface IFormRenderer<WidgetT> {
        void ReplaceMasterWithAdapter(IView<WidgetT> view);
        void ReplaceMaster(IBareForm<WidgetT> newForm);
        void AddPopup(IBareForm<WidgetT> newForm);
        void Remove(IBareForm<WidgetT> frm);
        void ClearMaster();
        IFormRenderer<WidgetT> CreateRendererWithBase(IFormCanvas<WidgetT> masterCanvas);
    }
}
