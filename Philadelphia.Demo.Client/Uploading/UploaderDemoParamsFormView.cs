using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class UploaderDemoParamsFormView : IFormView<HTMLElement> {
        private readonly HorizontalTabbedView _tabs;

        public InputTypeButtonActionView CreateRowBased {get;} 
            = new InputTypeButtonActionView("Create");
        public InputTypeButtonActionView CreateGridBased {get;}
            = new InputTypeButtonActionView("Create");
        public InputView GridColumnsCount {get; } 
            = new InputView("Grid columns count");
        public InputView MaxFileNameLength {get; } 
            = new InputView("Max filename length");
        public SingleChoiceDropDown<OpenImagesMethod> ItemOpeningMethod {get; } 
            = new SingleChoiceDropDown<OpenImagesMethod>(
                "Item opening method", 
                x => x.ToString(), 
                UnboundDataGridColumnBuilder
                    .For<OpenImagesMethod>("Name")
                    .WithValueAsText(x => x+"", x => x+"")
                    .Build());
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[0];

        public UploaderDemoParamsFormView() {
            ItemOpeningMethod.FilterElement.InputWidget.Style.MinWidth = "200px";
            ItemOpeningMethod.PermittedValues = EnumExtensions.GetEnumValues<OpenImagesMethod>();

            _tabs = HorizontalTabbedView.CreateGeneric(
                Tuple.Create<string,Action<HTMLElement>>(
                    "Thumbnail grid based", cnt => {
                        cnt.AppendChild(GridColumnsCount.Widget);
                        cnt.AppendChild(CreateGridBased.Widget);
                    }),
                Tuple.Create<string,Action<HTMLElement>>(
                    "Text row based", cnt => {
                        cnt.AppendChild(CreateRowBased.Widget);
                    }) );
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                MaxFileNameLength, ItemOpeningMethod,"<br>",_tabs};
        }
    }
}
