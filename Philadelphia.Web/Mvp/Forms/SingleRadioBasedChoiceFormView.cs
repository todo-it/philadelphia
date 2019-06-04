using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Web {
    /// <summary>
    /// T must be identified by int
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleRadioBasedChoiceFormView<T> : IFormView<HTMLElement> {
        private readonly Func<T, string> _getLabel;
        private readonly Func<T, int> _itemToInt;
        
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(Confirm);
        public RadioBasedSingleChoice Choice { get; }
        public LabellessReadOnlyView Description { get; }
            = new LabellessReadOnlyView();
        public InputTypeButtonActionView Confirm { get; }
            = new InputTypeButtonActionView("OK").With(x => x.MarkAsFormsDefaultButton());

        public SingleRadioBasedChoiceFormView(
                T defaultValue, Func<T,string> getLabel, 
                Func<int,T> intToItem, 
                Func<T,int> itemToInt, 
                Action<SingleRadioBasedChoiceFormView<T>> postInitialization = null) {

            _getLabel = getLabel;
            _itemToInt = itemToInt;

            Choice = RadioBasedSingleChoiceUtilExtensions.Build<T,HTMLElement>(
                defaultValue, getLabel, intToItem, itemToInt);
            postInitialization?.Invoke(this);
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            parentContainer.ClassName = GetType().FullNameWithoutGenerics();
            return new RenderElem<HTMLElement>[] {Description, Choice};
        }
    }
}
