using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridForm : IForm<HTMLElement,DataboundDatagridForm,DataboundDatagridForm.Outcome> {
        private readonly DataboundDatagridFormView _view;
        public enum Outcome {
            ReloadData,
            Canceled,
            EditItemDemanded,
            CreateItemDemanded
        }
        public string Title => "Databound datagrid";
        public DataGridModel<SomeDto> Items { get; }
        public event Action<DataboundDatagridForm,Outcome> Ended;
        public IFormView<HTMLElement> View => _view;
        public SomeDto ChoosenItem { get; private set; }

        public DataboundDatagridForm() {
            _view = new DataboundDatagridFormView();
            Func<string,BaseUnboundColumnBuilder<SomeDto>> build = x => UnboundDataGridColumnBuilder.For<SomeDto>(x);
            
            Items = DataGridModel<SomeDto>.CreateAndBindReloadable(
                _view.Items,
                () => Ended?.Invoke(this, Outcome.ReloadData),
                (el,theaderHeight,_) => //most of the time you would use Toolkit.DefaultTableBodyHeightProvider()
                    el.GetAvailableHeightForFormElement(0, 2) - theaderHeight - _view.Help.Widget.OffsetHeight,
                new List<IDataGridColumn<SomeDto>> { 
                    build("#")
                        .WithValueLocalized(x => Items.Items.IndexOf(x)+1)
                        .NonTransformable()
                        .Build(),
                    build("SomeNumber")
                        .WithValueLocalized(x => x.SomeNumber)
                        .TransformableDefault()
                        .Observes(x => nameof(x.SomeNumber))
                        .Build(),
                    build("SomeText")
                        .WithValue(x => x.SomeText)
                        .TransformableDefault()
                        .Observes(x => nameof(x.SomeText))
                        .Build(),
                    build("SomeBool")
                        .WithValueLocalized(x => x.SomeBool)
                        .TransformableDefault()
                        .Observes(x => nameof(x.SomeBool))
                        .Build(),
                    build("SomeTrait")
                        .WithValueAsText(x => x.SomeTrait, x => x.ToString())
                        .TransformableAsText()
                        .Observes(x => nameof(x.SomeTrait))
                        .Build(),
                }).model;
            
            Items.Activated.Changed += (sender, oldValue, newValue, errors, isUserChange) => {
                if (newValue == null) {
                    return;
                }
                ChoosenItem = Items.Activated.Value;
                Ended?.Invoke(this, Outcome.EditItemDemanded);
            };

            LocalActionBuilder.Build(_view.Creator, () => Ended?.Invoke(this, Outcome.CreateItemDemanded));
            
            //button that is activated only if exactly one record is selected in the datagarid
            var activateEditor = LocalActionBuilder.Build(_view.Editor, () => {
                ChoosenItem = Items.Selected[0];
                Ended?.Invoke(this, Outcome.EditItemDemanded);
            });
            activateEditor.BindSelectionIntoEnabled(Items, SelectionNeeded.ExactlyOneSelected);
        }

        public ExternalEventsHandlers ExternalEventsHandlers =>
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Outcome.Canceled));
    }
}
