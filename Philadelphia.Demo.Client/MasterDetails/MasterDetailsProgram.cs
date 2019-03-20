using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public enum LayoutChoice {
        Horizontal,
        Vertical
    }

    public class MasterDetailsProgram : IFlow<HTMLElement> {
        private readonly HeadersForm _headers;
        private readonly DetailsForm _details;
        private readonly IObservableCollection<HeaderDto> _headerItems 
            = new FilterableSortableObservableCollection<HeaderDto>();
        private readonly IObservableCollection<DetailDto> _detailItems 
            = new FilterableSortableObservableCollection<DetailDto>();
        private readonly RemoteActionsCallerForm _fetchData;
        private readonly EnumChoiceForm<LayoutChoice> _layoutChoice;

        public MasterDetailsProgram(ISomeService someService) {
            _fetchData = new RemoteActionsCallerForm(x => {
                x.Add(someService.FetchHeaders, y => _headerItems.Replace(y));
                x.Add(someService.FetchDetails, y => _detailItems.Replace(y));
            });

            _headers = new HeadersForm();
            _details = new DetailsForm();

            _headerItems.Changed += (_, __, ___) => {
                _headers.Headers.Items.Replace(_headerItems);
            };
            _layoutChoice = new EnumChoiceForm<LayoutChoice>(
                "Choose screen layout", true, LayoutChoice.Horizontal, x => x.ToString(), x => (LayoutChoice)x,
                x => x.Choice.Widget.ClassList.Add("horizontalOrVerticalChoice"));
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_layoutChoice);

            _layoutChoice.Ended += (x, outcome) => {
                renderer.Remove(x);
                switch (outcome) {
                    case EnumChoiceFormOutcome.Canceled:
                        atExit();
                        break;

                    case EnumChoiceFormOutcome.Choosen:
                        switch(x.ChoosenValue) {
                            case LayoutChoice.Horizontal: {
                                var panel = TwoPanelsWithResizerBuilder.BuildHorizontal(
                                    Hideability.None, false, renderer, SpacingPolicy.FirstWins);
                                panel.First.ReplaceMaster(_headers);
                                panel.Second.ReplaceMaster(_details);
                                renderer.ReplaceMasterWithAdapter(panel.Panel);
                                break;
                            }

                            case LayoutChoice.Vertical: {
                                var panel = TwoPanelsWithResizerBuilder.BuildVertical(
                                    Hideability.None, false, renderer, SpacingPolicy.Proportional);
                                panel.First.ReplaceMaster(_headers);
                                panel.Second.ReplaceMaster(_details);
                                renderer.ReplaceMasterWithAdapter(panel.Panel);
                                break;
                            }
                        }

                        renderer.AddPopup(_fetchData);
                        break;

                    default: throw new Exception("unsupported EnumChoiceFormOutcome");
                }
            };
            
            _fetchData.Ended += (x, outcome) => {
                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        renderer.Remove(x);
                        break;
                    case RemoteActionsCallerForm.Outcome.Canceled:
                    case RemoteActionsCallerForm.Outcome.Interrupted:
                        renderer.Remove(x);
                        renderer.ClearMaster();
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };

            _headers.Ended += (_, outcome) => {
                switch (outcome) {
                    case HeadersForm.Outcome.ChoosenHeader:
                        var headerId = _headers.ChoosenHeader.Id;
                        _details.Details.Items.Replace(_detailItems.Where(x => headerId == x.ParentId));
                        break;

                    case HeadersForm.Outcome.Canceled:
                        renderer.ClearMaster();
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };
        }
    } 
}
