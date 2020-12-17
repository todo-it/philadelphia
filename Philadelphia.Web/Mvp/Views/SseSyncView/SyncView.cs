using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SyncView : IView<HTMLElement> {
        private readonly InputTypeButtonActionView _toggleState;
        private readonly AnchorBasedActionView _showLogs;
        private readonly FramelessPopupProvider _logsPopup;
        private readonly HTMLDivElement _logItems = new HTMLDivElement();
        private readonly HTMLDivElement _container = new HTMLDivElement();

        public HTMLElement Widget => _container;
        public InputTypeButtonActionView ToggleState => _toggleState;
        public AnchorBasedActionView ShowLogsAction => _showLogs;
        public HTMLDivElement LogItems => _logItems;

        public SyncState State {
            set {
                switch (value) {
                    case SyncState.Offline:
                        _toggleState.ProperLabelElem.TextContent = I18n.Translate("Offline");
                        _toggleState.ProperLabelElem.Style.Color = "black";
                        break;

                    case SyncState.Connecting:
                        _toggleState.ProperLabelElem.TextContent = I18n.Translate("Online");
                        _toggleState.ProperLabelElem.Style.Color = "yellow";
                        break;

                    case SyncState.OnlineOk:
                        _toggleState.ProperLabelElem.TextContent = I18n.Translate("Online");
                        _toggleState.ProperLabelElem.Style.Color = "green";
                        break;

                    case SyncState.OnlineButError:
                        _toggleState.ProperLabelElem.TextContent = I18n.Translate("Online");
                        _toggleState.ProperLabelElem.Style.Color = "red";
                        break;
                }
            }
        }

        public SyncView(Action<HTMLElement> stylePopupHolder = null) {
            _toggleState = new InputTypeButtonActionView("");
            _showLogs = new AnchorBasedActionView("");
            _showLogs.Widget.Style.Padding = "5px";
            _logsPopup = new FramelessPopupProvider(_showLogs.Widget) {
                PopupRawContent = _logItems};

            _container.Style.Display = Display.Flex;
            _container.Style.AlignItems = AlignItems.Center;
            _container.AppendChild(_toggleState.Widget);
            _container.AppendChild(_logsPopup.Widget);

            _logItems.Style.Padding = "20px";
            _logItems.Style.MinHeight = "75px";
            _logItems.Style.MaxHeight = "75px";
            _logItems.Style.OverflowX = Overflow.Auto;
            _logItems.Style.OverflowY = Overflow.Scroll;
            _logItems.Style.WhiteSpace = WhiteSpace.Pre;
            
            stylePopupHolder?.Invoke(_logsPopup.PopupHolderElement);
            
            _toggleState.StaysPressed = true;
            
            _showLogs.Triggered += () => _logsPopup.ShowPopup();
        }
    }
}