using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class FlexibleLayoutFlow : IFlow<HTMLElement> {
        private readonly MenuForm _menu;
        private readonly FlexibleLayoutContentForm _content;
        private readonly ActionsBarMenuFormView _menuView;
        private Action _onExit;

        public FlexibleLayoutFlow() {
            _menuView = new ActionsBarMenuFormView(x => {
                var res = InputTypeButtonActionView.CreateFontAwesomeIconedButtonLabelless(x.Label.Value);
                x.Label.Changed += (_, __, newValue, ___, ____) => res.PreLabelElem.TextContent = newValue;
                res.StaysPressed = true;
                return res;
            });
            _menuView.BodyPanel.Widget.Style.MinWidth = "332px";
            _menuView.BodyPanel.Value = "Activate some action button...";

            _menu = new MenuForm(_menuView, new []{
                MenuItemUserModel.CreateLocalLeaf(Magics.FontAwesomeSearch, 
                    () => ChangeModeTo(FlexibleLayoutContentForm.ModeType.Search)),
                MenuItemUserModel.CreateLocalLeaf(Magics.FontAwesomeGears, 
                    () => ChangeModeTo(FlexibleLayoutContentForm.ModeType.Settings)),
                MenuItemUserModel.CreateLocalLeaf(Magics.FontAwesomeBarChart, 
                    () => ChangeModeTo(FlexibleLayoutContentForm.ModeType.Chart)),
                MenuItemUserModel.CreateLocalLeaf(Magics.FontAwesomeTable, 
                    () => ChangeModeTo(FlexibleLayoutContentForm.ModeType.Table)),
                MenuItemUserModel.CreateLocalLeaf(Magics.FontAwesomeSignOut, () => {
                    _onExit?.Invoke();
                }) });
            _menu.Title = "Menu";
            _content = new FlexibleLayoutContentForm();
        }

        private void ChangeModeTo(FlexibleLayoutContentForm.ModeType newMode) {
            _menuView.BodyPanel.Value = "Selected menu item: "+newMode;
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            var panel = TwoPanelsWithResizerBuilder.BuildHorizontal(
                Hideability.First, true, renderer, SpacingPolicy.FirstWins);
            
            _onExit = () => {
                renderer.ClearMaster();
                atExit();
            };

            panel.FirstCanvas.LayoutMode = LayoutModeType.TitleExtra_Actions_Body;
            panel.First.ReplaceMaster(_menu);
            panel.SecondCanvas.LayoutMode = LayoutModeType.ExtraTitle_Body_Actions;
            panel.Second.ReplaceMaster(_content);
            renderer.ReplaceMasterWithAdapter(panel.Panel);

            _content.Ended += (x, outcome) => {
                switch (outcome) {
                    case FlexibleLayoutContentForm.Outcome.LeftPanelLayoutChanged:
                        panel.FirstCanvas.LayoutMode = x.CurrentLayoutMode;
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };
        }
    }
}
