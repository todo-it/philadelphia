using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

//FIXME move inline css to css sheet
namespace Philadelphia.Web {
    public enum UploadViewPresentation {
        TextRow,
        ThumbnailGrid
    }

    public enum OpenImagesMethod {
        DownloadAsAttachment=1,
        InlineInNewTab=2,
        Lightbox=3
    }

    public class UploadView : IReadWriteValueView<HTMLElement,List<RemoteFileDescr>>,IBeforeChangeCapableValueView<List<RemoteFileDescr>> {
        class ThumbnailUploadViewAction : IUploadViewAction {
            private readonly UploadView _parent;
            private readonly IDictionary<RemoteFileDescr,AnchorBasedActionView> _known 
                = new Dictionary<RemoteFileDescr, AnchorBasedActionView>();
            
            //assumption: file is downloadable(readable) even if control is disabled.
            //To avoid race conditions file is not downloadable during running operation(s)

            public ThumbnailUploadViewAction(UploadView parent) {
                _parent = parent;
            }
            
            private void UpdateContent(
                    RemoteFileDescr forFile, AnchorBasedActionView widget, bool isOperationRunning) {

                var oldStatusRaw = widget.Widget.GetAttribute(Magics.AttrDataStatus);
                UploadStatus? oldStatus = null;

                if (!string.IsNullOrWhiteSpace(oldStatusRaw)) {
                    oldStatus = EnumExtensions.GetEnumByLabel<UploadStatus>(oldStatusRaw);
                    var changed = oldStatus.Value != forFile.Status;

                    Logger.Debug(GetType(), 
                        "UpdateContentIfNeeded old={0} new={1} changed?={2}", 
                        oldStatus.Value, forFile.Status, changed);

                    if (changed) {
                        widget.Widget.SetAttribute(Magics.AttrDataStatus, forFile.Status.ToString());
                        widget.Widget.RemoveAllChildren();
                    }
                }

                UpdateContentImpl(forFile, oldStatus, widget, isOperationRunning);
            }
            
            private void UpdateContentImpl(
                    RemoteFileDescr forFile, UploadStatus? formerStatus, AnchorBasedActionView widget,
                    bool isOperationRunning) {
                
                var enabledVal = 
                    _parent.Downloadable &&
                    forFile.Status != UploadStatus.Running &&
                    forFile.FileId != null &&
                    !isOperationRunning;

                Logger.Debug(GetType(), 
                    "UpdateContentImpl file={0} formerStatus={1} isOperationRunning={2} enabled={3}", 
                    forFile, formerStatus, isOperationRunning, enabledVal);
                
                widget.Enabled = enabledVal;

                if (formerStatus.HasValue && formerStatus.Value == forFile.Status) {
                    //no need to recreate details
                    return;
                }

                switch (forFile.Status) {
                    case UploadStatus.Running: {
                        widget.State = ActionViewState.CreateOperationRunning();
                        
                        var throbber = new HTMLDivElement {TextContent = FontAwesomeSolid.IconSpinner};
                        throbber.AddClasses(IconFontType.FontAwesomeSolid.ToCssClassName());
                        
                        throbber.Style.FontSize = $"{_parent._cellSize.width/3}px";
                        throbber.Style.SetProperty("animation", "throbberSpin 1.0s linear infinite");

                        widget.Widget.AppendChild(throbber);
                        break; }

                    case UploadStatus.Failed:
                    case UploadStatus.Succeeded:
                        if (formerStatus.HasValue && formerStatus.Value == UploadStatus.Running) {
                            //file just uploaded (succ or fail)
                            SetupOnClick(forFile, widget, false);
                        }
                        
                        if (forFile.UploadErrorMessage != null) {
                            widget.Widget.SetAttribute(
                                Magics.AttrDataErrorsTooltip, forFile.UploadErrorMessage);
                        }

                        if (forFile.ThumbDimensions != null) {
                            var dim = _parent.CalculateDimensionsFittingIntoCell(
                                forFile.ThumbDimensions.Value);
                            var img = new HTMLImageElement {
                                Width = dim.width,
                                Height = dim.height,
                                Src = forFile.ThumbnailDataUrl};
                            widget.Widget.AppendChild(img);
                        } else {
                            var icn = new HTMLDivElement {TextContent = FontAwesomeSolid.IconFileAlt};
                            icn.AddClasses(IconFontType.FontAwesomeSolid.ToCssClassName());
                            icn.Style.FontSize = $"{_parent._cellSize.width/3}px";

                            widget.Widget.AppendChild(icn);
                        } 
                        break;

                    default: throw new Exception("unsupported UploadStatus");
                }
            }
            
