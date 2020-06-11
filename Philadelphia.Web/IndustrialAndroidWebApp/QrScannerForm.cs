using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class QrScannerForm<T> : IForm<HTMLElement,QrScannerForm<T>, CompletedOrCanceled>,IOnShownNeedingForm<HTMLElement> {
        private readonly string _title;
        private readonly Func<string, Task<T>> _getItemByCode;
        private readonly int _scannerPopupMmFromTop, _scannerPopupHeightMm;
        private readonly QrScannerFormView _view;
        private IScanResult _hndl;
        public string Title => _title;
        public IFormView<HTMLElement> View => _view;
        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => _hndl?.CancelScanning());
        public event Action<QrScannerForm<T>, CompletedOrCanceled> Ended;
        public T ScannedCode { get; private set; }
        private string _errorOrNull;
        private readonly LocalValue<string> _errorLbl;
        
        public QrScannerForm(
                QrScannerFormView view,
                string title, string label, 
                Func<string,Task<T>> getItemByCode,
                int scannerPopupMmFromTop = 15, 
                int scannerPopupHeightMm = 40) {

            _view = view;
            _view.Label = label;
            
            _view.PadMmFromTop = scannerPopupMmFromTop;
            _scannerPopupMmFromTop = scannerPopupMmFromTop;
            
            _view.HeightMm = scannerPopupHeightMm;
            _scannerPopupHeightMm = scannerPopupHeightMm;
            
            _title = title;
            _getItemByCode = getItemByCode;
            
            LocalActionBuilder.Build(_view.Unpause, async () => {
                _view.Unpause.Widget.Style.Display = Display.None;
                await _errorLbl.DoChange("", false, this, false);
                _hndl?.ResumeScanning();
            });
            LocalActionBuilder.Build(_view.Cancel, () => _hndl?.CancelScanning());
            _errorLbl = LocalValueFieldBuilder.Build(_view.Error);
        }

        public void OnShown() {
            ScannedCode = default(T);
            Window.SetTimeout(async () => await HandleScanning(), 1);
            _view.Unpause.Widget.Style.Display = Display.None;
        }

        private async Task HandleScanning() {
            _hndl = IawAppApi.RequestScanQr(
                true, 
                new MatchWidthWithFixedHeightLayoutStrategy(
                    _scannerPopupHeightMm, _scannerPopupMmFromTop));
            
            var ended = false;

            while (!ended) {
                var ev = await _hndl.GetNext();
                Logger.Debug(GetType(), "got ({0}; {1})", ev.Item1, ev.Item2);

                switch (ev.Item1) {
                    case ScanResultType.Scanned:
                        try {
                            _errorOrNull = null;
                            ScannedCode = await _getItemByCode(ev.Item2);
                        } catch (Exception ex) {
                            _errorOrNull = ex.Message;
                        }
                        await _errorLbl.DoChange(_errorOrNull, false, this, false);
                        break;

                    case ScanResultType.Paused:
                        if (_errorOrNull == null) {
                            _hndl.CancelScanning();
                        } else {
                            
                            //TODO extract into stylesheet
                            _view.Unpause.Widget.Style.SetProperty("display", "");
                        }
                        break;

                    case ScanResultType.Disposed:
                        _hndl = null;
                        ended = true;
                        Ended?.Invoke(this,
                            ScannedCode == null ? CompletedOrCanceled.Canceled : CompletedOrCanceled.Completed);
                        break;
                }
            }
        }
    }
}
