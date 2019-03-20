namespace Philadelphia.Common {
    // ReSharper disable once TypeParameterCanBeVariant due to
    // https://github.com/bridgedotnet/Bridge/issues/3876
    // as it breaks in UploadView calling IUploadViewAction->Create()
    public interface IView<WidgetT> {
        WidgetT Widget { get; }
    }
}
