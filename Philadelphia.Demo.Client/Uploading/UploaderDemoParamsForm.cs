using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class UploaderDemoParamsForm : IForm<HTMLElement,UploaderDemoParamsForm,CompletedOrCanceled> {
        public event Action<UploaderDemoParamsForm, CompletedOrCanceled> Ended;
        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
        private readonly UploaderDemoParamsFormView _view;
        private UploadView _uv;
        private static (int width,int height) DemoThumbnailsMaxDimensions = (160, 107);

        public string Title => "Uploader parameters";
        public IFormView<HTMLElement> View => _view;
        
        public UploaderDemoParamsForm() {
            _view = new UploaderDemoParamsFormView();

            
            var fileNameLen = LocalValueFieldBuilder.BuildNullableInt(
                14, _view.MaxFileNameLength, Validator.MustBePositive<int>());
            var itemOpeningMethod = LocalValueFieldBuilder.Build(
                OpenImagesMethod.Lightbox, _view.ItemOpeningMethod, Validator.IsNotNull);
            

            var gridColCount = LocalValueFieldBuilder.BuildNullableInt(
                3, _view.GridColumnsCount, Validator.MustBePositive<int>());
            var createGridBased = LocalActionBuilder.Build(_view.CreateGridBased, () => {
                _uv.ChangePresentationToThumbnailGrid(DemoThumbnailsMaxDimensions);
                _uv.ImageOpenMethod = itemOpeningMethod.Value;
                _uv.FileNameMaxVisibleLength = fileNameLen.Value.Value;
                _uv.GridColumnsCount = gridColCount.Value.Value;
                Ended?.Invoke(this, CompletedOrCanceled.Completed);
            });            
            var mayCreateGridBased = new AggregatedErrorsValue<bool>(
                false, self => !self.Errors.Any(), x => {
                    x.Observes(itemOpeningMethod);
                    x.Observes(fileNameLen);
                    x.Observes(gridColCount);
                });
            createGridBased.BindEnableAndInitialize(mayCreateGridBased);
            

            

            var createRowBased = LocalActionBuilder.Build(_view.CreateRowBased, () => {
                _uv.ChangePresentationToTextRows();
                _uv.ImageOpenMethod = itemOpeningMethod.Value;
                _uv.FileNameMaxVisibleLength = fileNameLen.Value.Value;
                Ended?.Invoke(this, CompletedOrCanceled.Completed);
            });            
            var mayCreateRowBased = new AggregatedErrorsValue<bool>(
                false, self => !self.Errors.Any(), x => {
                    x.Observes(itemOpeningMethod);
                    x.Observes(fileNameLen);
                });
            createRowBased.BindEnableAndInitialize(mayCreateRowBased);
        }

        public void SetParent(UploadView uv) {
            _uv = uv;
        }
    }
}
