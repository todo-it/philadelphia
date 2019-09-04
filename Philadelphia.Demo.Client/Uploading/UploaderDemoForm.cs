using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;
using System.Linq;
using System.Threading.Tasks;

namespace Philadelphia.Demo.Client {
    public class UploaderDemoForm : IForm<HTMLElement,UploaderDemoForm,Unit> {
        public event Action<UploaderDemoForm, Unit> Ended;
        public string Title => "Upload widget demo";
        public IFormView<HTMLElement> View => _view;

        private readonly UploaderDemoFormView _view;
        private readonly LocalValue<List<RemoteFileDescr>> _attachments;

        public UploaderDemoForm(IHttpRequester httpRequester) {
            _view = new UploaderDemoFormView();
            _view.Attachments.SetImplementation(new ISomeService_OrderAttachment(httpRequester,() => 124, () => true));
            _attachments = LocalValueFieldBuilder.Build(new List<RemoteFileDescr>(), _view.Attachments, 
                (newValue, errors) => {
                    errors.IfTrueAdd(newValue.Count > 10, "Maximum 10 files allowed");
                } );
            
            //TODO this is experimental feature. It should be added by the above statement (if BeforeChange is supported in IView)
            _view.Attachments.BeforeChange += (newValue, isUser, preventProp) => {
                if (newValue.Count > 10) {
                    preventProp(new HashSet<string> { "Maximum 10 files allowed" });
                }
            };

            var mayConfirm = new AggregatedErrorsValue<bool>(
                false, self => !self.Errors.Any(), x => x.Observes(_attachments));
            
            var confirm = LocalActionBuilder.Build(_view.Confirm, () => Ended?.Invoke(this, Unit.Instance));
            confirm.BindEnableAndInitialize(mayConfirm);
        }
        
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Unit.Instance));

        public async Task InitFiles(RemoteFileId[] remoteFileIds) {
            await _attachments.DoChange(
                remoteFileIds.Select(x => RemoteFileDescr.CreateFrom(x)).ToList(),
                false,
                this,
                false);
        }

        public UploadView GetUploadControl() {
            return _view.Attachments;
        }
    }
}