            private void SetupOnClick(
                RemoteFileDescr forFile, AnchorBasedActionView widget, bool isInitial) {
                
                if (!_parent.Downloadable) {
                    widget.Href = "#";

                    return;
                }
                
                var effectiveMethod = 
                    _parent.ImageOpenMethod == OpenImagesMethod.Lightbox && forFile.FullDimensions == null ?
                        OpenImagesMethod.DownloadAsAttachment : _parent.ImageOpenMethod;

                switch(effectiveMethod) {
                    case OpenImagesMethod.DownloadAsAttachment:
                        if (isInitial) {
                            widget.Triggered += () => 
                                _parent.RunDownloadOperation(forFile, DownloadMethod.Attachment);
                        }

                        break; 

                    case OpenImagesMethod.InlineInNewTab:
                        widget.Target = "_blank";
                        widget.Href = _parent.BuildHref(forFile, DownloadMethod.Inline);
                        break;

                    case OpenImagesMethod.Lightbox:
                        if (isInitial) {
                            widget.ShouldTriggerOnTarget = x => {
                                var firstChild = widget.Widget.Children[0];
                                var firstChildIsImg = firstChild.TagName == "IMG";
                                return x == widget.Widget || firstChildIsImg && firstChild == x;
                            };

                            widget.Triggered += () => { 
                                new LightBoxManager(x => _parent.BuildHref(forFile, DownloadMethod.Inline))
                                    .Start(widget, forFile);
                            };
                        }
                        break;
                    default: throw new Exception("unsupported OpenImagesMethod");
                }
            }

            public IView<HTMLElement> Create(RemoteFileDescr forFile, Action forceAddOrRemoveToView) {
                var widget = new AnchorBasedActionView();
                
                //save current status
                widget.Widget.SetAttribute(Magics.AttrDataStatus, forFile.Status.ToString());

                widget.Widget.Style.Display = Display.Flex;
                widget.Widget.Style.Width = $"{_parent._cellSize.width}px";
                widget.Widget.Style.Height = $"{_parent._cellSize.height}px";
                widget.Widget.Style.AlignItems = AlignItems.Center;
                widget.Widget.Style.JustifyContent = JustifyContent.Center;
                
                UpdateContentImpl(forFile, null, widget, false);
                SetupOnClick(forFile, widget, true);
                _known.Add(forFile, widget);

                return widget;
            }

            public void Destroy(RemoteFileDescr forFile) {
                _known.Remove(forFile);
            }
            
            public void OnNotifyOperationStart(RemoteFileDescr forFile, IView<HTMLElement> _) {
                _known
                    .Where(x => x.Key == forFile)
                    .ForEach(x => UpdateContent(x.Key, x.Value, true));
            }

            public void OnNotifyOperationEnded(RemoteFileDescr forFile, IView<HTMLElement> _) {
                _known
                    .Where(x => x.Key == forFile)
                    .ForEach(x => UpdateContent(x.Key, x.Value, false));
            }
        }

        class FileNameUploadViewAction : IUploadViewAction {
            private readonly UploadView _parent;
            private readonly IDictionary<RemoteFileDescr,AnchorBasedActionView> _known 
                = new Dictionary<RemoteFileDescr, AnchorBasedActionView>();
             
            //assumption: file is downloadable(readable) even if control is disabled.
            //To avoid race conditions file is not downloadable during running operation(s)

            public FileNameUploadViewAction(UploadView parent) {
                _parent = parent;
            }

            public IView<HTMLElement> Create(RemoteFileDescr forFile, Action forceAddOrRemoveToView) {
                var widget = new AnchorBasedActionView(
                    forFile.GetNotTooLongFileName(
                        _parent.FileNameMaxVisibleLength, _parent.FileNameShortening), 
                    forFile.FileName);
                
                UpdateEnablementAndHref(forFile, widget, false);
                
                var effectiveMethod = 
                    _parent.ImageOpenMethod == OpenImagesMethod.Lightbox && forFile.FullDimensions == null ?
                        OpenImagesMethod.DownloadAsAttachment : _parent.ImageOpenMethod;

                switch(effectiveMethod) {
                    case OpenImagesMethod.DownloadAsAttachment:
                        widget.Triggered += () => {    
                            if (!_parent.Downloadable) {
                                return;
                            }
                            _parent.RunDownloadOperation(forFile, DownloadMethod.Attachment);
                        };
                        break;

                    case OpenImagesMethod.InlineInNewTab:
                        widget.Target = "_blank";
                        widget.Href = _parent.BuildHref(forFile, DownloadMethod.Inline);
                        break;

                    case OpenImagesMethod.Lightbox:
                        widget.ShouldTriggerOnTarget = x => x == widget.Widget;
                        widget.Triggered += () => { 
                            new LightBoxManager(x => _parent.BuildHref(forFile, DownloadMethod.Inline))
                                .Start(widget, forFile);
                        };
                    
                        break;
                        
                    default: throw new Exception("unsupported OpenImagesMethod");
                }
                
                _known.Add(forFile, widget);

                return widget;
            }
            
            private void UpdateEnablementAndHref(RemoteFileDescr forFile, bool isOperationRunning) {
                _known
                    .Where(x => x.Key == forFile)
                    .ForEach(x => UpdateEnablementAndHref(x.Key, x.Value, isOperationRunning));
            }
            
            private void UpdateEnablementAndHref(
                    RemoteFileDescr forFile, AnchorBasedActionView widget, bool isOperationRunning) {
                
                var value = forFile.FileId != null && 
                            _parent.Downloadable && 
                            forFile.Status == UploadStatus.Succeeded &&
                            !isOperationRunning;
                
                Logger.Debug(GetType(), "updating enablement+href for {0} to={1}", forFile, value);
                widget.Enabled = value;

                if (_parent.ImageOpenMethod == OpenImagesMethod.InlineInNewTab) {
                    widget.Target = "_blank";
                    widget.Href = !_parent.Downloadable ? "#" : _parent.BuildHref(forFile, DownloadMethod.Inline); 
                    //note: FileId may be null
                }
            }

