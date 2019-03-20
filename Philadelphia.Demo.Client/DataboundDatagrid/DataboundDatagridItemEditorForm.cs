using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridItemEditorForm : IForm<HTMLElement,DataboundDatagridItemEditorForm,Unit> {        
        public event Action<DataboundDatagridItemEditorForm, Unit> Ended;
        public string Title => "Item editor";
        public IFormView<HTMLElement> View => _view;

        private readonly DataboundDatagridItemEditorFormView _view;
        private int _someDtoId;
        private readonly ClassFieldRemoteMutator<int, int, SomeDto> _someNumber;
        private readonly ClassFieldRemoteMutator<string, string, SomeDto> _someText;
        private readonly ClassFieldRemoteMutator<bool, bool, SomeDto> _someBool;
        private readonly ClassFieldRemoteMutator<SomeTraitType?, SomeTraitType, SomeDto> _someTrait;

        public DataboundDatagridItemEditorForm(ISomeService someService) {
            _view = new DataboundDatagridItemEditorFormView();
            var fld = RemoteFieldBuilder<SomeDto>.For(someService.Modify, () => _someDtoId);
            
            _someText = fld.Build(x => x.SomeText, _view.SomeText, Validator.IsNotNullRef);
            _someNumber = fld.BuildInt(x => x.SomeNumber, _view.SomeNumber, Validator.MustBePositive);
            _someBool = fld.Build(x => x.SomeBool, _view.SomeBool);
            
            _view.SomeChoice.PermittedValues = EnumExtensions.GetEnumValues<SomeTraitType>().Select(x => (SomeTraitType?)x);
            _someTrait = fld.BuildSingleChoiceDropDown(
                x => x.SomeTrait, _view.SomeChoice, x => x.Value, x => x, Validator.IsNotNull);
        }
        
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Unit.Instance));

        public async Task InitializeFrom(SomeDto itemToEdit) {
            _someDtoId = itemToEdit.Id;

            await _someText.BindTo(itemToEdit); 
            await _someNumber.BindTo(itemToEdit); 
            await _someBool.BindTo(itemToEdit); 
            await _someTrait.BindTo(itemToEdit);
        }
    }
}
