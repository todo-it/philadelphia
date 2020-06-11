using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bridge.Html5;
using Newtonsoft.Json;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LayoutStrategy {
        public string typeName;
    }

    public class FillScreenLayoutStrategy : LayoutStrategy {
        public string screenTitle;
        public bool hideToolbar;
        
        public FillScreenLayoutStrategy(string screenTitle = null) {
            typeName = nameof(FillScreenLayoutStrategy);
            this.screenTitle = screenTitle;
        }
    }

    public class MatchWidthWithFixedHeightLayoutStrategy : LayoutStrategy {
        public bool paddingOriginIsTop;
        public int paddingMm;
        public int heightMm;

        public MatchWidthWithFixedHeightLayoutStrategy(int heightMm, int paddingMm, bool paddingOriginIsTop = true) {
            typeName = nameof(MatchWidthWithFixedHeightLayoutStrategy);
            this.heightMm = heightMm;
            this.paddingMm = paddingMm;
            this.paddingOriginIsTop = paddingOriginIsTop;
        }
    }

    public class MenuItemInfo {
        public string webMenuItemId;
        public bool trueForAction;
        public string title;
        public bool enabled = true;
        public string iconMediaIdentifierId;
    }

    public class ToolbarSettings {
        public bool BackActionVisible { get; set; }
        public List<Tuple<MenuItemInfo,Action>> MenuItems { get; } = new List<Tuple<MenuItemInfo,Action>>();
        
        /// <summary>bool isCommitted, string queryText</summary>
        public Action<bool,string> SearchCallback { get; set; }

        /// <summary>in format accepted by android.graphics.Color.parse() typically #RRGGBB</summary>
        public string AppBarBackgroundColor { get; set; }
        
        /// <summary>in format accepted by android.graphics.Color.parse() typically #RRGGBB</summary>
        public string AppBarForegroundColor { get; set; }
    }

    public interface IAugmentsToolbar {
        void OnAugmentToolbar(ToolbarSettings toAugment);
    }
    
    /**
     * refer to TypeScript file for full docs
     */
    public class IAWAppHostApi {
        private readonly object _impl;
        private static IDictionary<string,Action<IAWAppScanReply>> _scanQrCallbacks 
            = new Dictionary<string, Action<IAWAppScanReply>>();
        private static IDictionary<string,Action<string>> _mediaReadyCallbacks
            = new Dictionary<string, Action<string>>();
        
        public IAWAppHostApi(object impl) {
            _impl = impl;
        }

        public void setToolbarItems(string menuItemInfosAsJson) {
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(setToolbarItems), menuItemInfosAsJson);
        }
        
        public void setToolbarSearchState(bool v) {
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(setToolbarSearchState), 
                //avoid boxing
                v ? BridgeObjectUtil.True : BridgeObjectUtil.False);
        }

        public void setToolbarBackButtonState(bool v) {
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(setToolbarBackButtonState), 
                //avoid boxing
                v ? BridgeObjectUtil.True : BridgeObjectUtil.False);
        }

        public bool setScanSuccessSound(string mediaAssetId) =>
            (bool)BridgeObjectUtil.CallMethodPlain(_impl, nameof(setScanSuccessSound), mediaAssetId);
        
        public bool setPausedScanOverlayImage(string mediaAssetId) =>
            (bool)BridgeObjectUtil.CallMethodPlain(_impl, nameof(setPausedScanOverlayImage), mediaAssetId);

        public bool hasMediaAsset(string mediaAssetId) =>
            (bool)BridgeObjectUtil.CallMethodPlain(_impl, nameof(hasMediaAsset), mediaAssetId);
        
        public void setToolbarColors(string backgroundColor, string foregroundColor) {
            BridgeObjectUtil.CallMethodPlain(_impl, "setToolbarColors", backgroundColor, foregroundColor);
        }
        
        public void requestScanQr(string webRequestId, bool askJsForValidation, LayoutStrategy layout) {
            var layoutAsJson = JsonConvert.SerializeObject(layout);
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(requestScanQr),
                webRequestId,
                askJsForValidation ? BridgeObjectUtil.True : BridgeObjectUtil.False, 
                layoutAsJson);
        }

        public void resumeScanQr(string webRequestId) => 
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(resumeScanQr), webRequestId);
        
        public void cancelScanQr(string webRequestId) => 
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(cancelScanQr), webRequestId);

        public void registerMediaAsset(string webRequestId, string fileContent) =>
            BridgeObjectUtil.CallMethodPlain(_impl, nameof(registerMediaAsset), webRequestId, fileContent);
        
        public static void RegisterPostMediaAssetReady(string requestId, Action<string> action) {
            Logger.Debug(typeof(IAWAppHostApi), "RegisterPostMediaAssetReady {0}", requestId);
            var handler = BridgeObjectUtil.GetFieldValue(Window.Instance, "androidPostMediaAssetReady");
            
            if (handler == null) {
                Logger.Debug(typeof(IAWAppHostApi), "setup handler");
                BridgeObjectUtil.SetFieldValue(Window.Instance, nameof(androidPostMediaAssetReady), (Action<string,string>)androidPostMediaAssetReady);
            } else {
                Logger.Debug(typeof(IAWAppHostApi), "handler already installed");    
            }
            
            _mediaReadyCallbacks.Add(requestId, action);
        }

        public static void UnregisterPostMediaAssetReady(string requestId) {
            Logger.Debug(typeof(IAWAppHostApi), "UnregisterPostMediaAssetReady {0}", requestId);
            _mediaReadyCallbacks.Remove(requestId);
        }

        public static void androidPostMediaAssetReady(string webRequestIdUriEncoded, string mediaAssetId) {
            Logger.Debug(typeof(IAWAppHostApi), "androidPostMediaAssetReady webRequestIdUriEncoded={0} scanReplyJsonUriEncoded={1}", 
                webRequestIdUriEncoded, mediaAssetId);
            var webRequestId = Window.DecodeURIComponent(webRequestIdUriEncoded);
            
            var f = _mediaReadyCallbacks[webRequestId];
            UnregisterPostMediaAssetReady(webRequestId);
            f(mediaAssetId);
        }
        
        public static void RegisterPostScanQrReplyHandler(string requestId, Action<IAWAppScanReply> action) {
            Logger.Debug(typeof(IAWAppHostApi), "RegisterPostScanQrReplyHandler {0}", requestId);
            var handler = BridgeObjectUtil.GetFieldValue(Window.Instance, nameof(androidPostScanQrReply));
            
            if (handler == null) {
                Logger.Debug(typeof(IAWAppHostApi), "setup handler");
                BridgeObjectUtil.SetFieldValue(Window.Instance, nameof(androidPostScanQrReply), (Action<string>)androidPostScanQrReply);
            } else {
                Logger.Debug(typeof(IAWAppHostApi), "handler already installed");    
            }
            
            _scanQrCallbacks.Add(requestId, action);
        }

        public static void UnregisterPostScanQrReplyHandler(string requestId) {
            Logger.Debug(typeof(IAWAppHostApi), "UnregisterPostScanQrReplyHandler {0}", requestId);
            _scanQrCallbacks.Remove(requestId);
        }
        
        public static void androidPostScanQrReply(string scanReplyJsonUriEncoded) {
            Logger.Debug(typeof(IAWAppHostApi), "androidPostScanQrReply scanReplyJsonUriEncoded={0}", scanReplyJsonUriEncoded);
            var scanReplyJson = Window.DecodeURIComponent(scanReplyJsonUriEncoded);
            var reply = JsonConvert.DeserializeObject<IAWAppScanReply>(scanReplyJson);

            Logger.Debug(typeof(IAWAppHostApi), "androidPostScanQrReply invoking for webRequestId={0} cancellation?={1}", reply.WebRequestId, reply.IsCancellation);
            var f = _scanQrCallbacks[reply.WebRequestId];

            if (reply.IsDisposal) {
                UnregisterPostScanQrReplyHandler(reply.WebRequestId);    
            }

            f(reply);
        }
    }
    
    public class IAWAppScanReply {
        public string WebRequestId;
        public bool IsDisposal;
        public bool IsPaused;
        public bool IsCancellation;
        public string Barcode;

        public override string ToString() {
            return $"<IAWAppScanReply WebRequestId={WebRequestId} IsDisposal={IsDisposal} IsCancellation={IsCancellation} Barcode={Barcode}>";
        }
    }
    
    public enum ScanResultType {
        Error,
        Scanned,
        Paused,
        Cancelled,
        Disposed
    }

    public interface IScanResult {
        Task<Tuple<ScanResultType, string>> GetNext();
        void ResumeScanning();
        void CancelScanning();
    }

    public class ScanResult : IScanResult {
        private readonly IAWAppHostApi _impl;
        private readonly LayoutStrategy _layout;
        private readonly string _webRequestId = UniqueIdGenerator.GenerateAsString();
        
        private bool _cancellationConfirmed, _pauseAfterBarcode, _started, _disposed;
        private IPromise _currentPromise;
        
        private Action<Tuple<ScanResultType, string>> _onSuccess;
        private Action<Exception> _onError;
        private List<IAWAppScanReply> _queued = new List<IAWAppScanReply>();

        public ScanResult(IAWAppHostApi impl, bool pauseAfterBarcode, LayoutStrategy layout) {
            _impl = impl;
            _layout = layout;
            _pauseAfterBarcode = pauseAfterBarcode;
        }

        private Tuple<ScanResultType, string> ReplyToTuple(IAWAppScanReply reply) {
            Logger.Debug(GetType(), "OnPostScanQrReceived() for webRequestId={0} received={1}",
                _webRequestId, reply);
            
            if (reply.IsDisposal) {
                _disposed = true;
                Logger.Debug(GetType(), "OnPostScanQrReceived() disposed");
                return Tuple.Create(ScanResultType.Disposed, (string)null);
            }

            if (reply.IsPaused) {
                Logger.Debug(GetType(), "OnPostScanQrReceived() paused");
                return Tuple.Create(ScanResultType.Paused, (string)null);
            }

            if (reply.IsCancellation) {
                _cancellationConfirmed = true;
                Logger.Debug(GetType(), "OnPostScanQrReceived() cancelled");
                return Tuple.Create(ScanResultType.Cancelled, (string)null);
            }

            Logger.Debug(GetType(), "OnPostScanQrReceived() scanned");
            return Tuple.Create(ScanResultType.Scanned, reply.Barcode);
        }

        private void OnPostScanQrReceived(IAWAppScanReply reply) {
            Logger.Debug(GetType(), "OnPostScanQrReceived() for webRequestId={0} received={1} hasOnSuccess?={2} hasOnError?={3}", 
                _webRequestId, reply, _onSuccess != null, _onError != null);

            var onSuccess = _onSuccess;
            _onSuccess = null;
            
            var onError = _onError;
            _onError = null;

            if (onSuccess == null || onError == null) {
                _queued.Add(reply);
                return;
            }

            var res = ReplyToTuple(reply);

            if (res != null) {
                onSuccess.Invoke(res);
            }
        }

        private void OnWaitingForPost(Action<Tuple<ScanResultType, string>> onSuccess, Action<Exception> onError) {
            Logger.Debug(GetType(), "OnWaitingForPost() for webRequestId={0}", _webRequestId);
            
            _onSuccess = onSuccess;
            _onError = onError;
        }

        public Task<Tuple<ScanResultType, string>> GetNext() {
            Logger.Debug(GetType(), "starting scanner webRequestId={0}", _webRequestId);
            
            if (!_started) {
                try {
                    IAWAppHostApi.RegisterPostScanQrReplyHandler(_webRequestId, OnPostScanQrReceived);
                    _impl.requestScanQr(_webRequestId, _pauseAfterBarcode, _layout);
                    _started = true;
                } catch (Exception ex) {
                    Logger.Error(GetType(), "error starting scanner {0}", ex);
                    IAWAppHostApi.UnregisterPostScanQrReplyHandler(_webRequestId);
                    return Task.FromResult(Tuple.Create(ScanResultType.Error, (string)null));
                }
            }

            if (_disposed) {
                Logger.Error(GetType(), "scanner instance is disposed");
                return Task.FromResult(Tuple.Create(ScanResultType.Error, (string)null));
            }

            while (_queued.Any()) {
                var fst = _queued[0];
                _queued.RemoveAt(0);

                var res = ReplyToTuple(fst);

                if (res != null) {
                    return Task.FromResult(res);
                }
            }

            _currentPromise = new TypeSafePromise<Tuple<ScanResultType, string>>(OnWaitingForPost);
            var tsk = Task.FromPromise<Tuple<ScanResultType, string>>(
                _currentPromise,
                (Func<Tuple<ScanResultType, string>,Tuple<ScanResultType, string>>)(x => x));
            return tsk;
        }

        public void ResumeScanning() => _impl.resumeScanQr(_webRequestId);
        public void CancelScanning() => _impl.cancelScanQr(_webRequestId);
    }
    
    /// <summary>
    /// simple&stupid developer oriented in-desktop-browser scanning simulator
    /// </summary>
    public class WindowPromptBasedScanResult : IScanResult {
        private readonly bool _pauseAfterBarcode;
        private readonly LayoutStrategy _layout;
        private bool _canceled, _scanned, _disposed, _paused, _gotCancelRequest, _gotPauseRequest;

        public WindowPromptBasedScanResult(bool pauseAfterBarcode, LayoutStrategy layout) {
            _pauseAfterBarcode = pauseAfterBarcode;
            _layout = layout;
        }

        private Task WaitWhileFalse(Func<bool> cond) {
            Action<Unit> onSucc = null;
            int intervalHndl = 0;
            
            return Task.FromPromise(new TypeSafePromise<Unit>((succ, _) => {
                onSucc = succ;
                intervalHndl = Window.SetInterval(
                    () => {
                        Logger.Debug(GetType(), "WaitWhileFalse()");
                        if (cond()) {
                            Logger.Debug(GetType(), "WaitWhileFalse() ending");
                            Window.ClearInterval(intervalHndl);
                            succ(Unit.Instance);
                            return;
                        }
                        Logger.Debug(GetType(), "WaitWhileFalse() continuing");
                    }, 
                    500);
            }));
        }

        public void ResumeScanning() {
            Logger.Debug(GetType(), "ResumeScanning() _paused={0}", _paused);
            if (_paused) {
                _gotPauseRequest = false;
                _paused = false;
            }
        }

        public void CancelScanning() {
            Logger.Debug(GetType(), "CancelScanning() _gotCancelRequest={0} _paused={1}", _gotCancelRequest, _paused);
            if (!_gotCancelRequest) {
                _gotCancelRequest = true;

                if (_paused) {
                    _gotPauseRequest = false;
                    _paused = false;
                }
            }
        }

        public async Task<Tuple<ScanResultType, string>> GetNext() {
            Logger.Debug(GetType(), "GetNext() _canceled={0}, _scanned={1}, _disposed={2}, _paused={3}, _gotCancelRequest={4}, _gotPauseRequest={5}",
                _canceled, _scanned, _disposed, _paused, _gotCancelRequest, _gotPauseRequest);

            if (_disposed) {
                throw new Exception("already disposed");
            }

            if (_paused) {
                await WaitWhileFalse(() => !_paused);
            }

            if (_gotCancelRequest) {
                _gotCancelRequest = false;
                _canceled = true;
                return Tuple.Create(ScanResultType.Cancelled, (string)null);
            }

            if (_gotPauseRequest) {
                _paused = true;
                return Tuple.Create(ScanResultType.Paused, (string)null);
            }

            if (!_canceled && !_scanned) {
                var res = Window.Prompt("scan qr:");

                if (res == null) {
                    _canceled = true;
                    return Tuple.Create(ScanResultType.Cancelled, (string) null);
                }

                _scanned = !_pauseAfterBarcode;
                _gotPauseRequest = _pauseAfterBarcode;
                return Tuple.Create(ScanResultType.Scanned, res);
            }

            if (_scanned && !_canceled) {
                _canceled = true;
                return Tuple.Create(ScanResultType.Cancelled, (string)null);
            }

            if (_canceled) {
                _disposed = true;
                return Tuple.Create(ScanResultType.Disposed, (string)null);
            }

            throw new Exception("unknown state");
        }
    }

    public static class IawAppApi {
        private static IAWAppHostApi GetImpl() {
            var impl = BridgeObjectUtil.GetFieldValue(Window.Instance, "IAWApp");

            if (impl != null) {
                Logger.Debug(typeof(IawAppApi), "has proper IAWApp API");
                return new IAWAppHostApi(impl);
            }

            Logger.Debug(typeof(IawAppApi), "doesn't have proper IAWApp API");
            return null;
        }

        public static Task<string> RegisterMediaAsset(string fileContent) {
            var requestId = UniqueIdGenerator.GenerateAsString();
            
            var impl = GetImpl();
            
            if (impl == null) {
                Logger.Debug(typeof(IawAppApi), "has no proper IAWApp API");
                return Task.FromResult(fileContent.Length.ToString());
            }
            
            //androidPostMediaAssetReady(webRequestIdUriEncoded : string, properMediaFileId : string) : void;
            return Task.FromPromise<string>(
                new TypeSafePromise<string>((onSucc, onFail)=> {
                    IAWAppHostApi.RegisterPostMediaAssetReady(requestId, x => onSucc(x));

                    try {
                        impl.registerMediaAsset(requestId, fileContent);
                    } catch (Exception ex) {
                        Logger.Error(typeof(IawAppApi), "registerMediaAsset got exception {0}", ex);
                        IAWAppHostApi.UnregisterPostMediaAssetReady(requestId);
                        onFail(ex);
                    }
                }), 
                (Func<string,string>)( x => x));
        }

        public static IScanResult RequestScanQr(bool pauseAfterBarcode, LayoutStrategy layout) {
            var impl = GetImpl();
            
            if (impl == null) {
                Logger.Debug(typeof(IawAppApi), "has no proper IAWApp API");
                return new WindowPromptBasedScanResult(pauseAfterBarcode, layout);
            }
            
            return new ScanResult(impl, pauseAfterBarcode, layout);
        }
        
        /// <summary>
        /// returned async task function:
        ///   throws exception if there was error (f.e. android camera permissions)
        ///   OR returns not null scanned code
        ///   OR returns null if scanning was cancelled by user
        /// </summary>
        public static async Task<string> RequestScanQrNonValidatable(LayoutStrategy layout, CancellationToken canc) {
            var hndl = RequestScanQr(false, layout);
            string result = null;

            canc.Register(() => {
                Logger.Debug(typeof(IawAppApi), "RequestScanQrNonValidatable() requesting cancellation");
                hndl.CancelScanning();
            });
            
            while (true) {
                Logger.Debug(typeof(IawAppApi), "RequestScanQrNonValidatable() requesting next");
                var v = await hndl.GetNext();
                Logger.Debug(typeof(IawAppApi), "RequestScanQrNonValidatable() got ({0}; {1})", v.Item1, v.Item2);

                if (v.Item1 == ScanResultType.Paused) {
                    throw new Exception("unexpected pause");
                }

                if (v.Item1 == ScanResultType.Scanned) {
                    result = v.Item2;
                    Logger.Debug(typeof(IawAppApi), "RequestScanQrNonValidatable() deferring result {0}", result);
                }

                if (v.Item1 == ScanResultType.Disposed) {
                    Logger.Debug(typeof(IawAppApi), "RequestScanQrNonValidatable() returning deferred result {0}", result);
                    return result;
                } 
            }
        }
        
        /// <param name="callback">return true if webapp consumed event. false if it didn't</param>
        public static void SetOnBackPressed(Func<bool> callback) =>
            BridgeObjectUtil.SetFieldValue(Window.Instance, "androidBackConsumed", callback);
        
        public static bool HasMediaAsset(string mediaIdentifierId) {
            var impl = GetImpl();
            
            if (impl != null) {
                var result = impl.hasMediaAsset(mediaIdentifierId);
                Logger.Debug(typeof(IawAppApi), "IAWApp HasMediaAsset() called");
                return result;
            }
            
            Logger.Debug(typeof(IawAppApi), "fake HasMediaAsset() called");
            return true;
        }
        
        public static bool SetScanSuccessSound(string mediaIdentifierId) {
            var impl = GetImpl();
            
            if (impl != null) {
                var res = impl.setScanSuccessSound(mediaIdentifierId);
                Logger.Debug(typeof(IawAppApi), "IAWApp setScanSuccessSound() called");
                return res;
            } 
            
            BridgeObjectUtil.SetFieldValue(Window.Instance, "_iawapp_setScanSuccessSound", mediaIdentifierId);
            Logger.Debug(typeof(IawAppApi), "fake setScanSuccessSound() called");
            return true;
        }
        
        public static bool SetPausedScanOverlayImage(string mediaIdentifierId) {
            var impl = GetImpl();
            
            if (impl != null) {
                var res = impl.setPausedScanOverlayImage(mediaIdentifierId);
                Logger.Debug(typeof(IawAppApi), "IAWApp setPausedScanOverlayImage() called");
                return res;
            } 
            
            BridgeObjectUtil.SetFieldValue(Window.Instance, "_iawapp_setPausedScanOverlayImage", mediaIdentifierId);
            Logger.Debug(typeof(IawAppApi), "fake setPausedScanOverlayImage() called");
            return true;
        }
        
        public static void SetToolbarColors(string backgroundColor, string foregroundColor) {
            var impl = GetImpl();
            
            if (impl != null) {
                impl.setToolbarColors(backgroundColor, foregroundColor);
                Logger.Debug(typeof(IawAppApi), "IAWApp SetToolbarColors() called");
            } else {
                var v = backgroundColor + "_" + foregroundColor;
                BridgeObjectUtil.SetFieldValue(Window.Instance, "_iawapp_setToolbarColors", v);
                Logger.Debug(typeof(IawAppApi), "fake SetToolbarColors() called");
            }
        }

        public static void SetToolbarBackButtonState(bool enabled) {
            var impl = GetImpl();
            
            if (impl != null) {
                impl.setToolbarBackButtonState(enabled);
                Logger.Debug(typeof(IawAppApi), "IAWApp SetToolbarBackButtonState() called");
            } else {
                BridgeObjectUtil.SetFieldValue(Window.Instance, "_iawapp_setToolbarBackButtonState", enabled);
                Logger.Debug(typeof(IawAppApi), "fake SetToolbarBackButtonState() called");
            }
        }

        public static void SetToolbarSearchState(Action<bool, string> onUpdateOrNull = null) {
            Action<bool, string> searchUsed = (committed, queryUriEncoded) => {
                var query = Window.DecodeURIComponent(queryUriEncoded);
                Logger.Debug(typeof(IawAppApi), "androidPostToolbarSearchUpdate({0}, {1})", committed, query);
                onUpdateOrNull(committed, query);
            };

            if (onUpdateOrNull == null) {
                Logger.Debug(typeof(IawAppApi), "setToolbarSearchState() deactivation");
                BridgeObjectUtil.SetFieldValue(Window.Instance, "androidPostToolbarSearchUpdate", null);
            } else {
                Logger.Debug(typeof(IawAppApi), "setToolbarSearchState() deactivation");
                BridgeObjectUtil.SetFieldValue(Window.Instance, "androidPostToolbarSearchUpdate", searchUsed);
            }
            
            var impl = GetImpl();
            
            if (impl != null) {
                impl.setToolbarSearchState(onUpdateOrNull != null);
                Logger.Debug(typeof(IawAppApi), "IAWApp setToolbarSearchState() called");
            } else {
                BridgeObjectUtil.SetFieldValue(Window.Instance, "_iawapp_setToolbarSearchState", onUpdateOrNull != null);
                Logger.Debug(typeof(IawAppApi), "fake setToolbarItems() called");
            }
        }

        public static void SetToolbarItems(Action<MenuItemInfo> onActivated, params MenuItemInfo[] rawMenuItems) =>
            SetToolbarItems(onActivated, rawMenuItems.AsEnumerable());
        
        public static void SetToolbarItems(Action<MenuItemInfo> onActivated, IEnumerable<MenuItemInfo> rawMenuItems) {
            var menuItems = rawMenuItems.ToList();

            Action<string> rawtoolbarItemActivated = webItemIdUriEncoded => {
                var webItemId = Window.DecodeURIComponent(webItemIdUriEncoded);
                var maybeMenuItem = menuItems.FirstOrDefault(x => x.webMenuItemId == webItemId);
                
                Logger.Debug(typeof(IawAppApi), "androidPostToolbarItemActivated({0}) found?={1} allItems={2}", webItemId, maybeMenuItem, menuItems.PrettyToString(y => y.webMenuItemId));

                if (maybeMenuItem != null) {
                    onActivated(maybeMenuItem);
                }
            };

            BridgeObjectUtil.SetFieldValue(Window.Instance, "androidPostToolbarItemActivated", rawtoolbarItemActivated);
            
            var impl = GetImpl();
            
            var mi = JsonConvert.SerializeObject(menuItems.ToArray());

            if (impl != null) {
                impl.setToolbarItems(mi);
                Logger.Debug(typeof(IawAppApi), "IAWApp setToolbarItems() called");
            } else {
                BridgeObjectUtil.SetFieldValue(Window.Instance, "_iawapp_setToolbarItems", mi);
                Logger.Debug(typeof(IawAppApi), "fake setToolbarItems() called");
            }
        }
        
        public static Task<string> ReuseMediaAssetIdOrHttpGet(string urlToGet, IStorage storage, string variableName) =>
            Task.FromPromise<string>(new TypeSafePromise<string>((onSucc, onFail) => {
                var result = storage.GetStringOrNull(variableName);

                if (result != null && HasMediaAsset(result)) {
                    Logger.Debug(typeof(IawAppApi), "ReuseMediaAssetOrFetch={0} is reusable", result);
                    onSucc(result);
                    return;
                }
    
                Logger.Debug(typeof(IawAppApi), "ReuseMediaAssetOrFetch={0} is not usable", result);
        
                var req = new XMLHttpRequest();
                req.Open("GET", urlToGet, true);
                req.ResponseType = XMLHttpRequestResponseType.ArrayBuffer;
                req.OnLoad = async ev => {
                    var content = new Uint8Array(req.Response).ToString();
                    result = await RegisterMediaAsset(content);
                    storage.Set(variableName, result);
                    Logger.Debug(typeof(IawAppApi), "ReuseMediaAssetOrFetch registered {0}", result);
                    onSucc(result);
                }; 
                req.Send(string.Empty);                        
            }),
            (Func<string,string>)(x => x));
    }
}
