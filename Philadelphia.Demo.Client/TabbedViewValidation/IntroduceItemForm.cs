using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class IntroduceItemForm : IForm<HTMLElement,IntroduceItemForm,IntroduceItemForm.Outcome> {
        public SomeDto CreatedItem => new SomeDto {
            SomeNumber = _someNumber.Value,
            SomeText = _someText.Value,
            SomeBool = _someBool.Value,
            SomeTrait = _someTrait.Value.Value
        };
        public enum Outcome {
            Saved,
            Canceled
        }
        public event Action<IntroduceItemForm,Outcome> Ended;
        public string Title => "Introduce item";
        public IFormView<HTMLElement> View => _view;
        
        private readonly IntroduceItemFormView _view;
        private readonly LocalValue<string> _someText;
        private readonly LocalValue<int> _someNumber;
        private readonly LocalValue<bool> _someBool;
        private readonly LocalValue<SomeTraitType?> _someTrait;

        public IntroduceItemForm() {
            _view = new IntroduceItemFormView();
            
            _someText = LocalValueFieldBuilder.Build(_view.SomeText, Validator.IsNotEmptyOrWhitespaceOnly);
            _someNumber = LocalValueFieldBuilder.BuildInt(_view.SomeNumber, Validator.MustBePositive);
            _someBool = LocalValueFieldBuilder.Build(_view.SomeBool);

            _view.SomeTrait.PermittedValues = EnumExtensions.GetEnumValues<SomeTraitType>().Select(x => (SomeTraitType?)x);
            _someTrait = LocalValueFieldBuilder.Build(_view.SomeTrait, Validator.IsNotNull);
            
            var isFormValid = new AggregatedErrorsValue<bool>(false, self => !self.Errors.Any(), x => {
                x.Observes(_someText); 
                x.Observes(_someNumber); 
                x.Observes(_someBool);
                x.Observes(_someTrait);
            });

            LocalActionBuilder
                .Build(_view.Create, () => Ended?.Invoke(this, Outcome.Saved) )
                .With(x => x.BindEnableAndInitialize(isFormValid));
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Outcome.Canceled));
    }
}
