using System;
using System.Linq;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ActionModelExtensions {
        public static Action BindSelectionIntoEnabled<ActionT,ItemT>(
            this IActionModel<ActionT> model, DataGridModel<ItemT> datagrid, SelectionNeeded mode, RowFilter<ItemT> extraRowFilterOrNull = null) {
            void UpdateEnabled() {
                bool enabled;

                switch (mode) {
                    case SelectionNeeded.AtLeastOneSelected:
                        enabled = datagrid.Selected.Length >= 1;
                        break;

                    case SelectionNeeded.ExactlyOneSelected:
                        enabled = datagrid.Selected.Length == 1;
                        break;

                    default:
                        throw new Exception("unsupported selectionNeeded");
                }

                if (!enabled) {
                    switch (mode) {
                        case SelectionNeeded.AtLeastOneSelected:
                            model.ChangeEnabled(false, new[] {I18n.Translate("Need at least one row selected")}, true);
                            break;

                        case SelectionNeeded.ExactlyOneSelected:
                            model.ChangeEnabled(false, new[] {I18n.Translate("Need exactly one row selected")}, true);
                            break;

                        default:
                            throw new Exception("unsupported selectionNeeded");
                    }

                    return;
                }

                if (extraRowFilterOrNull == null) {
                    model.ChangeEnabled(true, new string[] { }, true);
                    return;
                }

                if (datagrid.Selected.Length != datagrid.Selected.Where(extraRowFilterOrNull.Filter).Count()) {
                    model.ChangeEnabled(false, new[] {extraRowFilterOrNull.UserFriendlyReason}, true);
                    return;
                }

                model.ChangeEnabled(true, new string[] { }, true);
            }

            datagrid.Selected.Changed += (_, __, ___) => UpdateEnabled();
            UpdateEnabled();
            return UpdateEnabled;
        }
    }
}
