using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SelectionColumnBuilder {
        public static IDataGridColumn<RecordT> Build<RecordT>(
            Func<DataGridModel<RecordT>> model,
            params Tuple<string,Func<RecordT,bool>>[] customSelection) where RecordT : new() {

            var header = new LocalValue<string>(string.Format(I18n.Translate("{0} of {1}"), 0,0));
            
            var defaultActions = new List<HTMLElement> {
                new HTMLAnchorElement {
                    Href = "#",
                    TextContent = I18n.Translate("All")
                }.With(x => {
                    x.OnClick += ev => {
                        ev.PreventDefault();
                        model().Selected.Replace(model().Items);
                    };    
                }),
                new HTMLAnchorElement {
                    Href = "#",
                    TextContent = I18n.Translate("None")
                }.With(x => {
                    x.OnClick += ev => {
                        ev.PreventDefault();
                        model().Selected.Clear();
                    };
                })
            };
            
            return new DataGridColumn<RecordT,bool>(
                header, 
                TextType.TreatAsText,
                x => model().Selected.Contains(x),
                x => LocalizationUtil.BoolToUnicodeCheckbox(x),
                (x,exp) => exp.Export(x),
                _ => Tuple.Create<HTMLElement,DataGridColumnControllerResult<bool>>(null, 
                    new DataGridColumnControllerResult<bool> {
                        FilterImpl = null,
                        AggregationImpl = null,
                        GroupingImpl = null,
                        SortingImpl = null,
                        IsGroupingActive = () => false
                    }),
                new SelectedItemsListener<RecordT>(model),
                () => {
                    return new InputCheckboxView("")
                        .With(x => x.Widget.ClassList.Add(Magics.CssClassIsSelectionHandler) );
                }, 
                () => new List<Validate<bool>>(),
                (x,toBeSelected) => {
                    var isSelected = model().Selected.Contains(x);
                    if (!isSelected && toBeSelected) {
                        model().Selected.InsertAt(0, x);
                    } else if (isSelected && !toBeSelected) {
                        model().Selected.Delete(x);
                    }
                },
                null,
                null,
                true,
                () => {
                    var cntnr = new HTMLDivElement();
                    cntnr.Style.Display = Display.Flex;
                    cntnr.Style.FlexDirection = FlexDirection.Column;
                    
                    var customActions = customSelection.Select(x => 
                        new HTMLAnchorElement {
                            Href = "#",
                            TextContent = x.Item1
                        }.With(y => {
                            y.OnClick += ev => {
                                ev.PreventDefault();
                                model().Selected.Replace(model().Items.Where(z => x.Item2(z)));
                            };
                        })
                    );
                    
                    defaultActions
                        .Concat(customActions)
                        .ForEach(x => cntnr.AppendChild(x));

                    return cntnr;
                },
                dgmodel => {
                    var mdl = dgmodel ?? model();
                    if (mdl == null) {
                        throw new Exception("datagrid model is null");
                    }

                    // initialize
                    var selectedCount = mdl.Selected.Length;
                    var allCount = mdl.Items.Length;
                    header.DoChange(string.Format(I18n.Translate("{0} of {1}"), selectedCount,allCount), false);

                    mdl.Selected.Changed += (_, __, ___) => {
                        selectedCount = model().Selected.Length;
                        header.DoChange(string.Format(I18n.Translate("{0} of {1}"), selectedCount,allCount), false);
                    };

                    mdl.Items.Changed += (_, __, ___) => {
                        allCount = model().Items.Length;
                        header.DoChange(string.Format(I18n.Translate("{0} of {1}"), selectedCount,allCount), false);
                    };
                }
            );
        }
    }
}
