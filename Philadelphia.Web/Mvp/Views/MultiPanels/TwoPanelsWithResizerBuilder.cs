using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class TwoPanelsWithResizerBuilder {
        
        private static BuiltPanels<T> BuildNonhideable<T>(T panels, IFormRenderer<HTMLElement> renderer) 
                where T : TwoPanelsWithResizer {

            var leftCanvas = new ElementWrapperFormCanvas(panels.FirstPanel, Toolkit.DefaultExitButtonBuilder);
            var leftRenderer = renderer.CreateRendererWithBase(leftCanvas);
            
            var rightCanvas = new ElementWrapperFormCanvas(panels.SecondPanel, Toolkit.DefaultExitButtonBuilder);
            var rightRenderer = renderer.CreateRendererWithBase(rightCanvas);
            
            return BuiltPanels<T>.BuiltNonHideable(panels, leftRenderer, rightRenderer, leftCanvas, rightCanvas); 
        }
        
        /// <param name="whichOne">true = first, false=second</param>
        private static BuiltPanels<T> BuildHideable<T>(
                T panels, bool whichOne, bool showable, IFormRenderer<HTMLElement> renderer) 
                    where T : TwoPanelsWithResizer {
            
            var fstIsHideable = whichOne;
            var toBeHidden = fstIsHideable ? panels.FirstPanel : panels.SecondPanel;

            var hideAct = new HTMLDivElement{TextContent = FontAwesomeSolid.IconTimes}
                .With(x => x.AddClasses(
                    Magics.CssClassHideAction, Magics.CssClassEnabled, IconFontType.FontAwesomeSolid.ToCssClassName()));
            var showAct = showable ? new HTMLDivElement{TextContent = FontAwesomeSolid.IconBars}
                .With(x => x.AddClasses(
                    Magics.CssClassShowAction, IconFontType.FontAwesomeSolid.ToCssClassName())) : null;
            
            panels.FirstPanel.AddClasses(Magics.CssClassPositionRelative);
            var leftCanvas = new ElementWrapperFormCanvas(
                panels.FirstPanel, 
                Toolkit.DefaultExitButtonBuilder,
                fstIsHideable ? hideAct : showAct);
            
            var leftRenderer = renderer.CreateRendererWithBase(leftCanvas);

            panels.SecondPanel.AddClasses(Magics.CssClassPositionRelative);
            var rightCanvas = new ElementWrapperFormCanvas(
                panels.SecondPanel, 
                Toolkit.DefaultExitButtonBuilder, 
                fstIsHideable ? showAct : hideAct);
            
            var rightRenderer = renderer.CreateRendererWithBase(rightCanvas);
            
            var lastSize = new MutableHolder<Tuple<double,double>>();
            var shown = new MutableHolder<bool>(true);

            void HideAction() {
                if (!shown.Value) {
                    Logger.Debug(typeof(TwoPanelsWithResizerBuilder), "already hidden");
                    return;
                }

                hideAct.RemoveClasses(Magics.CssClassEnabled);
                if (showable) {
                    showAct.AddClasses(Magics.CssClassEnabled);
                }

                lastSize.Value = panels.Sizes;
                toBeHidden.AddClasses(Magics.CssClassNotRendered);
                panels.Splitter.AddClasses(Magics.CssClassNotRendered);
                shown.Value = false;
                panels.ForceSizeCalculation(VisibilityAction.Hiding);
            }

            void ShowAction() {
                if (shown.Value) {
                    Logger.Debug(typeof(TwoPanelsWithResizerBuilder), "already shown");
                    return;
                }

                if (showable) {
                    showAct.RemoveClasses(Magics.CssClassEnabled);
                }

                hideAct.AddClasses(Magics.CssClassEnabled);
                toBeHidden.RemoveClasses(Magics.CssClassNotRendered);
                panels.Splitter.RemoveClasses(Magics.CssClassNotRendered);
                shown.Value = true;
                panels.ForceSizeCalculation(VisibilityAction.Showing, lastSize.Value);
            }

            hideAct.OnClick += ev => HideAction();
            if (showable) {
                showAct.OnClick += ev => ShowAction();
            }

            return BuiltPanels<T>.BuiltHideable(panels, leftRenderer, rightRenderer, 
                leftCanvas, rightCanvas, HideAction, ShowAction); 
        }
        
        private static BuiltPanels<T> Builder<T>(
            T prototype, Hideability hideable, bool showable, 
            IFormRenderer<HTMLElement> renderer ) where T : TwoPanelsWithResizer {
            
            switch (hideable) {
                case Hideability.None:
                    return BuildNonhideable(prototype, renderer);
                case Hideability.First:
                case Hideability.Second:
                    return BuildHideable(
                        prototype, 
                        hideable == Hideability.First, 
                        showable, 
                        renderer);

                default: throw new Exception("unsupported Hideability");
            }    
        }

        public static BuiltPanels<TwoVerticalPanelsWithResizer> BuildVertical(
                Hideability hideable, bool showable, IFormRenderer<HTMLElement> renderer,
                SpacingPolicy policy = SpacingPolicy.Proportional) {

            return Builder(new TwoVerticalPanelsWithResizer(hideable, policy), hideable, showable, renderer);
        }
        
        public static BuiltPanels<TwoHorizontalPanelsWithResizer> BuildHorizontal(
            Hideability hideable, bool showable, IFormRenderer<HTMLElement> renderer, 
            SpacingPolicy policy = SpacingPolicy.Proportional) {

            return Builder(new TwoHorizontalPanelsWithResizer(hideable, policy), hideable, showable, renderer);
        }
        
        public static BuiltPanels<TwoVerticalPanelsWithResizer> BuildVertical(
            Hideability hideable, bool showable, IFormRenderer<HTMLElement> renderer,
            Tuple<int?,int?> fixedSize) {

            return Builder(new TwoVerticalPanelsWithResizer(hideable, fixedSize), hideable, showable, renderer);
        }
        
        public static BuiltPanels<TwoHorizontalPanelsWithResizer> BuildHorizontal(
            Hideability hideable, bool showable, IFormRenderer<HTMLElement> renderer, 
            Tuple<int?,int?> fixedSize) {

            return Builder(new TwoHorizontalPanelsWithResizer(hideable, fixedSize), hideable, showable, renderer);
        }
    }
}
