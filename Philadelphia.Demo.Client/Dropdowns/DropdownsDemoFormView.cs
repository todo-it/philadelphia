using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DropdownsDemoFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions { get; } = new IView<HTMLElement>[0];
        
        public LabeledReadOnlyView SingleChoiceState = new LabeledReadOnlyView("Sometext of choosen item");
        public SingleChoiceDropDown<SomeDto> SingleChoice {get; } = CreateSomeDtoSingleChoice();

        public MultipleChoiceDropDown<SomeDto> MultiChoice {get; } = CreateSomeDtoMultiChoice();
        public LabeledReadOnlyView MultiChoiceState = new LabeledReadOnlyView("How many selected?");
        
        private static MultipleChoiceDropDown<SomeDto> CreateSomeDtoMultiChoice() {
            Func<string,BaseUnboundColumnBuilder<SomeDto>> bld = x => UnboundDataGridColumnBuilder.For<SomeDto>(x);

            return new MultipleChoiceDropDown<SomeDto>(
                "Multichoice dropdown with columns",
                x => x.SomeText,
                bld("SomeText").WithValue(x => x.SomeText).Build(),
                bld("SomeNumber").WithValueLocalized(x => x.SomeNumber).Build());
        }
        
        private static SingleChoiceDropDown<SomeDto> CreateSomeDtoSingleChoice() {
            Func<string,BaseUnboundColumnBuilder<SomeDto>> bldr = x => UnboundDataGridColumnBuilder.For<SomeDto>(x);
            
            return new SingleChoiceDropDown<SomeDto>("Singlechoice dropdown with columns", x => x?.SomeText ?? "",
                bldr("SomeText").WithValue(x => x.SomeText).Build(),
                bldr("SomeNumber").WithValueLocalized(x => x.SomeNumber).Build()
            );
        }
        
        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                $"<div class='{Magics.CssClassTableLike}'>",
                SingleChoice,
                SingleChoiceState,
                MultiChoice,
                MultiChoiceState,
                "</div>"
            };
        }
    }
}
