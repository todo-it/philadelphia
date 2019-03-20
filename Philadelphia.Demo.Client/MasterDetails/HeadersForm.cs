using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class HeadersForm : IForm<HTMLElement,HeadersForm,HeadersForm.Outcome>  {
        public enum Outcome {
            Canceled,
            ChoosenHeader
        }

        public event Action<HeadersForm, Outcome> Ended;
        public string Title => "Headers...";
        public IFormView<HTMLElement> View => _view;
        private readonly HeadersFormView _view;
        public DataGridModel<HeaderDto> Headers {get;}
        public HeaderDto ChoosenHeader {get; private set;}

        public HeadersForm() {
            _view = new HeadersFormView();

            Func<string,BaseUnboundColumnBuilder<HeaderDto>> build = x => UnboundDataGridColumnBuilder.For<HeaderDto>(x);
            Headers = DataGridModel<HeaderDto>.CreateAndBindNonReloadable(
                _view.Items,
                (el,theaderHeight,_) => //most of the time you would use Toolkit.DefaultTableBodyHeightProvider()
                    el.GetAvailableHeightForFormElement(0, 2) - theaderHeight - _view.Help.Widget.OffsetHeight,
                new List<IDataGridColumn<HeaderDto>> { 
                    build("#")
                        .WithValueLocalized(x => Headers.Items.IndexOf(x)+1)
                        .Build(),
                    build("Id")
                        .WithValueLocalized(x => x.Id)
                        .TransformableDefault()
                        .Build(),
                    build("Name")
                        .WithValue(x => x.Name)
                        .TransformableDefault()
                        .Build()
                }).Item1;
            Headers.Selected.Changed += (_, __, ___) => {
                if (Headers.Selected.Length != 1) {
                    return;
                }
                ChoosenHeader = Headers.Selected.First();
                Ended?.Invoke(this, Outcome.ChoosenHeader);
            };
            Headers.Activated.Changed += (sender, oldValue, newValue, errors, isUserChange) => {
                if (newValue == null) {
                    return;
                }
                ChoosenHeader = Headers.Activated.Value;
                Ended?.Invoke(this, Outcome.ChoosenHeader);
            };
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Outcome.Canceled));
    }
}
