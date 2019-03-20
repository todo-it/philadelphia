using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DropdownsDemoForm : IForm<HTMLElement,DropdownsDemoForm,Unit> {
        public event Action<DropdownsDemoForm, Unit> Ended;
        public string Title => "Dropdowns demo";
        private DropdownsDemoFormView _view;
        public IFormView<HTMLElement> View => _view;
        
        private static readonly IEnumerable<SomeDto> KnownSomeDtos = new List<SomeDto> {
            new SomeDto { Id = 1, SomeText = "Another option", SomeNumber = 15 },
            new SomeDto { Id = 2, SomeText = "Some option", SomeNumber = 52 },
            new SomeDto { Id = 3, SomeText = "Choose this", SomeNumber = 155 },
            new SomeDto { Id = 4, SomeText = "Or choose that", SomeNumber = 124},
            new SomeDto { Id = 5, SomeText = "Or maybe this", SomeNumber = 633 },
            new SomeDto { Id = 6, SomeText = "Pick me", SomeNumber = 364 },
            new SomeDto { Id = 7, SomeText = "No, pick me", SomeNumber = 633 },
            new SomeDto { Id = 8, SomeText = "Take this", SomeNumber = 562 },
            new SomeDto { Id = 9, SomeText = "Take that", SomeNumber = 243 },
            new SomeDto { Id = 10, SomeText = "Take me", SomeNumber = 754 },
            new SomeDto { Id = 11, SomeText = "Option A", SomeNumber = 532 },
            new SomeDto { Id = 12, SomeText = "Option B", SomeNumber = 743 },
            new SomeDto { Id = 13, SomeText = "Option 1", SomeNumber = 185 },
            new SomeDto { Id = 14, SomeText = "Option 2", SomeNumber = 106 },
            new SomeDto { Id = 15, SomeText = "Option 3", SomeNumber = 864 },
            new SomeDto { Id = 16, SomeText = "Option 4", SomeNumber = 143 },
            new SomeDto { Id = 17, SomeText = "Option 5", SomeNumber = 198 } };

        public DropdownsDemoForm() {
            _view = new DropdownsDemoFormView();

            _view.MultiChoice.PermittedValues = KnownSomeDtos;
            var choosenMultipleCount = LocalValueFieldBuilder.BuildInt(_view.MultiChoiceState);
            var choosenMultiple = LocalValueFieldBuilder.Build(_view.MultiChoice);
            choosenMultiple.Changed += async (_, __, newValue, ___, ____) => {
                await choosenMultipleCount.DoChange(newValue.Count(), false);
            };

            _view.SingleChoice.PermittedValues = KnownSomeDtos;
            var choosenSingleSomeText = LocalValueFieldBuilder.Build(_view.SingleChoiceState);
            var choosenSingle = LocalValueFieldBuilder.Build(_view.SingleChoice);
            choosenSingle.Changed += async (_, __, newValue, ___, ____) => {
                await choosenSingleSomeText.DoChange(newValue?.SomeText ?? "(not choosen)", false);
            };
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Unit.Instance));
    }
}
