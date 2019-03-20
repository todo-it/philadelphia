using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class FlexibleLayoutContentForm : IForm<HTMLElement,FlexibleLayoutContentForm,FlexibleLayoutContentForm.Outcome> {
        public enum ModeType {
            None,
            Search,
            Settings,
            Chart,
            Table
        }
        public enum Outcome {
            LeftPanelLayoutChanged
        }
        public event Action<FlexibleLayoutContentForm, Outcome> Ended;
        public string Title { get; } = "Content";
        public IFormView<HTMLElement> View => _view;
        public LayoutModeType CurrentLayoutMode => _layoutMode.Value;

        private readonly FlexibleLayoutContentFormView _view;
        private LocalValue<LayoutModeType> _layoutMode;

        public FlexibleLayoutContentForm() {
            _view = new FlexibleLayoutContentFormView();
            _layoutMode = LocalValueFieldBuilder.BuildEnumBasedChoice(
                LayoutModeType.TitleExtra_Actions_Body, _view.LeftPanelLayout);

            _layoutMode.Changed += (sender, oldValue, newValue, errors, isUserChange) => {
                if (!isUserChange) {
                    return;
                }
                Ended?.Invoke(this, Outcome.LeftPanelLayoutChanged);
            };
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }
}