            public void Destroy(RemoteFileDescr forFile) {
                _known.Remove(forFile);
            }

            public void OnNotifyOperationStart(RemoteFileDescr forFile, IView<HTMLElement> _) {
                 UpdateEnablementAndHref(forFile, true);
            }

            public void OnNotifyOperationEnded(RemoteFileDescr forFile, IView<HTMLElement> _) {
                UpdateEnablementAndHref(forFile, false);
            }
        }

        class CloseFailedUploadAction : IUploadViewAction {
            private readonly UploadView _parent;
            private readonly IDictionary<RemoteFileDescr,InputTypeButtonActionView> _known 
                = new Dictionary<RemoteFileDescr, InputTypeButtonActionView>();

            public CloseFailedUploadAction(UploadView parent) {
                _parent = parent;
            }

            public IView<HTMLElement> Create(RemoteFileDescr forFile, Action forceAddOrRemoveToView) {
                var widget = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                    IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTimes);
                widget.Widget.Title = I18n.Translate("Hide");
                widget.Triggered += () => _parent.OnRemoveTransient(forFile);
                
                UpdateVisibility(forFile, widget);

                _known.Add(forFile, widget);

                return widget;
            }

            public void Destroy(RemoteFileDescr forFile) {
                _known.Remove(forFile);
            }
            
            private void UpdateVisibility(RemoteFileDescr forFile) {
                _known
                    .Where(x => x.Key == forFile)
                    .ForEach(x => UpdateVisibility(x.Key, x.Value));
            }

            private void UpdateVisibility(RemoteFileDescr forFile, InputTypeButtonActionView widget) {
                widget.Widget.Style.SetProperty("display",
                    forFile.Status == UploadStatus.Failed && forFile.FileId == null ? "" : "none");
            }
            
            public void OnNotifyOperationStart(RemoteFileDescr forFile, IView<HTMLElement> _) {
                Logger.Debug(GetType(), "updating visibility for {0} due to oper starting", forFile);
                UpdateVisibility(forFile);
            }

            public void OnNotifyOperationEnded(RemoteFileDescr forFile, IView<HTMLElement> _) {
                Logger.Debug(GetType(), "updating visibility for {0} due to oper ending", forFile);
                UpdateVisibility(forFile);
            }
        }
        
        class RemoveFileAction : IUploadViewAction {
            private readonly UploadView _parent;
            private readonly IDictionary<RemoteFileDescr,InputTypeButtonActionView> _known 
                = new Dictionary<RemoteFileDescr, InputTypeButtonActionView>();

            public RemoveFileAction(UploadView parent) {
                _parent = parent;
            }

            public IView<HTMLElement> Create(RemoteFileDescr forFile, Action forceAddOrRemoveToView) {
                var widget = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                    IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTrashAlt);
                widget.Widget.Title = I18n.Translate("Remove");
                widget.Triggered += async () => {
                    widget.State = ActionViewState.CreateOperationRunning();
                    var result = await _parent.OnRemove(widget, forFile, forceAddOrRemoveToView);
                    widget.Widget.TextContent = FontAwesomeSolid.IconTrashAlt;
                    widget.State = result.Success ? 
                            ActionViewState.CreateIdleOrSuccess() 
                        : 
                            ActionViewState.CreateOperationFailed(new Exception(result.ErrorMessage));
                };

                UpdateEnablement(forFile, widget, false);

                _known.Add(forFile, widget);
                return widget;
            }

            private void UpdateEnablement(
                    RemoteFileDescr forFile, bool isOperationStarted, IView<HTMLElement> senderOrNull) {

                _known
                    .Where(x => x.Key == forFile)
                    .ForEach(x => {
                        if (x.Value != senderOrNull) {
                            //reset former errors if any
                            x.Value.State = ActionViewState.CreateIdleOrSuccess();
                        }
                        UpdateEnablement(x.Key, x.Value, isOperationStarted);
                    });
            }

            private void UpdateEnablement(
                    RemoteFileDescr forFile, InputTypeButtonActionView widget, bool isOperationStarted) {

                var val = 
                    _parent.Mutable && 
                    _parent.Enabled && 
                    forFile.FileId != null && 
                    !isOperationStarted;

                widget.Enabled = val;
                widget.Widget.Style.Cursor = val ? Cursor.Pointer : Cursor.Default;
            }

            public void Destroy(RemoteFileDescr forFile) {
                _known.Remove(forFile);
            }
            
            public void OnNotifyOperationStart(RemoteFileDescr forFile, IView<HTMLElement> senderOrNull) {
                Logger.Debug(GetType(), "updating enablement for {0} due to oper starting", forFile);
                UpdateEnablement(forFile, true, senderOrNull);
            }

