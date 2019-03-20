using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridProgram : IFlow<HTMLElement> {
        private readonly DataboundDatagridForm _datagrid;
        private readonly RemoteActionsCallerForm _fetchData;
        private readonly DataboundDatagridItemEditorForm _itemEditor;
        private readonly DataboundDatagridItemCreatorForm _itemCreator;

        public DataboundDatagridProgram(ISomeService someService) {
            _datagrid = new DataboundDatagridForm();
            _fetchData = new RemoteActionsCallerForm(o => o.Add(someService.FetchItems, x => _datagrid.Items.Items.Replace(x)));
            _itemCreator = new DataboundDatagridItemCreatorForm(someService);
            _itemEditor = new DataboundDatagridItemEditorForm(someService);
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.ReplaceMaster(_datagrid);
            renderer.AddPopup(_fetchData);

            _fetchData.Ended += (x, outcome) => {
                renderer.Remove(x);

                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        break;
                    case RemoteActionsCallerForm.Outcome.Interrupted:
                    case RemoteActionsCallerForm.Outcome.Canceled:
                        renderer.Remove(_datagrid);
                        break;
                        
                    default: throw new Exception("outcome not supported");
                }
            };

            _datagrid.Ended += async (x, outcome) => {
                switch (outcome) {
                    case DataboundDatagridForm.Outcome.Canceled:
                        renderer.Remove(x);
                        atExit();
                        break;

                    case DataboundDatagridForm.Outcome.ReloadData:
                        renderer.AddPopup(_fetchData);
                        break;

                    case DataboundDatagridForm.Outcome.CreateItemDemanded:
                        renderer.AddPopup(_itemCreator);
                        break;

                    case DataboundDatagridForm.Outcome.EditItemDemanded:
                        await _itemEditor.InitializeFrom(_datagrid.Items.Selected[0]);
                        renderer.AddPopup(_itemEditor);
                        break;

                    default: throw new Exception("outcome not supported");
                }
            };
            
            _itemCreator.Ended += (x, outcome) => {
                renderer.Remove(x);

                switch (outcome) {
                    case DataboundDatagridItemCreatorForm.Outcome.Canceled:
                        break;

                    case DataboundDatagridItemCreatorForm.Outcome.Created:
                        _datagrid.Items.Items.InsertAt(0, _itemCreator.CreatedItem);
                        break;
                        
                    default: throw new Exception("outcome not supported");
                }
            };

            _itemEditor.Ended += (x, _) => {
                renderer.Remove(x);
            };
        }
    }
}
