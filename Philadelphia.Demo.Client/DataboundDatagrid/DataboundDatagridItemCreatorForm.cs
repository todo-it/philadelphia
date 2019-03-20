using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridItemCreatorForm : IForm<HTMLElement,DataboundDatagridItemCreatorForm,DataboundDatagridItemCreatorForm.Outcome> {
        public enum Outcome {
            Canceled,
            Created
        }
        
        public event Action<DataboundDatagridItemCreatorForm, Outcome> Ended;
        public string Title => "Item creator";
        public IFormView<HTMLElement> View => _view;
        public SomeDto CreatedItem {get; private set;}

        private readonly DataboundDatagridItemCreatorFormView _view;
        
        public DataboundDatagridItemCreatorForm(ISomeService someService) {
            _view = new DataboundDatagridItemCreatorFormView();
            
            var someText = LocalValueFieldBuilder.Build(_view.SomeText, Validator.IsNotNullRef);
            var someNumber = LocalValueFieldBuilder.BuildNullableInt(_view.SomeNumber, Validator.IsNotNull, Validator.MustBePositive<int>());
            var someBool = LocalValueFieldBuilder.Build(_view.SomeBool);

            _view.SomeTrait.PermittedValues = EnumExtensions.GetEnumValues<SomeTraitType>().Select(x => (SomeTraitType?)x);
            var someTrait = LocalValueFieldBuilder.Build(_view.SomeTrait, Validator.IsNotNull);
            
            var createProduct = RemoteActionBuilder.Build(_view.CreateAction,
                () => someService.Create(
                    new SomeDto {
                        SomeNumber = someNumber.Value.GetValueOrDefault(),
                        SomeText = someText.Value,
                        SomeBool = someBool.Value,
                        SomeTrait = someTrait.Value.Value
                    }), 
                x => {
                    CreatedItem = x;
                    Ended?.Invoke(this, Outcome.Created);
                });

            var isFormValid = new AggregatedErrorsValue<bool>(false, self => !self.Errors.Any(), x => {
                x.Observes(someText); 
                x.Observes(someNumber); 
                x.Observes(someBool); 
                x.Observes(someTrait); 
            });
            createProduct.BindEnableAndInitialize(isFormValid);
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Outcome.Canceled));
    }
}
