using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class RemoteValueChangeByChoiceFormView<T> : IFormView<HTMLElement> where T:new() {
        public InputTypeButtonActionView Confirm { get; } 
            = new InputTypeButtonActionView(I18n.Translate("Confirm"));
        public IView<HTMLElement>[] Actions  => ActionsBuilder.For(Confirm);
        public SingleChoiceDropDown<T> Choosen { get; }

        public RemoteValueChangeByChoiceFormView(
                string dropDownLabel, Func<T,string> userFriendlyNameBld, IDataGridColumn<T> customColOrNull=null) {
            
            Choosen = new SingleChoiceDropDown<T>(dropDownLabel, userFriendlyNameBld,
                customColOrNull ?? UnboundDataGridColumnBuilder.For<T>("").WithValue(userFriendlyNameBld).Build());
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {$"<div class='{Magics.CssClassTableLike}'>",Choosen,"</div>"};
        }
    }
}
