namespace Philadelphia.Common {
    // REVIEW: does this really need to inherit from IBareForm<WidgetT>?
    public interface IOnShownNeedingForm<WidgetT> : IBareForm<WidgetT> {
        void OnShown();
    }
}
