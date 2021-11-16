using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// Implementation: Value is stored as DataGrid's selection. Selection change causes Change event to  be raised
    /// Internally there are several ways to update Value: 
    /// -set programmatically using Value property
    /// -cleared filter (causing null to be put into Value)
    /// -choose item with mouse
    /// -choose item with arrows+enter
    /// Complication factor: items filtering
    /// 
    /// Should be improved with possible refactor: 
    ///   use ActivatedRecord instead of Selected. In that case DataGrid should have option to advertise single click as row activation 
    ///   that would let us get rid of some there's-something-wrong-with-it booleans
    /// </summary>
    public class SingleChoiceDropDown<T> : IRestrictedSingleReadWriteValueView<HTMLElement,T> {
        private readonly Func<T, string> _itemUserFriendlyString;
        private readonly HtmlTableBasedTableView _view;
        private readonly DataGridModel<T> _model;
        private readonly DataGridModelPresenter<T> _presenter;
        private readonly InputView _filter;
        private T _value;
        private bool _popupShown, _valueIsBeingSet, _isEnabled = true, _arrowNavigation,_ignoreShowOnBlur;
        public Action<DataGridModel<T>,FilterRowsAction,string> FilterStrategy {get; set; } = 
            (mdl,fra,fltr) => mdl.ChangeGlobalFilterToBeginsWithOrAnyWordBeginsCaseInsensitive(fra, fltr);

        public IEnumerable<T> PermittedValues { set => _model.Items.Replace(value); }
        public HTMLElement Widget => _filter.Widget;

        public T Value {
            get => _value;
            set {
                Logger.Debug(GetType(), "requested value change to {0}", value);
                if (_valueIsBeingSet) {
                    return;
                }

                _value = value;
                _valueIsBeingSet = true;
                _filter.Value = _itemUserFriendlyString(Value);
                if (!_model.Selected.HasTheSameContentAs(_value)) {
                    _model.Selected.Replace(value == null? new T[] {} : new [] {_value});    
                }
                
                _valueIsBeingSet = false;
            }
        }

        public event UiErrorsUpdated ErrorsChanged;
        public event ValueChangedSimple<T> Changed;
        public bool IsValidating  { set => _filter.IsValidating = value; }
        public bool Enabled {
            set {
                _filter.Enabled = value;
                _isEnabled = value;
            }
        }
        public InputView FilterElement => _filter;

        public ISet<string> DisabledReasons { set => _filter.DisabledReasons = value; }
        public int VisibleItemsCount { get; set; } = Magics.DefaultDropDownVisibleItems;

        public SingleChoiceDropDown(string label, Func<T,string> itemUserFriendlyString, params IDataGridColumn<T>[] columns) {
            _itemUserFriendlyString = itemUserFriendlyString;
            _filter = new InputView(label) {
                Clearable = true, 
                SubmitOnEnter = false //as we handle ENTER as part of selection up/down arrows
            };
            _filter.Widget.ClassList.Add(GetType().FullNameWithoutGenerics());
            _filter.PlaceHolder = I18n.Translate("Click or type...");

            _view = new HtmlTableBasedTableView {
                Sortable = false,
                Filterable = false,
                Groupable = false,
                Resizable = false
            };

            var modelAndPresenter = DataGridModel<T>.CreateAndBindNonReloadable(
                _view,
                (_, __, rowHeight) => (rowHeight ?? Magics.DefaultDataGridRowHeight)*VisibleItemsCount, 
                columns);
            _model = modelAndPresenter.Item1;
            _presenter = modelAndPresenter.Item2;
            _filter.InputWidget.OnKeyDown += ev => {
                var current = _model.Selected.FirstOrDefault();
                var changed = false;

                switch (ev.KeyCode) {
                    case Magics.KeyCodeAlt:
                    case Magics.KeyCodeCtrl:
                    case Magics.KeyCodeShift:
                        Logger.Debug(GetType(), "modifier key doesn't modify input");
                        break;
                    
                    case Magics.KeyCodeArrowDown:
                        ShowPopupIfPossible();
                        _arrowNavigation = true;

                        if (current == null) {
                            if (_model.Items.Any()) {
                                _model.Selected.Replace(_model.Items[0]);
                            }
                        } else {
                            var idx = _model.Items.IndexOf(current);
                            if (idx+1 <= _model.Items.Length-1) {
                                _model.Selected.Replace(_model.Items[idx+1]);
                            }
                        }
                        changed = true;
                        _arrowNavigation = false;
                        break;

                    case Magics.KeyCodeArrowUp:
                        ShowPopupIfPossible();
                        _arrowNavigation = true;

                        if (current != null) {
                            var idx = _model.Items.IndexOf(current);
                            if (idx > 0) {
                                _model.Selected.Replace(_model.Items[idx-1]);
                            } else {
                                _model.Selected.Replace();
                            }
                        }
                        changed = true;
                        _arrowNavigation = false;
                        break;

                    case Magics.KeyCodeEnter:
                        if (current != null) {
                            ev.PreventDefault(); //otherwise event will be handled again by Input->OnChanged
                            if (_popupShown) {
                                ev.StopPropagation();
                            }
                            
                            Value = current;
                            HidePopupIfPossible();
                            Changed?.Invoke(Value, true);
                        }
                        break;
                    case Magics.KeyCodeEscape:
                        if (_popupShown) {
                            ev.PreventDefault();
                            ev.StopPropagation();
                            Logger.Debug(GetType(), "ESC consumed as popup hider");
                        }
                        HidePopupIfPossible();
                        break;

                    case Magics.KeyCodeTab:
                        _ignoreShowOnBlur = true;
                        break; //just loosing focus

                    default:
                        Logger.Debug(GetType(), "clearing selection");
                        _model.Selected.Replace();
                        break;
                }
                Logger.Debug(GetType(), "got key={0} and active item is now: {1}", ev.KeyCode, _model.Selected.FirstOrDefault());
                if (changed && _model.Selected.Any()) {
                    _presenter.ScrollToItem(_model.Selected.FirstOrDefault());
                }
            };

            _filter.Changed += (newValue, isUserInput) => {
                //clear sends empty string instead of null
                Logger.Debug(GetType(), "_filter->changed to {0} byUser?={1} _ignoreShowOnBlur={2}", newValue, isUserInput, _ignoreShowOnBlur);
                
                if (!isUserInput || _ignoreShowOnBlur) {
                    _ignoreShowOnBlur = false;
                    return;
                }

                FilterStrategy(
                    _model,
                    string.IsNullOrEmpty(newValue) ? FilterRowsAction.Remove : FilterRowsAction.Change, 
                    newValue ?? "");
                
                if (string.IsNullOrEmpty(newValue)) {
                    //most likely used 'clear' action
                    _model.Selected.Clear(); //event will be handled elsewhere
                    return;
                }

                if (!_valueIsBeingSet) {
                    Logger.Debug(GetType(), "show popup because filter was changed");
                    ShowPopupIfPossible(); //because after programmatic focus there is no popup    
                }
            };
            
            _view.Widget.ClassList.Add(Magics.CssClassPopup);

            _filter.Clicked += ev => {
                _ignoreShowOnBlur = false;

                if (!_isEnabled || _valueIsBeingSet) {
                    return;
                }
                Logger.Debug(GetType(), "show popup because filter was clicked");
                ShowPopupIfPossible();
            };

            _filter.Focused += ev => {
                _ignoreShowOnBlur = false;

                if (!ev.IsUserGenerated() || !_isEnabled || _valueIsBeingSet) {
                    return;
                }

                Logger.Debug(GetType(), "show popup because filter was focused");
                //popup should be shown only if focus was triggered by user (and not by auto-select-first-input-in-form logic)
                ShowPopupIfPossible();
            };
            
            DocumentUtil.AddMouseDownListener(_view.Widget, x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var htmlTarget = x.HtmlTarget();
                
                if (htmlTarget.IsElementOrItsDescendant(_filter.Widget)) {
                    //clicked inside control (focus stays within logical control) thus do nothing
                    return;
                }
                
                HidePopupIfPossible();
            });
            
            _model.Selected.Changed += (insertAt, inserted, removed) => {
                var isClear = _model.Selected.Length <= 0 && removed.Length > 0;
                var isSelect = _model.Selected.Length >= 1 && inserted.Length > 0;
                var isIrrelevant = removed.HasTheSameContentAs(inserted);

                Logger.Debug(GetType(), "dropdown selection changed: inserted={0} removed={1} isSelect={2} isClear={3} isIrrelevant={4} size={5} _dontSetValue={6} _arrowNavigation={7}", 
                    inserted, removed, isSelect, isClear, isIrrelevant, _model.Selected.Length, _valueIsBeingSet, _arrowNavigation);
                
                if (isIrrelevant) {
                    return;
                }

                if (!isClear && !isSelect || _valueIsBeingSet || _arrowNavigation) {
                    return;
                }

                //user choose value with mouse...
                Value = isSelect ? _model.Selected[0] : default(T);
                _filter.Value = _itemUserFriendlyString(Value);
            
                if (isSelect) {
                    HidePopupIfPossible();    
                }
            
                Changed?.Invoke(Value, true);
            };
        }
        
        private void HidePopupIfPossible() {
            Logger.Debug(GetType(), "HidePopupIfPossible id={0} _popupShown={1}", _filter.Widget.Id, _popupShown);
            if (!_popupShown) {
                return;
            }
            HidePopup();
        }

        private void ShowPopupIfPossible() {
            Logger.Debug(GetType(), "ShowPopupIfPossible id={0} _popupShown={1}", _filter.Widget.Id, _popupShown);
            if (_popupShown) {
                return;
            }
            ShowPopup();
        }

        private void HidePopup() {
            _filter.InputContainerWidget.RemoveChild(_view.Widget);
            _popupShown = false;
        }

        private void ShowPopup() {
            _filter.InputContainerWidget.InsertBefore(_view.Widget, _filter.InputWidget);
            _popupShown = true;
            if (_model.Selected.Any()) {
                _presenter.ScrollToItem(_model.Selected.FirstOrDefault());
            }
        }

        public ISet<string> Errors => _filter.Errors;

        public void SetErrors(ISet<string> errors, bool causedByUser) {
            _filter.SetErrors(errors, causedByUser);
            ErrorsChanged?.Invoke(this, errors);
        }

        public void EnableClickToEdit() {
            _filter.EnableClickToEdit(x => x);
        }
        
        public static implicit operator RenderElem<HTMLElement>(SingleChoiceDropDown<T> inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}