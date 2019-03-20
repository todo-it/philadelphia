using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// stock HTML's SELECT element functionality: single selection, fixed values list, without suggest/autocomplete
    /// NOTE: fixed values SHOULD have unique keys BUT it is not a responisbility of SelectBox to enforce it
    /// </summary>
    public class DropDownSelectBox : IRestrictedSingleReadWriteValueView<HTMLElement,Tuple<string, string>> {
        private readonly HTMLLabelElement _label;
        private readonly HTMLSelectElement _input;
        private readonly HTMLElement _container, _inputContainer;
        private readonly ControlWithValueLogic<Tuple<string,string>> _logic;
        private Func<DropDownSelectBox,string> _textVersionProvider;
        private readonly HTMLSpanElement _textVersion;
        
        public event ValueChangedSimple<Tuple<string,string>> Changed;
        public event UiErrorsUpdated ErrorsChanged;
        
        public HTMLSelectElement InputWidget => _input;
        public HTMLElement InputContainerWidget => _inputContainer;

        //not really disabling field as it looses focus...
        //FIXME: it should delegate to _logic
        public bool IsValidating {
            get => _input.ClassList.Contains(Magics.CssClassIsValidating);
            set => _input.AddOrRemoveClass(value, Magics.CssClassIsValidating);
        }

        //FIXME: it should delegate to _logic
        public bool Enabled { 
            get => !_input.ClassList.Contains(Magics.CssClassDisabled);
            set { 
                _input.AddOrRemoveClass(!value, Magics.CssClassDisabled);
                _input.ForEachChildElement(x => ((HTMLOptionElement)x).Disable = !value);
            } 
        }
        
        public Tuple<string,string> Value { 
            get => _logic.Value;
            set => _logic.Value = value;
        }

        public string LabelOfValue => _input.SelectedIndex >= 0 ? _input.Options[_input.SelectedIndex].Text : "";

        public ISet<string> DisabledReasons { 
            set => DefaultInputLogic.SetDisabledReasons(_input, value);
        }

        public ISet<string> Errors => DefaultInputLogic.GetErrors(_input);

        public void SetErrors(ISet<string> errors, bool isUserInput) { 
            DefaultInputLogic.SetErrors(_container, _input, isUserInput, errors);
            ErrorsChanged?.Invoke(this, errors);
        }
        
        public IEnumerable<Tuple<string, string>> PermittedValues {
            set {
                var isValid = false;
                var oldVal = _logic.Value?.Item1;

                _logic.PreserveValueDuringOperation(
                    () => { 
                        _input.RemoveAllChildren();

                        value.ForEach(x => {
                            var item = (HTMLOptionElement)Document.CreateElement("option");
                            if (x.Item1 == oldVal) {
                                isValid = true;
                                Logger.Debug(GetType(), "{0} == {1}", x.Item1, oldVal);
                            }
                            item.Value = x.Item1;
                            item.Text = x.Item2;
                            _input.AppendChild(item);
                        });    
                    });

                if (!isValid) {
                    Logger.Debug(GetType(), "unabled to restore preserved value {0}", oldVal);
                    _logic.Value = null;
                    Changed?.Invoke(_logic.Value, false);
                }
            }
        }

        public HTMLElement Widget => _container;
        private bool IsClickToEdit => _textVersionProvider != null;

        public DropDownSelectBox(string label) {
            _label = new HTMLLabelElement {
                TextContent = label
            };
            var id = UniqueIdGenerator.GenerateAsString();
            _label.SetAttribute("for", id);

            _textVersion = new HTMLSpanElement();
            _textVersion.ClassList.Add(Magics.CssClassTextVersion);
            
            _inputContainer = new HTMLDivElement();
            _inputContainer.ClassName = Magics.CssClassInputContainer;

            _input = new HTMLSelectElement();
            _input.SetAttribute("id", id);
			
            _container = new HTMLSpanElement();
            _container.ClassName = GetType().FullName;
            _container.AppendChild(_label);

            _inputContainer.AppendChild(_textVersion);
            _inputContainer.AppendChild(_input);
            
            _container.AppendChild(_inputContainer);
            
            _input.OnChange += ev => {
                _logic.PhysicalChanged(false, ev.IsUserGenerated());
                if (IsClickToEdit) {
                    _textVersion.TextContent = _textVersionProvider(this);    
                }
            };
            
            _input.OnBlur += x => {
                if (!IsClickToEdit) {
                    return;
                }
                
                _container.ClassList.Remove(Magics.CssClassEditing);
                _container.ClassList.Add(Magics.CssClassViewing);
            };
            
            _label.OnClick += _ => {
                if (!IsClickToEdit) {
                    return;
                }

                _textVersion.Click();
            };
            _textVersion.OnClick += ev => {
                _container.ClassList.Remove(Magics.CssClassViewing);
                _container.ClassList.Add(Magics.CssClassEditing);

                _input.Focus();
            };

            _logic = new ControlWithValueLogic<Tuple<string,string>>( 
                (newVal,isUser) => Changed?.Invoke(newVal, isUser),
                () => Tuple.Create(_input.Value, LabelOfValue),
                v => {
                    var newVal = v?.Item1;
                    Logger.Debug(GetType(), "changing value to {0} being {1}", v, newVal);
                    _input.Value = newVal;
                    if (IsClickToEdit) {
                        _textVersion.TextContent = _textVersionProvider(this);    
                    }
                },
                () => Enabled,
                v => Enabled = v,
                () => IsValidating,
                v => IsValidating = v
            );
        }

        public void EnableClickToEdit() {
            EnableClickToEdit(v => v.LabelOfValue);
        }

        public void EnableClickToEdit(Func<DropDownSelectBox,string> textVersionProvider) {
            _textVersionProvider = textVersionProvider;
            
            _container.ClassList.Add(Magics.CssClassClickToEditable);
            _container.ClassList.Add(Magics.CssClassViewing);
        }
        
        public static implicit operator RenderElem<HTMLElement>(DropDownSelectBox inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
