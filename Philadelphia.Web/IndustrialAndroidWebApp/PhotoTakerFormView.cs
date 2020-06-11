using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PhotoTakerFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For();
        public InputTypeButtonActionView TakePhoto { get; } = new InputTypeButtonActionView(new LabelDescr {
            Label = I18n.Translate("Take photo"),
            PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconCamera)
        });
        public InputTypeButtonActionView AcceptPhoto { get; } = new InputTypeButtonActionView(new LabelDescr {
            Label = I18n.Translate("Accept"),
            PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconCheck)
        });
        public InputTypeButtonActionView RetryPhoto { get; } = new InputTypeButtonActionView(new LabelDescr {
            Label = I18n.Translate("Retry"),
            PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconRedo)
        });

        private HTMLImageElement _img;
        public HTMLDivElement Content { get; } = new HTMLDivElement();

        public HTMLInputElement InputFile { get; } = new HTMLInputElement()
            .With(x => x.Type = InputType.File)
            .With(x => x.Style.Display = Display.None);

        private readonly Dictionary<int, Tuple<double, double>> _touches = new Dictionary<int, Tuple<double, double>>();
        private double _imgScale = 1.0;
        private double _touchScalingFactor = 0.01;
        private double _touchTranslateFactor = 0.1;
        private double _translateX;
        private double _translateY;
        private readonly HTMLFormElement _imgFrm;
        private readonly HTMLDivElement _photoActions;
        
        public PhotoTakerFormView() {
            _imgFrm = new HTMLFormElement();
            
            //TODO extract into stylesheet
            _img = new HTMLImageElement()
                .With(x => x.Style.Display = Display.None)
                .With(x => x.Style.Position = Position.Absolute)
                .With(x => x.Style.MaxWidth = "100vw")
                .With(x => x.Style.MaxHeight = "100vh")
                .With(x => x.Style.Margin = "auto")
                .With(x => x.Style.Left = "0")
                .With(x => x.Style.Top = "0")
                .With(x => x.Style.Right = "0")
                .With(x => x.Style.Bottom = "0");
            _imgFrm.AppendChild(InputFile);
            
            _photoActions = new HTMLDivElement();
            _photoActions.AppendAllChildren(TakePhoto.Widget, RetryPhoto.Widget, AcceptPhoto.Widget);
            
            //TODO extract into stylesheet
            _photoActions.Style.Display = Display.Flex;
            _photoActions.Style.ZIndex = "100";
            _photoActions.Style.BackgroundColor = "white";
            _photoActions.Style.Position = Position.Fixed;
            _photoActions.Style.Bottom = "0";
            _photoActions.Style.Width = "100vw";
            _photoActions.Style.JustifyContent = "flex-end";
            _photoActions.Style.PaddingTop = "10px";
            _photoActions.Style.BackgroundColor = "#f4f4f4";
            
            _img.OnTouchStart += evs => {
                Logger.Debug(GetType(), "OnTouchStart count={0}", evs.Touches.Length);
                evs.ChangedTouches.ForEachI((i,x) => {
                    Logger.Debug(GetType(), "OnTouchStart el={0} pageX={1} pageY={2} id={3}", i, x.PageX, x.PageY, x.Identifier);
                    _touches[x.Identifier] = Tuple.Create((double)x.PageX, (double)x.PageY);
                });
            };
            _img.OnTouchEnd += eve => {
                Logger.Debug(GetType(), "OnTouchEnd count={0}", eve.Touches.Length);
                eve.ChangedTouches.ForEachI((i,x) => {
                    Logger.Debug(GetType(), "OnTouchEnd el={0} pageX={1} pageY={2} id={3}", i, x.PageX, x.PageY, x.Identifier);
                    _touches.Remove(x.Identifier);
                });
            };
            
            _img.OnTouchMove += evm => {
                Logger.Debug(GetType(), "OnTouchMove count={0} known={1}", evm.Touches.Length, _touches.Count);

                _touches.ForEach(x => Logger.Debug(GetType(), "known pageX={0} pageY={1} id={2}", x.Value.Item1, x.Value.Item2, x.Key) );

                double? oldLength = null;
                Tuple<double,double> oldPos = null;

                if (_touches.Count == 1) {
                    oldPos = Tuple.Create(_touches.First().Value.Item1, _touches.First().Value.Item2);
                } else if (_touches.Count == 2) {
                    var x = _touches.ToList();
                    oldLength = Math.Sqrt(
                        Math.Pow(x[0].Value.Item1 - x[1].Value.Item1, 2) +
                        Math.Pow(x[0].Value.Item2 - x[1].Value.Item2, 2));
                }

                evm.ChangedTouches.ForEachI((i,x) => {
                    Logger.Debug(GetType(), "OnTouchMove el={0} pageX={1} pageY={2} id={3}", i, x.PageX, x.PageY, x.Identifier);
                    _touches[x.Identifier] = Tuple.Create((double)x.PageX, (double)x.PageY);
                });
                
                double? newLength = null;
                Tuple<double,double> newPos = null;
                
                if (_touches.Count == 1) {
                    newPos = Tuple.Create(_touches.First().Value.Item1, _touches.First().Value.Item2);
                } if (_touches.Count == 2) {
                    var x = _touches.ToList();
                    newLength = Math.Sqrt(
                        Math.Pow(x[0].Value.Item1 - x[1].Value.Item1, 2) +
                        Math.Pow(x[0].Value.Item2 - x[1].Value.Item2, 2));
                }
                
                Logger.Debug(GetType(), "OnTouchMove oldLength={0} newLength={1}", oldLength, newLength);

                var updateStyle = false;
                
                if (oldLength != null && newLength != null) {
                    var changeScaleBy = (double)((oldLength - newLength) * _touchScalingFactor);
                
                    Logger.Debug(GetType(), "OnTouchMove changeScaleBy={0} oldScale={1}", changeScaleBy, _imgScale);
                    _imgScale -= changeScaleBy;

                    if (_imgScale < 0.5) {
                        _imgScale = 0.5;
                    } else if (_imgScale > 10) {
                        _imgScale = 10;
                    }

                    updateStyle = true;
                }

                if (oldPos != null && newPos != null) {
                    Logger.Debug(GetType(), "OnTouchMove oldPos=({0};{1}) newPos=({2};{3})", oldPos.Item1, oldPos.Item2, newPos.Item1, newPos.Item2);
                    
                    var changeTranslateXBy = (oldPos.Item1 - newPos.Item1) * _touchTranslateFactor;
                    var changeTranslateYBy = (oldPos.Item2 - newPos.Item2) * _touchTranslateFactor;
                    
                    Logger.Debug(GetType(), "OnTouchMove translatedBy=({0}; {1}) oldTranslated=({2};{3})", changeTranslateXBy, changeTranslateYBy, _translateX, _translateY);

                    _translateX -= changeTranslateXBy;
                    _translateY -= changeTranslateYBy;
                    
                    updateStyle = true;
                }

                if (updateStyle) {
                    UpdateStyle(true);
                }
            };
        }

        private void UpdateStyle(bool hasImage) {
            //TODO extract into stylesheet
            AcceptPhoto.Widget.Style.Display = hasImage ? "" : "none";
            RetryPhoto.Widget.Style.Display = hasImage ? "" : "none";
            TakePhoto.Widget.Style.Display = hasImage ? "none" : "";
        
            _img.Style.Transform = $"scale({_imgScale}) translate({_translateX}px, {_translateY}px)";
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                _imgFrm,
                _img,
                Content,
                _photoActions
            }; 
        }

        public void ClearImage() {
            //TODO extract into stylesheet
            _img.Style.Display = Display.None;
            
            _imgFrm.Reset();
            ResetPreviewStyle(false);
        }

        public void SetImageFromDataUrl(string frResult) {
            _img.Src  = frResult;
            
            //TODO extract into stylesheet
            _img.Style.SetProperty("display", "");
            
            ResetPreviewStyle(true);
        }

        public void ResetPreviewStyle(bool hasImg) {
            _imgScale = 1;
            _translateX = 0;
            _translateY = 0;
            UpdateStyle(hasImg);
        }
    }
}
