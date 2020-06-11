using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PhotoTakerForm : IForm<HTMLElement,PhotoTakerForm,CompletedOrCanceled> {
        private readonly PhotoTakerFormView _view;
        public string Title => I18n.Translate("Taking photo");
        public IFormView<HTMLElement> View => _view;
        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
        public event Action<PhotoTakerForm, CompletedOrCanceled> Ended;
        public File PhotoAsFile => _view.InputFile.Files.FirstOrDefault();
            
        public PhotoTakerForm(PhotoTakerFormView view) {
            _view = view;
            LocalActionBuilder.Build(_view.TakePhoto, () => _view.InputFile.Click());
            LocalActionBuilder.Build(_view.AcceptPhoto, () => {
                _view.ResetPreviewStyle(true);
                Ended?.Invoke(this, CompletedOrCanceled.Completed);
            });
            LocalActionBuilder.Build(_view.RetryPhoto, () => {
                _view.ClearImage();
                _view.InputFile.Click();
            });
            
            _view.InputFile.OnChange += _ => {
                var files = _view.InputFile.Files;
                if (files == null || files.Length <= 0) {
                    Logger.Debug(GetType(), "got no files");
                    return;
                }
                Logger.Debug(GetType(), "got files {0}", files.Length);
                
                var fr = new FileReader();
                fr.OnLoad += ev => {
                    _view.SetImageFromDataUrl((string)fr.Result);
                };
                fr.ReadAsDataURL(files[0]);
            };
        }

        public void ClearImage() => _view.ClearImage();
    }
}
