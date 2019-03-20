using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DetailsForm : IForm<HTMLElement,DetailsForm,Unit> {
        public event Action<DetailsForm, Unit> Ended;
        public string Title => "...details";
        public IFormView<HTMLElement> View => _view;
        private readonly DetailsFormView _view;
        public DataGridModel<DetailDto> Details {get;}

        public DetailsForm() {
            _view = new DetailsFormView();

            Func<string,BaseUnboundColumnBuilder<DetailDto>> build = x => UnboundDataGridColumnBuilder.For<DetailDto>(x);
            Details = DataGridModel<DetailDto>.CreateAndBindNonReloadable(
                _view.Items,
                Toolkit.DefaultTableBodyHeightProvider(),
                new List<IDataGridColumn<DetailDto>> { 
                    build("#")
                        .WithValueLocalized(x => Details.Items.IndexOf(x)+1)
                        .Build(),
                    build("ParentId")
                        .WithValueLocalized(x => x.ParentId)
                        .TransformableDefault()
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
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }
}
