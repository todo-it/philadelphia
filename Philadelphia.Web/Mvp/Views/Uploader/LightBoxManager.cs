using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LightBoxManager {
        private readonly Func<RemoteFileDescr,string> _urlBuilder;

        public LightBoxManager(Func<RemoteFileDescr,string> urlBuilder) {
            _urlBuilder = urlBuilder;
        }

        private (int width, int height) CalculateDimensionsFittingIntoBrowserWindow(
            (int width, int height) asked, int extraPadding=0) {

            return DimensionsUtil.CalculateDimensionsNotLargerThan(
                asked, (Window.InnerWidth - extraPadding, Window.InnerHeight - extraPadding));
        }

        public void Start(IView<HTMLElement> parentForModal, RemoteFileDescr forFile) {
            Start(parentForModal.Widget, forFile);
        }

        public void Start(HTMLElement parentForModal, RemoteFileDescr forFile) {
            var generatedImageUrl = _urlBuilder(forFile);
            
            var dim = CalculateDimensionsFittingIntoBrowserWindow(
                forFile.FullDimensions.Value, Magics.DefaultLightBoxOuterMargin);

            var glass = new HTMLDivElement {ClassName = Magics.CssClassGlass};
            var lightboxCnt = new HTMLDivElement();
            lightboxCnt.AddClasses(
                Magics.CssClassLightBox, Magics.CssClassLightBoxLoading);
                
            lightboxCnt.Style.Width = $"{dim.Item1}px";
            lightboxCnt.Style.Height = $"{dim.Item2}px";
            
            var throbber = new HTMLDivElement {
                TextContent = Magics.FontAwesomeSpinner};
            throbber.AddClasses(
                Magics.CssClassUsesFontAwesome, Magics.CssClassLightBoxThrobber);
            lightboxCnt.AppendChild(throbber);

            var closeAction = new HTMLDivElement {
                TextContent = Magics.FontAwesomeClose};
            closeAction.AddClasses(
                Magics.CssClassUsesFontAwesome, Magics.CssClassLightBoxClose);
            lightboxCnt.AppendChild(closeAction);
            closeAction.OnClick += ev => parentForModal.RemoveChild(glass);

            var openInNewTab = new HTMLAnchorElement {
                TextContent = Magics.FontAwesomeArrowsAlt,
                Href = generatedImageUrl,
                Target = "_blank"
            };
            openInNewTab.AddClasses(
                Magics.CssClassUsesFontAwesome, Magics.CssClassLightBoxOpenInNewTab);
            lightboxCnt.AppendChild(openInNewTab);

            glass.OnClick += ev => {
                if (ev.HtmlTarget() != glass) {
                    return;
                }

                parentForModal.RemoveChild(glass);
            };

            var img = new HTMLImageElement {
                Width = 1, 
                Height = 1};
            lightboxCnt.AppendChild(img);
            
            img.OnLoad += ev => {
                img.Width = dim.Item1;
                img.Height = dim.Item2;
                lightboxCnt.RemoveClasses(Magics.CssClassLightBoxLoading);
                lightboxCnt.AddClasses(Magics.CssClassLightBoxLoaded);
            };
            
            //populated after OnLoad setup as otherwise it may be not invoked...
            img.Src = generatedImageUrl;

            glass.AppendChild(lightboxCnt);
            parentForModal.AppendChild(glass);
        }
    }
}
