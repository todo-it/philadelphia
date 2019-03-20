using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// Implementation notes: 
    /// -mousemove has to listen on body as otherwise if mouse escapes div (=_splitter) rectangle then it is not raised.
    /// -touchmove doesn't need tricks like mousemove as it is called even if touch escapes rectangle's area
    /// </summary>
    public abstract class TwoPanelsWithResizer : IView<HTMLElement> {
        public const int DefaultPanelSizePx = 30;
        public Tuple<double,double> Sizes {get; private set;} = Tuple.Create(-1.0, -1.0);

        private readonly HTMLElement _splitter = new HTMLElement("div");
        private bool _isDragging;
        private int _touchId;

        private readonly HTMLElement _container = new HTMLElement("div");

        protected HTMLElement Container => _container;
        public HTMLElement Splitter => _splitter;
        public HTMLElement FirstPanel { get; } = new HTMLElement();
        public HTMLElement SecondPanel { get; } = new HTMLElement();
        public HTMLElement Widget => _container;
        
        protected abstract void SetPanelsSize(Tuple<double,double> sizes);
        protected abstract Tuple<double,double> CalculateSizesOnAttachOrResize(VisibilityAction change, Tuple<double,double> lastSizeOrNull = null);
        protected abstract Tuple<double,double> CalculateSizesOnUserResize(int pageX, int pageY);
        protected int MinPanelSizePx {get; }
        protected readonly Hideability Hideable;
        private readonly Tuple<int?,int?> _fixedSize;
        private readonly SpacingPolicy? _spacingPolicy;

        protected TwoPanelsWithResizer(
                Hideability hideable, int minPanelSizePx, Tuple<int?,int?> fixedSize, SpacingPolicy? spacingPolicy) {

            MinPanelSizePx = minPanelSizePx;

            if (fixedSize == null && !spacingPolicy.HasValue || 
                fixedSize != null && spacingPolicy.HasValue ||
                fixedSize != null && fixedSize.Item1.HasValue && fixedSize.Item2.HasValue || 
                fixedSize != null && !fixedSize.Item1.HasValue && !fixedSize.Item2.HasValue) {

                throw new Exception("Either spacing policy needs to be provided OR exactly one fixedSize dimension");
            }

            Hideable = hideable;
            _fixedSize = fixedSize;
            _spacingPolicy = spacingPolicy;
            _container.ClassName = GetType().FullName;
            _container.Id = UniqueIdGenerator.GenerateAsString();
            _splitter.ClassName = Magics.CssClassSplitter;
            
            _container.AppendChild(FirstPanel);
            _container.AppendChild(_splitter);
            _container.AppendChild(SecondPanel);
            
            _splitter.OnTouchStart += x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }

                var htmlTarget = x.HtmlTarget();

                if (!htmlTarget.IsElementOrItsDescendant(_splitter)) {
                    return;
                }
             
                _isDragging = true;
                _touchId = x.TargetTouches[0].Identifier;
                Logger.Debug(GetType(), "TouchStart {0}", _touchId);
            };
            
            _splitter.OnTouchEnd += ev => {
                _isDragging = false;
                _touchId = 0;
                Logger.Debug(GetType(), "TouchEnd {0}", _touchId);
            };

            _splitter.OnTouchMove += ev => {                
                if (!_isDragging) {
                    return;
                }

                var touch = ev.Touches.FirstOrDefault(x => x.Identifier == _touchId);

                Logger.Debug(GetType(), "TouchMove {0} present?={1}", _touchId, touch != null);

                if (touch == null) {
                    return;
                }

                var sizes = CalculateSizesOnUserResize(touch.PageX, touch.PageY);

                if (sizes.Item1 < minPanelSizePx || sizes.Item2 < minPanelSizePx) {
                    return;
                }
                Logger.Debug(GetType(), "updating sizes for panelType={0} id={1} to ({2}; {3})", GetType().FullName, _container.Id, sizes.Item1, sizes.Item2);

                _container.AddClasses(Magics.CssClassActive);
                ev.PreventDefault();
                Sizes = sizes;
                SetPanelsSize(sizes);
            };

            DocumentUtil.AddMouseDownListener(_splitter, x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }

                var htmlTarget = x.HtmlTarget();

                if (!htmlTarget.IsElementOrItsDescendant(_splitter)) {
                    return;
                }
             
                _isDragging = true;
                x.PreventDefault();
            });

            DocumentUtil.AddMouseUpListener(_splitter, x => {
                _isDragging = false;
            });

            DocumentUtil.AddMouseMoveListener(_splitter, ev => {
                if (!_isDragging) {
                    return;
                }
                ev.PreventDefault();

                var sizes = CalculateSizesOnUserResize(ev.PageX, ev.PageY);

                if (sizes.Item1 < minPanelSizePx || sizes.Item2 < minPanelSizePx) {
                    return;
                }
                Logger.Debug(GetType(), "updating sizes for panelType={0} id={1} to ({2}; {3})", GetType().FullName, _container.Id, sizes.Item1, sizes.Item2);

                _container.AddClasses(Magics.CssClassActive);
                
                Sizes = sizes;
                SetPanelsSize(sizes);
            });

            DocumentUtil.AddElementAttachedToDocumentListener(_container, InitializeWidthsOnAttachOrResize);
            DocumentUtil.AddElementResizeListener(_container, InitializeWidthsOnAttachOrResize);
        }
        
        protected Tuple<double,double> ComputeSpace(double fstDemanded, double sndDemanded, double available) {
            double fstResult,sndResult;

            if (_fixedSize != null) {
                if (_fixedSize.Item1.HasValue) {
                    fstResult = Math.Min(available, _fixedSize.Item1.Value);
                    sndResult = available - fstResult;
                } else {
                    sndResult = Math.Min(available, _fixedSize.Item2.Value);
                    fstResult = available - sndResult;
                }

                return Tuple.Create(fstResult, sndResult);
            }
            
            switch (_spacingPolicy.Value) {
                case SpacingPolicy.Proportional:
                    var firstPct = fstDemanded/(fstDemanded+sndDemanded);
                    fstResult = available * firstPct;
                    sndResult = available - fstResult;
                    break;

                case SpacingPolicy.FirstWins:
                    fstResult = Math.Min(available, fstDemanded);
                    sndResult = available - fstResult;
                    break;

                case SpacingPolicy.SecondWins:
                    sndResult = Math.Min(available, sndDemanded);
                    fstResult = available - sndResult;
                    break;

                default: throw new Exception("unsupported sizing policy");
            }

            return Tuple.Create(fstResult, sndResult);
        }

        public void ForceSizeCalculation(VisibilityAction change, Tuple<double,double> lastSizeOrNull = null) {
            var sizes = CalculateSizesOnAttachOrResize(change, lastSizeOrNull);
            Sizes = sizes;
            Logger.Debug(GetType(), "forced size update for panelType={0} id={1} to ({2}; {3})", GetType().FullName, _container.Id, sizes.Item1, sizes.Item2);
            SetPanelsSize(sizes);
        }

        private void InitializeWidthsOnAttachOrResize() {
            var sizes = CalculateSizesOnAttachOrResize(VisibilityAction.Showing, null);
            Sizes = sizes;
            Logger.Debug(GetType(), "initializing sizes for panelType={0} id={1} to ({2}; {3})", GetType().FullName, _container.Id, sizes.Item1, sizes.Item2);
            SetPanelsSize(sizes);
        }
    }
}
