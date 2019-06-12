using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class MenuItemUserModel {
        public IActionModel<Unit> Action {get; }
        public string DescriptionOrNull;
        public IReadOnlyValue<string> Label;
        public bool IsLeaf => Action != null && Items == null;
        public bool IsSubTree => !IsLeaf;
        public bool IsExit { get; set;}

        public IEnumerable<MenuItemUserModel> Items {get; }
        
        private MenuItemUserModel(
                IReadOnlyValue<string> label, IActionModel<Unit> actionOrNull, 
                IEnumerable<MenuItemUserModel> subItemsOrNull, bool isExitItem, string descriptionOrNull) {

            Action = actionOrNull;
            DescriptionOrNull = descriptionOrNull;
            Label = label;
            Items = subItemsOrNull;
            IsExit = isExitItem;
        }
        
        public static MenuItemUserModel CreateLogoutLeaf(
            string label, string descriptionOrNull = null) {

            return new MenuItemUserModel(
                new LocalValue<string>(label), new LocalActionModel(LambdaUtil.DoNothingAction), 
                null, true, descriptionOrNull);
        }

        public static MenuItemUserModel CreateLogoutLeaf(
                IReadOnlyValue<string> label, string descriptionOrNull = null) {

            return new MenuItemUserModel(
                label, new LocalActionModel(LambdaUtil.DoNothingAction), null, true, descriptionOrNull);
        }

        public static MenuItemUserModel CreateLocalLeaf(
                string label, Action command, string descriptionOrNull = null) {

            return new MenuItemUserModel(
                new LocalValue<string>(label), new LocalActionModel(command), null, false, descriptionOrNull);
        }

        public static MenuItemUserModel CreateLocalLeaf(
                IReadOnlyValue<string> label, Action command, string descriptionOrNull = null) {

            return new MenuItemUserModel(label, new LocalActionModel(command), null, false, descriptionOrNull);
        }

        public static MenuItemUserModel CreateRemoteLeaf(
                IReadOnlyValue<string> label, Func<Task<Unit>> command, bool isExitItem = false, 
                string descriptionOrNull = null) {

            return new MenuItemUserModel(
                label, new RemoteActionModel<Unit>(false, command), null, isExitItem, descriptionOrNull);
        }

        public static MenuItemUserModel CreateSubTree(
                IReadOnlyValue<string> label, IEnumerable<MenuItemUserModel> subItems) {

            return new MenuItemUserModel(label, null, subItems, false, null);
        }
        
        public static MenuItemUserModel CreateSubTree(string label, params MenuItemUserModel[] subItems) {

            return new MenuItemUserModel(new LocalValue<string>(label), null, subItems, false, null);
        }

        public static MenuItemUserModel CreateSubTree(
                IReadOnlyValue<string> label, params MenuItemUserModel[] subItems) {

            return new MenuItemUserModel(label, null, subItems, false, null);
        }
    }
}