            public void OnNotifyOperationEnded(RemoteFileDescr forFile, IView<HTMLElement> senderOrNull) {
                Logger.Debug(GetType(), "updating enablement for {0} due to oper ending", forFile);
                UpdateEnablement(forFile, false, senderOrNull);
            }
        }

        class ReplaceFileAction : IUploadViewAction {
            private readonly UploadView _parent;
            private readonly IDictionary<RemoteFileDescr,InputTypeButtonActionView> _known 
                = new Dictionary<RemoteFileDescr, InputTypeButtonActionView>();
            private readonly ISet<RemoteFileDescr> _activatedFor = new HashSet<RemoteFileDescr>();
            public ReplaceFileAction(UploadView parent) {
                _parent = parent;
            }

            public IView<HTMLElement> Create(RemoteFileDescr forFile, Action forceAddOrRemoveToView) {
                var widget = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                    IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconExchangeAlt);
                widget.Widget.Title = I18n.Translate("Replace");
                widget.Triggered += () => {
                    //HACK due to lack of: on input[file] activated but user declined to select anything
                    _parent._hackOnUploadStarted = 
                        () => {
                            widget.State = ActionViewState.CreateOperationRunning();
                            _activatedFor.Add(forFile);
                        };
                    
                    _parent.OnReplace(widget, forFile, forceAddOrRemoveToView);
                    
                    _parent._hackOnUploadEnded = result => {
                        _activatedFor.Remove(forFile);
                        widget.Widget.TextContent = FontAwesomeSolid.IconExchangeAlt;
                        widget.State = result.Success ? 
                                ActionViewState.CreateIdleOrSuccess() 
                            : 
                                ActionViewState.CreateOperationFailed(new Exception(result.ErrorMessage));
                        
                        UpdateEnablement(forFile, widget, false); //make action enabled (needed due to hack above)
                    };

                };
                UpdateEnablement(forFile, widget, false);
                _known.Add(forFile, widget);
                return widget;
            }
            
            private void UpdateEnablement(
                    RemoteFileDescr forFile, bool isOperationStarted, IView<HTMLElement> senderOrNull) {

                _known
                    .Where(x => x.Key == forFile)
                    .ForEach(x => {
                        if (x.Value != senderOrNull) {
                            //reset former errors if any
                            x.Value.State = ActionViewState.CreateIdleOrSuccess();
                        }
                    
                        UpdateEnablement(x.Key, x.Value, isOperationStarted);
                    });
            }

            private void UpdateEnablement(
                    RemoteFileDescr forFile, InputTypeButtonActionView widget, bool isOperationStarted) {

                var val = 
                    _parent.Mutable && 
                    _parent.Enabled && 
                    forFile.FileId != null && 
                    !isOperationStarted && 
                    !_activatedFor.Contains(forFile);

                Logger.Debug(GetType(), "UpdateEnablement for {0} value {1}", forFile, val);
                widget.Enabled = val;
            }

            public void Destroy(RemoteFileDescr forFile) {
                _known.Remove(forFile);
            }
            
            public void OnNotifyOperationStart(RemoteFileDescr forFile, IView<HTMLElement> senderOrNull) {
                Logger.Debug(GetType(), "updating enablement for {0} due to oper starting", forFile);
                UpdateEnablement(forFile, true, senderOrNull);
            }

            public void OnNotifyOperationEnded(RemoteFileDescr forFile, IView<HTMLElement> senderOrNull) {
                Logger.Debug(GetType(), "updating enablement for {0} due to oper ending", forFile);
                UpdateEnablement(forFile, false, senderOrNull);
            }
        }

        private Func<IEnumerable<Bridge.Html5.File>, FileUploadOperation, RemoteFileId, Task<RemoteFileId[]>> _uploadHndl;
        private Func<RemoteFileId, Task<FileModel>> _downloadAsAttachmentHndl;
        private Func<RemoteFileId, string> _downloadUsingGetUrlHndl;
        private readonly HTMLElement _container = new HTMLElement();
        private readonly InputTypeButtonActionView _adder = new InputTypeButtonActionView(I18n.Translate("Add"));
        private readonly HTMLInputElement _inp;
        private List<RemoteFileDescr> _files = new List<RemoteFileDescr>();
        private readonly HTMLDivElement _lblCnt,_lblItems,_lbl,_itemsCnt,_items,_drop;
        private readonly HTMLFormElement _inputForm = new HTMLFormElement();

        //HACK variables due to lack of: on input[file] activated but user declined to select anything
        private RemoteFileDescr _hackReplacingItem;
        private IView<HTMLElement> _hackActionToPass;
        private Action _hackOnUploadStarted;
        private Action<ResultHolder<Unit>> _hackOnUploadEnded;

        private bool Downloadable => _downloadAsAttachmentHndl != null;
        private bool Mutable => _uploadHndl != null;
        private readonly IDictionary<RemoteFileDescr,HTMLElement> _knownItems 
            = new Dictionary<RemoteFileDescr,HTMLElement>();
        private (int width,int height) _cellSize;
        private readonly IDictionary<RemoteFileDescr,List<IView<HTMLElement>>> _pendingOps 
            = new Dictionary<RemoteFileDescr, List<IView<HTMLElement>>>();
        private readonly FileNameUploadViewAction _fileNameAction;
        private readonly ThumbnailUploadViewAction _thumbnailAction;

        public HTMLElement Widget => _container;
        public int FileNameMaxVisibleLength {get; set; } = 30;
        public ShortenFileNamePolicy FileNameShortening {get; set; } = ShortenFileNamePolicy.Middle;
        public event UiErrorsUpdated ErrorsChanged;
        public event BeforeValueChangeSimple<List<RemoteFileDescr>> BeforeChange;
        public event ValueChangedSimple<List<RemoteFileDescr>> Changed;
        public bool IsValidating {
            get => _lblItems.ClassList.Contains(Magics.CssClassIsValidating);
            set => _lblItems.AddOrRemoveClass(value, Magics.CssClassIsValidating);
        }
        public bool Enabled {
            get => _adder.Enabled;
            set {
                _adder.Enabled = value;
                RefreshFileList();
            }}
        public ISet<string> DisabledReasons { set => DefaultInputLogic.SetDisabledReasons(_lblItems, value); }
        public List<RemoteFileDescr> Value {
            //return copy so that setter below works...
            get => _files.ToList();
            set {
                _files.Clear();
                _files.AddRange(value ?? new List<RemoteFileDescr>());
                RefreshFileList();
            }}
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_lblItems);
        public bool MultiEnabled {set => _inp.Multiple = value; }
        public bool SortFilesByName { get; set; } = true;
        
        private OpenImagesMethod _imageOpenMethod = OpenImagesMethod.DownloadAsAttachment;
        public OpenImagesMethod ImageOpenMethod {
            get {
                return _imageOpenMethod;
            }
            set {
                if (_files.Any()) {
                    throw new Exception("runtime replacement of ImageOpenMethod is not supported yet");
                }
                _imageOpenMethod = value;
            }
        }

        private List<IUploadViewAction> _customVisualActions = new List<IUploadViewAction>();
        public IEnumerable<IUploadViewAction> VisualActions {
            set {
                if (_files.Any()) {
                    throw new Exception("runtime replacement of VisualActions is not supported yet");
                }
                _customVisualActions = value.ToList();
            }
        }

        private List<IUploadViewAction> _standardVisualActions = new List<IUploadViewAction>();
        private IEnumerable<IUploadViewAction> AllVisualActions { 
            get {
                return _standardVisualActions
                    .Select(x => x)
                    .Concat(_customVisualActions);
            } }

        private int? _gridColumnsCount = 3;
        public int? GridColumnsCount {
            get {
                return _gridColumnsCount;
            }
            set {
                _gridColumnsCount = value;
                _items.Style.SetProperty(
                    "grid-template-columns", 
                    !value.HasValue ? "" : $"repeat({value.Value},auto)");
            }
        }
        
        private UploadViewPresentation _presentationMode;
        public UploadViewPresentation PresentationMode {
            get {
                return _presentationMode;
            }

            private set {
                _presentationMode = value;
                _container.SetAttribute("mode", value.ToString());

                switch (value) {
                    case UploadViewPresentation.ThumbnailGrid:
                        _standardVisualActions.AddRange(_fileNameAction, _thumbnailAction);
                        break;

                    case UploadViewPresentation.TextRow:
                        _standardVisualActions.AddRange(_fileNameAction);
                        break;

                    default: throw new Exception("unsupported UploadViewPresentation");
                }
            }
        }
        
        public UploadView(string label = "") {
            _fileNameAction = new FileNameUploadViewAction(this);
            _thumbnailAction = new ThumbnailUploadViewAction(this);
            PresentationMode = UploadViewPresentation.TextRow;
            
            VisualActions = new [] {
                BuildRemoveFileAction(), BuildReplaceFileAction(), BuildCloseFailedUploadAction() };

            //have to cancel default dragover+dragenter events to enable dropping 
            //see https://developer.mozilla.org/en-US/docs/Web/API/HTML_Drag_and_Drop_API/Drag_operations#droptargets
            _drop = new HTMLDivElement {TextContent = I18n.Translate("Drop files here")};
            _drop.OnDragOver += ev => {
                if (!Mutable) {
                    return;
                }
                ev.PreventDefault();
            };
            _drop.OnDragEnter += ev => {
                if (!Mutable) {
                    return;
                }
                ev.PreventDefault();
            };
            _drop.OnDrop += async rawEv => {
                if (!Mutable) {
                    return;
                }
                rawEv.PreventDefault();
                
                Document.Body.RemoveAttribute(Magics.AttrDataDraggingFile); //TODO do it in cleaner way?

                var dt = (DataTransfer)rawEv.GetFieldValue("dataTransfer");
                Logger.Debug(GetType(), "dragged files count={0}", dt.Files.Length);

                await OnUpload(null, dt.Files.ToList(), null, RefreshFileList);
            };
            _container.AppendChild(_drop);

            _container.ClassList.Add(GetType().FullNameWithoutGenerics());
            
            _lblItems = new HTMLDivElement();
            _lbl = new HTMLDivElement {TextContent = label};
            _lblItems.AppendChild(_lbl);

            _lblCnt = new HTMLDivElement();
            _lblCnt.AppendChild(_lblItems);
            _container.AppendChild(_lblCnt);

            _itemsCnt = new HTMLDivElement();
            _items = new HTMLDivElement {ClassName = Magics.CssClassItems};
            GridColumnsCount = GridColumnsCount; //to initialize inline style
            _items.SetAttribute(Magics.AttrDataEmptyCaption, I18n.Translate("There are no files"));
            _itemsCnt.AppendChild(_items);

            _container.AppendChild(_itemsCnt);
            
            //hidden input[file]
            _inp = new HTMLInputElement {Type = InputType.File};
            _inp.Style.Display = Display.None;
            _inp.OnChange += async ev => await OnInputChange(ev);
            _adder.Triggered += () => {
                _hackReplacingItem = null;
                _inp.Click();
            };
            _inputForm.AppendChild(_inp);
            _lblItems.AppendChild(_inputForm);

            _lblItems.AppendChild(_adder.Widget);
            _lblItems.AppendChild(new HTMLDivElement {TextContent = I18n.Translate("...or drop files here")} );
            
            UpdateMutability();
        }

        private void RunDownloadOperation(
                RemoteFileDescr forFile, DownloadMethod mth=DownloadMethod.Attachment) {
            
            var f = RemoteFileId.CreateRequest(forFile.FileId);
            f.DwnMthd = mth;

            _downloadAsAttachmentHndl(f);
        }

        private string BuildHref(
                RemoteFileDescr forFile, DownloadMethod mth=DownloadMethod.ServerDefault) {

            var f = RemoteFileId.CreateRequest(forFile.FileId);
            f.DwnMthd = mth;

            return _downloadUsingGetUrlHndl(f);
        }

        public void ChangePresentationToThumbnailGrid((int width,int height) cellDimensions) {
            PresentationMode = UploadViewPresentation.ThumbnailGrid;
            _cellSize = cellDimensions;
        }
        
        public void ChangePresentationToTextRows() {
            PresentationMode = UploadViewPresentation.TextRow;
        }

        private void OnStartedOperation(IView<HTMLElement> senderOrNull, RemoteFileDescr itm) {
            List<IView<HTMLElement>> lst;
            if (!_pendingOps.TryGetValue(itm, out lst)) {
                lst = new List<IView<HTMLElement>>();
                _pendingOps.Add(itm, lst);
            }
            lst.Add(senderOrNull);
            Logger.Debug(GetType(), "added - there are now {0} operations for item {1}", lst.Count, itm);

            if (lst.Count != 1) {
                //notify only for 1st operation
                return;
            }

            Logger.Debug(GetType(), "notifying {0} visualActions", AllVisualActions.Count());

            AllVisualActions.ForEach(x => x.OnNotifyOperationStart(itm, senderOrNull));
        }
        
        private void OnFinishedOperation(IView<HTMLElement> senderOrNull, RemoteFileDescr itm) {
            List<IView<HTMLElement>> lst;
            if (!_pendingOps.TryGetValue(itm, out lst)) {
                Logger.Error(GetType(), "bug in removal - there are no operations started for item {0}", itm);
                return;
            }
            lst.Remove(senderOrNull);

            Logger.Debug(GetType(), "removed - there are now {0} operations for item {1}", lst.Count, itm);
            
            if (lst.Any()) {
                //notify only when finished everything
                return;
            }

            Logger.Debug(GetType(), "notifying {0} visualActions", AllVisualActions.Count());

            AllVisualActions.ForEach(x => x.OnNotifyOperationEnded(itm, senderOrNull));
        }

        public void SetErrors(ISet<string> errors, bool causedByUser) {
            DefaultInputLogic.SetErrors(_container, _lblItems, causedByUser, errors);
            ErrorsChanged?.Invoke(this, errors);
        }

        private void UpdateMutability() {
            if (Mutable) {
                _container.SetAttribute(Magics.AttrDataMutable, "");
                _container.RemoveAttribute(Magics.AttrDataNonMutable);
                return;
            }

            _container.SetAttribute(Magics.AttrDataNonMutable, "");
            _container.RemoveAttribute(Magics.AttrDataMutable);
        }

        public void SetImplementation(BaseDownloadUploadHandler handler) {
            _uploadHndl = handler.UploadOrNull;
            _downloadAsAttachmentHndl = handler.DownloadOrNull;
            _downloadUsingGetUrlHndl = handler.DownloadUrlOrNull;
            UpdateMutability();
        }

        public void SetImplementation(
                Func<IEnumerable<Bridge.Html5.File>,FileUploadOperation,RemoteFileId,Task<RemoteFileId[]>> uploadHndlOrNull = null,
                Func<RemoteFileId, Task<FileModel>> downloadHndlOrNull=null,
                Func<RemoteFileId, string> downloadUsingGetUrl = null) {
            
            _uploadHndl = uploadHndlOrNull;
            _downloadAsAttachmentHndl = downloadHndlOrNull;
            _downloadUsingGetUrlHndl = downloadUsingGetUrl;
            UpdateMutability();
        }

        private bool PreValidateReturningMayContinue(
                bool isFromUser, RemoteFileDescr removingItemOrNull, 
                RemoteFileDescr replacingItemOrNull, List<RemoteFileDescr> addingItemsOrNull) {

            if (BeforeChange == null) {
                return true;
            }

            var newVal = Value.Select(x => x.CloneIt()).ToList();
            if (removingItemOrNull != null) {
                var toRem = newVal.First(x => x.FileId == removingItemOrNull.FileId);
                newVal.Remove(toRem);
            }
            if (replacingItemOrNull != null) {
                var toRepl = newVal.First(x => x.FileId == replacingItemOrNull.FileId);
                newVal.Remove(toRepl);
            }
            if (addingItemsOrNull != null) {
                newVal.AddRange(addingItemsOrNull);
            }

            var errs = new HashSet<string>();
            Logger.Debug(GetType(), "invoking BeforeChange for newvalue.lenght={0}", newVal.Count);

            BeforeChange(newVal, isFromUser, errs.AddRange);

            Logger.Debug(GetType(), "BeforeChange returned {0} problems", errs.Count);
            
            if (errs.Any()) {
                SetErrors(errs, isFromUser);
            }
            return !errs.Any();
        }
      
        public IUploadViewAction BuildCloseFailedUploadAction() {
            return new CloseFailedUploadAction(this);
        }

        public IUploadViewAction BuildRemoveFileAction() {
            return new RemoveFileAction(this);
        }
        
        public IUploadViewAction BuildReplaceFileAction() {
            return new ReplaceFileAction(this);
        }
        
        private HTMLElement RefreshFileTextItemAdder(RemoteFileDescr x) {
            var el = new HTMLDivElement();

            el.AppendChild(_fileNameAction.Create(x, RefreshFileList).Widget);
            el.SetAttribute("data-status", x.Status.ToString());
            
            _customVisualActions.ForEach(bld => el.AppendChild(bld.Create(x, RefreshFileList).Widget));
            
            return el;
        }
        
        private HTMLElement RefreshFileImgItemAdder(RemoteFileDescr x) {
            var el = new HTMLDivElement();
            el.Style.Width = $"{_cellSize.width}px";

            el.AppendChild(_thumbnailAction.Create(x, RefreshFileList).Widget);
        
            var actionsBar = new HTMLDivElement {ClassName = Magics.CssClassActions};
            actionsBar.Style.WhiteSpace = WhiteSpace.NoWrap;
            actionsBar.AppendChild(_fileNameAction.Create(x, RefreshFileList).Widget);

            el.SetAttribute("data-status", x.Status.ToString());

            _customVisualActions.ForEach(bld => actionsBar.AppendChild(bld.Create(x, RefreshFileList).Widget));
            
            el.AppendChild(actionsBar);

            return el;
        }

        private void RefreshFileList() {
            if (SortFilesByName) {
                _files = _files.OrderBy(x => x.FileName).ToList();
            }
            
            _knownItems
                .Where(x => !_files.Contains(x.Key))
                .ToList()
                .ForEach(x => {
                    Logger.Debug(GetType(), "removing view item that is deleted in model {0}", x.Key);
                    _knownItems.Remove(x);
                    _items.RemoveChild(x.Value);
                });

            _files.ForEach(x => {
                //already created?
                HTMLElement itm;
                if (_knownItems.TryGetValue(x, out itm)) {
                    Logger.Debug(GetType(), "not creating view item for already existing {0}", x);
                    itm.SetAttribute("data-status", x.Status.ToString());
                    return;
                }
                
                Logger.Debug(GetType(), "creating view item for {0}", x);
                
                switch (PresentationMode) {
                    case UploadViewPresentation.TextRow:
                        itm = RefreshFileTextItemAdder(x);
                        break;

                    case UploadViewPresentation.ThumbnailGrid:
                        itm = RefreshFileImgItemAdder(x);
                        break;

                    default: throw new Exception("unsupported UploadViewPresentation value");
                }
                _knownItems.Add(x, itm);
                _items.AppendChild(itm);
            });
        }
        
        private void OnRemoveTransient(RemoteFileDescr itm) {
            Logger.Debug(GetType(), "removing transient item={0} placeholder", itm.FileId);

            //item is being removed - no need to cleanup properly
            if (_pendingOps.ContainsKey(itm)) {
                _pendingOps.Remove(itm);
            }
            
            AllVisualActions.ForEach(x => x.Destroy(itm));
            _files.Remove(itm);
            RefreshFileList();
        }

        private async Task<ResultHolder<Unit>> OnRemove(
                IView<HTMLElement> sender, RemoteFileDescr itm, Action forceRefreshView) {

            Logger.Debug(GetType(), "removing item={0}", itm.FileId);

            if (!PreValidateReturningMayContinue(true, itm, null, null)) {
                return ResultHolder<Unit>.CreateSuccess(Unit.Instance);
            }
            
            OnStartedOperation(sender, itm);

            try {
                await _uploadHndl(
                    new Bridge.Html5.File[0], FileUploadOperation.Remove, itm.AsRemoteFileIdAndName());

                Logger.Debug(GetType(), "removal succeeded");
                OnFinishedOperation(sender, itm);
                OnRemoveTransient(itm);
                Changed?.Invoke(Value, true);

                return ResultHolder<Unit>.CreateSuccess(Unit.Instance);
            } catch(Exception ex) {
                Logger.Debug(GetType(), "removal failed {0}", ex);
                OnFinishedOperation(sender, itm);
                return ResultHolder<Unit>.CreateFailure(ex.Message.TillFirstNewLineOrEverything());
            }
        }
        
        private void OnReplace(IView<HTMLElement> sender, RemoteFileDescr itm, Action forceRefresh) {
            _hackReplacingItem = itm;
            _hackActionToPass = sender;
            _inp.Click();
        }
        
        /// <param name="senderOrNull">null as file(s) can be dragged into field</param>
        private async Task<ResultHolder<Unit>> OnUpload(
                IView<HTMLElement> senderOrNull, List<Bridge.Html5.File> files, 
                RemoteFileDescr replacing, Action forceRefreshView) {

            var uploadBatchId = UniqueIdGenerator.Generate();
            
            var newFiles = files
                .OrderBy(x => x.Name)
                .Select(x => new RemoteFileDescr {
                    Status = UploadStatus.Running,
                    FileName = x.Name,
                    TmpUploadJobId = uploadBatchId
                } )
                .ToList();
            
            newFiles.ForEach(x => OnStartedOperation(senderOrNull, x));

            if (!PreValidateReturningMayContinue(true, null, replacing, newFiles)) {
                return ResultHolder<Unit>.CreateSuccess(Unit.Instance);
            }
            
            if (replacing != null) {
                OnStartedOperation(senderOrNull, replacing);
            }

            _files.AddRange(newFiles);
            forceRefreshView();
            
            try {
                _inputForm.Reset();
                var handles = await _uploadHndl(
                    files.AsEnumerable(), 
                    FileUploadOperation.Add, 
                    replacing?.AsRemoteFileId());

                Logger.Debug(GetType(), "uploading succeeded. Handles count={0} firstId={1}", 
                    handles.Length, handles.FirstOrDefault()?.FileId);

                newFiles
                    .ForEach(x => {
                        var itm = handles.FirstOrDefault(y => y.FileName == x.FileName);
                        if (itm == null) {
                            x.Status = UploadStatus.Failed;
                            OnUploadFailed(x, I18n.Translate("Server didn't process this file"));
                        } else {
                            x.Status = UploadStatus.Succeeded;
                            x.FileId = itm.FileId;
                            x.ThumbnailDataUrl = itm.ThumbnailDataUrl;
                            x.ThumbDimensions = (itm.ThumbWidth>0 && itm.ThumbHeight>0) ? 
                                    (itm.ThumbWidth, itm.ThumbHeight) 
                                :
                                    ((int,int)?)null;
                            x.FullDimensions = (itm.FullWidth>0 && itm.FullHeight>0) ? 
                                    (itm.FullWidth, itm.FullHeight) 
                                :
                                    ((int,int)?)null;
                        }
                        OnFinishedOperation(senderOrNull, x);
                    });
                
                if (replacing != null) {
                    OnFinishedOperation(senderOrNull, replacing);
                    OnRemoveTransient(replacing);
                }

                Changed?.Invoke(Value, true);
            } catch(Exception ex) {
                var errMsg = ex.Message.TillFirstNewLineOrEverything();
                newFiles
                    .ForEach(x => {
                        x.Status = UploadStatus.Failed;
                        OnUploadFailed(x, errMsg);
                        OnFinishedOperation(senderOrNull, x);
                    });
                
                if (replacing != null) {
                    OnFinishedOperation(senderOrNull, replacing);
                }

                forceRefreshView();
                return ResultHolder<Unit>.CreateFailure(errMsg);
            }

            return ResultHolder<Unit>.CreateSuccess(Unit.Instance);
        }

        private void OnUploadFailed(RemoteFileDescr file, string errMsg) {
            HTMLElement itemDiv;
            if (!_knownItems.TryGetValue(file, out itemDiv)) {
                Logger.Error(GetType(), "cannot find item for file={0} to set error={1}", file, errMsg);
                return;
            }

            //within item DIV there's an A with DIV
            var aInItemDiv = itemDiv.Children[0];
            var aDivInAInItemDiv = aInItemDiv.Children[0];

            DefaultInputLogic.SetErrorsTooltip(aInItemDiv, new HashSet<string>(new []{ errMsg }));

            switch (PresentationMode) {
                case UploadViewPresentation.ThumbnailGrid:
                    //changing solid-throbber into solid-exclamation
                    aDivInAInItemDiv.TextContent = FontAwesomeSolid.IconExclamationTriangle;
                    aDivInAInItemDiv.Style.SetProperty("animation", "");
                    aDivInAInItemDiv.Style.SetProperty("color", "red");
                    DefaultInputLogic.SetErrorsTooltip(aDivInAInItemDiv, new HashSet<string>(new []{ errMsg }));
                    break;

                case UploadViewPresentation.TextRow:
                    break;

                default: throw new Exception("unsupported UploadViewPresentation");
            }
        }

        private async Task OnInputChange(Event<HTMLInputElement> ev) {
            var files = ev.CurrentTarget.Files.ToList();
            
            Logger.Debug(GetType(), 
                "choose files count={0} replaceMode?={1} isFromUser={2}", 
                files.Count, _hackReplacingItem, ev.IsUserGenerated());
            if (files.Count < 1) {
                return;
            }
            
            //HACK due to lack of: on input[file] activated but user declined to select anything
            _hackOnUploadStarted?.Invoke();
            var result = await OnUpload(_hackActionToPass, files, _hackReplacingItem, RefreshFileList);
            _hackOnUploadEnded?.Invoke(result);
        }
        
        private (int width,int height) CalculateDimensionsFittingIntoCell((int width,int height) asked) {
            return DimensionsUtil.CalculateDimensionsNotLargerThan(asked, _cellSize);
        }

        public static implicit operator RenderElem<HTMLElement>(UploadView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
