using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class UploaderDemoFlow : IFlow<HTMLElement> {
        private readonly UploaderDemoParamsForm _params;
        private readonly UploaderDemoForm _demo;
        private readonly RemoteActionsCallerForm _fetchFiles;
        private RemoteFileId[] _files;

        public UploaderDemoFlow(ISomeService someService) {
            _fetchFiles = new RemoteActionsCallerForm(x => x.Add(
                someService.OrderAttachmentGetFiles,
                y => _files = y));
            _demo = new UploaderDemoForm();
            _params = new UploaderDemoParamsForm();
            _params.SetParent(_demo.GetUploadControl());
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_fetchFiles);

            _fetchFiles.Ended += (x, outcome) => {
                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        renderer.Remove(x);
                        renderer.AddPopup(_params);
                        break;

                    case RemoteActionsCallerForm.Outcome.Canceled:
                    case RemoteActionsCallerForm.Outcome.Interrupted:
                        renderer.Remove(x);
                        atExit();
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };

            _params.Ended += async (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Canceled:
                        renderer.Remove(x);
                        atExit();
                        break;

                    case CompletedOrCanceled.Completed:
                        await _demo.InitFiles(_files);
                        renderer.Remove(x);
                        renderer.AddPopup(_demo);
                        break;
                    
                    default: throw new Exception("unsupported outcome");
                }
            };

            _demo.Ended += (x, _) => {
                renderer.Remove(x);
                atExit();
            };
        }
    }
}
