using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class MultipleChoiceDropDown<T> : IRestrictedMultipleReadWriteValueView<HTMLElement,IEnumerable<T>> {
        //
        // It internally uses anchor as tooltip container. Lack of mouseenter&mouseleave make it very difficult to put 
        // tooltips on container
        //
        private readonly Func<T, string> _itemUserFriendlyString;
        private readonly HTMLDivElement _container;
        private readonly HTMLDivElement _valueContainer,_txtVersionContainer;
        private readonly HTMLAnchorElement _txtVersion;
        private readonly HtmlTableBasedTableView _grid;
        private readonly DataGridModel<T> _gridModel;
        private bool _popupShown;
        private int _dontRaiseOnChanged;

        public event UiErrorsUpdated ErrorsChanged;
        public HTMLElement Widget => _container;
        public event ValueChangedSimple<IEnumerable<T>> Changed;

        public IEnumerable<T> PermittedValues {
            set {
                var missingSelectedItems = 
                    _gridModel.Selected.Count(x => !value.Contains(x));
                var willChangeValue = missingSelectedItems > 0;

                if (willChangeValue) {
                    _dontRaiseOnChanged++;
                }
                
                Logger.Debug(GetType(), 
                    "in PermittedValues setter: _dontRaiseOnChanged={0} willChangeValue={1} missingSelectedItems={2}", 
                    _dontRaiseOnChanged, willChangeValue, missingSelectedItems);

                _gridModel.Items.Replace(value);
            }
        }

        public IEnumerable<T> Value {
            get => _gridModel.Selected.ToArray();
            set {
                _dontRaiseOnChanged++;
                Logger.Debug(GetType(), "in Value setter: _dontRaiseOnChanged={0}", 
                    _dontRaiseOnChanged);

                _gridModel.Selected.Replace(value ?? new T[0]); //null is presented as empty list 
                //(similar idea: putting null into text input causes empty string)
            }
        }
        
        public bool IsValidating { 
            get => _container.ClassList.Contains(Magics.CssClassIsValidating);
            set => _container.AddOrRemoveClass(value, Magics.CssClassIsValidating);
        }
        public ISet<string> DisabledReasons { set => DefaultInputLogic.SetDisabledReasons(_txtVersion, value); }
        public bool Enabled { 
            protected get { return !_container.HasAttribute(Magics.AttrDataReadOnly); }
            set {
                if (value) {
                    _container.RemoveAttribute(Magics.AttrDataReadOnly);
                    return;
                }
                _container.SetAttribute(Magics.AttrDataReadOnly, "yes");
            } 
        }
        
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_txtVersion);
        public void SetErrors(ISet<string> errors, bool isUserInput) { 
            DefaultInputLogic.SetErrors(_container, _txtVersion, isUserInput, errors);
            ErrorsChanged?.Invoke(this, errors);
        }
        public HTMLElement TextVersionView => _txtVersion;
        
        public int VisibleItemsCount { get; set; } = Magics.DefaultDropDownVisibleItems;

        public MultipleChoiceDropDown(
            string label, Func<T,string> itemUserFriendlyString, 
            params IDataGridColumn<T>[] columns) {
            
            _itemUserFriendlyString = itemUserFriendlyString;
            
            var id = UniqueIdGenerator.GenerateAsString();

            _container = new HTMLDivElement();
            _container.ClassName = GetType().FullNameWithoutGenerics();

            _valueContainer = new HTMLDivElement {ClassName = Magics.CssClassValueContainer};
            _txtVersionContainer = new HTMLDivElement { Id = id, ClassName = Magics.CssClassValue};
            _valueContainer.AppendChild(_txtVersionContainer);

            var lbl = new HTMLLabelElement {
                TextContent = label,
                HtmlFor = id
            };

            _txtVersion = new HTMLAnchorElement();
            _txtVersionContainer.AppendChild(_txtVersion);
            _txtVersionContainer.AppendChild(new HTMLSpanElement {ClassName = Magics.CssClassIcon});
            
            _grid = new HtmlTableBasedTableView {
                Sortable = false,
                Filterable = false,
                Groupable = false,
                Resizable = false,
                SingleClickMeaning = SingleRowSelectionMode.ToggleRow };
            
            _container.AppendChild(lbl);
            _container.AppendChild(_valueContainer);
            
            _gridModel = DataGridModel<T>.CreateAndBindNonReloadable(
                _grid, 
                (_, __, rowHeight) => (rowHeight ?? Magics.DefaultDataGridRowHeight)*VisibleItemsCount, 
                columns).Item1;
            
            _gridModel.Selected.Changed += (insertedAt, inserted, removed) => {
                UpdateTextValue();

                Logger.Debug(GetType(), "_gridModel.Selected.Changed: _dontRaiseOnChanged={0}", 
                    _dontRaiseOnChanged);

                if (_dontRaiseOnChanged > 0) {
                    _dontRaiseOnChanged--;
                    return;    
                }
                
                Changed?.Invoke(_gridModel.Selected.ToArray(), true);
            };

            _txtVersionContainer.OnClick += ev => {
                if (Enabled) {
                    ShowPopupIfPossible();
                }
            };
            
            DocumentUtil.AddMouseDownListener(_grid.Widget, x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var htmlTarget = x.HtmlTarget();
                
                if (htmlTarget.IsElementOrItsDescendant(_grid.Widget)) {
                    //clicked inside control (focus stays within logical control) thus do nothing
                    return;
                }
                
                HidePopupIfPossible();
            });

            UpdateTextValue();
        }

        private void UpdateTextValue() {
            if (!_gridModel.Selected.Any()) {
                _txtVersion.TextContent = I18n.Translate("Choose...");
            } else {
                _txtVersion.TextContent = string.Join(
                    ",",
                    _gridModel.Selected.Select(x => _itemUserFriendlyString(x)));
            }
        }

        public void HidePopupIfPossible() {
            if (!_popupShown) {
                return;
            }

            _valueContainer.RemoveChild(_grid.Widget);
            _popupShown = false;
        }

        public void ShowPopupIfPossible() {
            if (_popupShown) {
                return;
            }

            _valueContainer.AppendChild(_grid.Widget);
            _popupShown = true;
        }

        public static implicit operator RenderElem<HTMLElement>(MultipleChoiceDropDown<T> inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
