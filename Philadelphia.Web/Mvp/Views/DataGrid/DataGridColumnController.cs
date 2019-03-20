using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class DataGridColumnController {
        private static void AssureUnique<T,U>(IEnumerable<T> items, Func<T,U> keyBuilder) {
            var known = new HashSet<U>();

            items.ForEach(x => {
                var i = keyBuilder(x);
                if (known.Contains(i)) {
                    throw new Exception(string.Format("unexpected duplicated value detected {0}", i));
                }
                known.Add(i);
            });
        }

        private static Tuple<HTMLElement,DataGridColumnControllerResult<OperT>> Create<OperT,ViewT>(
            ITransformationMediator listener,
            FilterDef<OperT>[] availableFilters,
            IEnumerable<AggregatorDef<OperT>> availableAggregators,
            IEnumerable<GrouperDef<OperT>> availableGroupers,
            Func<IReadWriteValue<OperT>,IReadWriteValueView<HTMLElement,ViewT>> paramEditor,
            OperT initialFilterValue,
            OperT invalidFilterValue,
            IComparer<OperT> sortingImpl) {

            return Create<OperT,OperT,ViewT>(
                x => x,
                listener,
                availableFilters,
                availableAggregators,
                availableGroupers,
                paramEditor,
                initialFilterValue,
                invalidFilterValue,
                sortingImpl );
        }

        //TODO refactor this monster
        private static Tuple<HTMLElement,DataGridColumnControllerResult<InternalT>> Create<InternalT,OperT,ViewT>(
                    Func<InternalT,OperT> toOper,
                    ITransformationMediator listener,
                    FilterDef<OperT>[] availableFilters,
                    IEnumerable<AggregatorDef<OperT>> rawAvailableAggregators,
                    IEnumerable<GrouperDef<OperT>> rawAvailableGroupers,
                    Func<IReadWriteValue<OperT>,IReadWriteValueView<HTMLElement,ViewT>> paramEditor,
                    OperT initialFilterValue,
                    OperT invalidFilterValue,
                    IComparer<OperT> sortingImpl) {
            
            var availableAggregators = rawAvailableAggregators.ToList();
            var availableGroupers = rawAvailableGroupers.ToList();

            AssureUnique(availableFilters, x => x.Label);
            AssureUnique(availableAggregators, x => x.Label);
            AssureUnique(availableGroupers, x => x.Label);

            LocalValue<AggregatorDef<OperT>> aggregateFunc = null;
            LocalValue<GrouperDef<OperT>> groupingFunc = null;
            LocalValue<GroupOrAggregate?> groupOrAggregateChoice = null;

            FilterDef<OperT> currentFilterImplOrNull = null;
            var filterLabelToImpl = new Dictionary<string,FilterDef<OperT>>();
            availableFilters.ForEach(x => filterLabelToImpl.Add(x.Label, x));
            
            AggregatorDef<OperT> currentAggrImplOrNull = null;
            var aggregLabelToImpl = new Dictionary<string,AggregatorDef<OperT>>();
            availableAggregators.ForEach(x => aggregLabelToImpl.Add(x.Label, x));
            
            GrouperDef<OperT> currentGrouperImplOrNull = null;
            var grouperLabelToImpl = new Dictionary<string,GrouperDef<OperT>>();
            availableGroupers.ForEach(x => grouperLabelToImpl.Add(x.Label, x));
            
            var filterParam = new LocalValue<OperT>(initialFilterValue, invalidFilterValue);

            HTMLElement controllerElem = new HTMLSpanElement {ClassName = Magics.CssClassFilterMainContainer};
            var actionContainer = new HTMLSpanElement {ClassName = Magics.CssClassFilterActionContainer};
            
            controllerElem.AppendChild(actionContainer);

            var groupOrAggregateChoiceView = new RadioBasedSingleChoice();

            Element GetLabelTh() {
                var filterTh = controllerElem.ParentElement;
                var filterTr = filterTh.ParentElement;
                var thead = filterTr.ParentElement;

                var iCol = filterTr.IndexOfChild(filterTh);

                var labelTr = thead.GetChildAtOrNull(0);
                var labelTh = labelTr.GetChildAtOrNull(iCol);
                return labelTh;
            }

            void MarkAsGrouped(bool activated) {
                Logger.Debug(typeof(DataGridColumnController), "markAsGrouped({0})", activated);
                controllerElem.ParentElement.AddOrRemoveClass(activated, Magics.CssClassActive);
                GetLabelTh().AddOrRemoveClass(activated, Magics.CssClassWithGrouping);
                controllerElem.ParentElement.AddOrRemoveClass(activated, Magics.CssClassWithGrouping);
            }

            void MarkAsAggregated(bool activated) {
                Logger.Debug(typeof(DataGridColumnController), "markAsAggregated({0})", activated);
                controllerElem.ParentElement.AddOrRemoveClass(activated, Magics.CssClassActive);
                GetLabelTh().AddOrRemoveClass(activated, Magics.CssClassWithAggregation);
                controllerElem.ParentElement.AddOrRemoveClass(activated, Magics.CssClassWithAggregation);
            }

            void MarkAsFiltered(bool activated) {
                Logger.Debug(typeof(DataGridColumnController), "markAsFiltered({0})", activated);
                controllerElem.ParentElement.AddOrRemoveClass(activated, Magics.CssClassActive);
                GetLabelTh().AddOrRemoveClass(activated, Magics.CssClassWithFilter);
                controllerElem.ParentElement.AddOrRemoveClass(activated, Magics.CssClassWithFilter);
            }

            var removeFilter = new HTMLAnchorElement {
                ClassName = Magics.CssClassFilterRemove,
                Target = "#",
                Title = I18n.Translate("Remove filter"),
                Text = Magics.FontAwesomeFilter };
            actionContainer.AppendChild(removeFilter);
                    
            var removeFilterActionView = new InputTypeButtonActionView(removeFilter);
            LocalActionBuilder.Build(removeFilterActionView, () => {
                currentFilterImplOrNull = null;
                MarkAsFiltered(false);
                listener.UserFilterChangedHandler(ChangeOrRemove.Removed);
            });

            var removeGrouping = new HTMLAnchorElement {
                ClassName = Magics.CssClassGroupingRemove,
                Target = "#",
                Title = I18n.Translate("Remove grouping"),
                Text = Magics.FontAwesomeListUl };
            actionContainer.AppendChild(removeGrouping);
                
            var removeGroupingActionView = new InputTypeButtonActionView(removeGrouping);
            LocalActionBuilder.Build(removeGroupingActionView, async () => {
                currentGrouperImplOrNull = null;
                MarkAsGrouped(false);
                await groupOrAggregateChoice.DoChange(null, false, null, false);
                listener.UserGroupingChangedHandler(ChangeOrRemove.Removed);
            });
                
            var removeAggregation = new HTMLAnchorElement {
                ClassName = Magics.CssClassAggregationRemove,
                Target = "#",
                Title = I18n.Translate("Remove aggregation"),
                Text = "Σ" };
            actionContainer.AppendChild(removeAggregation);
                
            var removeAggregationActionView = new InputTypeButtonActionView(removeAggregation);
            LocalActionBuilder.Build(removeAggregationActionView, async () => {
                currentAggrImplOrNull = null;
                await groupOrAggregateChoice.DoChange(null, false, null, false);
                MarkAsAggregated(false);
                listener.UserAggregationChangedHandler(ChangeOrRemove.Removed);
            });
            



            Action<string> groupingChangedHandler = async labelOrNull => {
                Logger.Debug(typeof(DataGridColumnController), "Setting grouping programmatically to {0}", labelOrNull);

                if (labelOrNull == null) {
                    currentGrouperImplOrNull = null;
                    MarkAsGrouped(false);
                    await groupOrAggregateChoice.DoChange(null, false, null, false);
                    return;
                }

                GrouperDef<OperT> grouper;

                if (!grouperLabelToImpl.TryGetValue(labelOrNull, out grouper)) {
                    Logger.Debug(typeof(DataGridColumnController), "No such grouping func when looking by label");
                    return;
                }

                await groupingFunc.DoChange(grouper, false, null, false);
                currentGrouperImplOrNull = grouper;
                MarkAsGrouped(true);
            };

            Action<string> aggregationChangedHandler = async labelOrNull => {
                Logger.Debug(typeof(DataGridColumnController), "Setting aggregation programmatically to {0}", labelOrNull);

                if (labelOrNull == null) {
                    currentAggrImplOrNull = null;
                    MarkAsAggregated(false);
                    await groupOrAggregateChoice.DoChange(null, false, null, false);
                    listener.UserAggregationChangedHandler(ChangeOrRemove.Removed);
                    return;
                }

                if (!aggregLabelToImpl.TryGetValue(labelOrNull, out var aggr)) {
                    Logger.Debug(typeof(DataGridColumnController), "No such aggregation func when looking by label");
                    return;
                }

                await aggregateFunc.DoChange(aggr, false, null, false);
                currentAggrImplOrNull= aggr;
                MarkAsAggregated(true);
            };
            
            listener.InitUserSide(groupingChangedHandler, aggregationChangedHandler);



            var filterOper = new LocalValue<FilterDef<OperT>>(availableFilters.FirstOrDefault());
            var filterOperView = new DropDownSelectBox("") {
                PermittedValues = filterLabelToImpl.Select(x => Tuple.Create(x.Key, x.Key))
            };
            filterOperView.BindReadWriteAndInitialize(filterOper, 
                x => x != null ? Tuple.Create(x.Label, x.Label) : Tuple.Create("", ""),
                x => !string.IsNullOrEmpty(x.Item1) ? filterLabelToImpl[x.Item1] : null);
            
            controllerElem.AppendChild(filterOperView.Widget);
            
            var val = paramEditor(filterParam);
            controllerElem.AppendChild(val.Widget);

            filterParam.Changed += (_, __, newValue, ___,isUserChange) => {
                if (!isUserChange) {
                    return;
                }
                currentFilterImplOrNull = filterOper.Value;
                MarkAsFiltered(true);
                listener.UserFilterChangedHandler(ChangeOrRemove.Changed);
            };

            filterOper.Changed += (_, __, newValue, ___, isUserChange) => {
                if (!isUserChange) {
                    return;
                }
                
                currentFilterImplOrNull = newValue;
                MarkAsFiltered(true);
                listener.UserFilterChangedHandler(ChangeOrRemove.Changed);
            };

            {
                groupOrAggregateChoice = new LocalValue<GroupOrAggregate?>(null);
                groupOrAggregateChoiceView.Widget.ClassList.Add(Magics.CssClassGroupOrAggregate);
                groupOrAggregateChoiceView.PermittedValues = 
                    EnumExtensions.GetEnumValues<GroupOrAggregate>().Select(x => 
                        Tuple.Create(((int)x).ToString(), x.GetUserFriendlyName()));
                
                groupOrAggregateChoiceView.BindReadWriteAndInitialize(groupOrAggregateChoice, 
                    x => !x.HasValue ? null : Tuple.Create(((int)x).ToString(), x.Value.GetUserFriendlyName()),
                    x => (x == null || string.IsNullOrEmpty(x.Item1)) ? 
                            null 
                        : 
                            (GroupOrAggregate?)Convert.ToInt32(x.Item1));
                
                groupingFunc = new LocalValue<GrouperDef<OperT>>(availableGroupers.FirstOrDefault());
                var groupingFuncView = new DropDownSelectBox(I18n.Translate("group by:"));

                groupingFuncView.Widget.ClassList.Add(Magics.CssClassGroupingFunc);
                groupingFuncView.PermittedValues = grouperLabelToImpl.Select(x => Tuple.Create(x.Key, x.Key));
                groupingFuncView.BindReadWriteAndInitialize(groupingFunc, 
                    x => x != null ? Tuple.Create(x.Label, x.Label) : Tuple.Create("", ""),
                    x => !string.IsNullOrEmpty(x.Item1) ? grouperLabelToImpl[x.Item1] : null);

                aggregateFunc = new LocalValue<AggregatorDef<OperT>>(availableAggregators.FirstOrDefault());
                var aggregateFuncView = new DropDownSelectBox(I18n.Translate("aggregate by:"));

                aggregateFuncView.Widget.ClassList.Add(Magics.CssClassAggregateFunc);
                aggregateFuncView.PermittedValues = aggregLabelToImpl.Select(x => Tuple.Create(x.Key, x.Key));
                aggregateFuncView.BindReadWriteAndInitialize(aggregateFunc, 
                    x => x != null ? Tuple.Create(x.Label, x.Label) : Tuple.Create("", ""),
                    x => !string.IsNullOrEmpty(x.Item1) ? aggregLabelToImpl[x.Item1] : null);
                
                groupingFunc.Changed += (_, __, newValue, errors, isUserChange) => {
                    if (!isUserChange) {
                        return;
                    }
                    currentGrouperImplOrNull = newValue;
                    MarkAsGrouped(newValue != null);
                    listener.UserGroupingChangedHandler(newValue != null ? ChangeOrRemove.Changed : ChangeOrRemove.Removed);
                };

                aggregateFunc.Changed += (_, __, newValue, errors, isUserChange) => {
                    if (!isUserChange) {
                        return;
                    }
                    currentAggrImplOrNull = newValue;
                    MarkAsAggregated(newValue != null);
                    listener.UserAggregationChangedHandler(newValue != null ? ChangeOrRemove.Changed : ChangeOrRemove.Removed);
                };

                groupOrAggregateChoice.Changed += (_, __, newValue, ___, isUserChange) => {
                    Logger.Debug(typeof(DataGridColumnController), "groupOrAggregateChoice changed to {0} by {1}", newValue, isUserChange );

                    //if (!isUserChange) {
                    //    return;
                    //}
                    
                    if (!newValue.HasValue) {
                        return;
                    }
                    
                    switch (newValue.Value) {
                        case GroupOrAggregate.Aggregate:
                            currentAggrImplOrNull = aggregateFunc.Value;
                            MarkAsAggregated(true);
                            listener.UserAggregationChangedHandler(ChangeOrRemove.Changed);
                            break;

                        case GroupOrAggregate.Group:
                            currentGrouperImplOrNull = groupingFunc.Value;
                            MarkAsGrouped(true);
                            listener.UserGroupingChangedHandler(ChangeOrRemove.Changed);
                            break;

                        default: throw new Exception("unsupported GroupOrAggregate");
                    }
                };

                controllerElem.AppendChild(groupOrAggregateChoiceView.Widget);
                controllerElem.AppendChild(groupingFuncView.Widget);
                controllerElem.AppendChild(aggregateFuncView.Widget);
            }
            
            DocumentUtil.AddMouseClickListener(controllerElem, ev => {
                if (!ev.HasHtmlTarget()) {
                    return;
                }

                //find out if clicked item is a descendant of th
                if (ev.HtmlTarget().IsDescendantOf(controllerElem.ParentElement)) {
                    controllerElem.ParentElement.ClassList.Add(Magics.CssClassActive);
                    return;
                }

                controllerElem.ParentElement?.ClassList.Remove(Magics.CssClassActive);
            });
                        
            return Tuple.Create(controllerElem, new DataGridColumnControllerResult<InternalT> {
                FilterImpl = x => 
                    currentFilterImplOrNull == null || currentFilterImplOrNull.FilterFunc(filterParam.Value, toOper(x)),
                AggregationImpl = x => currentAggrImplOrNull?.AggregatorFunc(x.Select(toOper)),
                GroupingImpl = x => currentGrouperImplOrNull?.GroupingFunc(x.Select(toOper)),
                SortingImpl = new CompareImpl<InternalT>((x,y) => sortingImpl.Compare(toOper(x),toOper(y))),
                IsGroupingActive = () => currentGrouperImplOrNull != null
            });
        }
        
        public static string GroupEverythingAsOneGroupLabel => I18n.Translate("one group");
        public static string SumAggregatorLabel => I18n.Translate("Sum");

        public static Tuple<HTMLElement,DataGridColumnControllerResult<string>> ForString(
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<string>[] additionalGrouperOrAggr) {
            
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;

            return Create(
                listener,
                new [] {
                    new FilterDef<string>(
                        I18n.Translate("contains"),
                        (filterParam, x) => (x ?? "").ContainsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("doesn't contain"),
                        (filterParam, x) => !(x ?? "").ContainsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("equals"),
                        (filterParam, x) => (x ?? "").EqualsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("doesn't equal"),
                        (filterParam, x) => !(x ?? "").EqualsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("begins with"), 
                        (filterParam, x) => (x ?? "").StartsWithCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("ends with"),    
                        (filterParam, x) => (x ?? "").EndsWithCaseInsensitive(filterParam ?? ""))
                },
                new List<AggregatorDef<string>> {
                    new AggregatorDef<string>(I18n.Translate("Count"), x => I18n.Localize(x.Count()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<string>> {
                    new GrouperDef<string>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<string>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y, y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, y => y, y => y);
                    return val;
                },
                default(string),
                default(string),
                new StringCompareBasedComparer()
            );
        }
        
        public static Tuple<HTMLElement,DataGridColumnControllerResult<int>> ForInt(
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<int>[] additionalGrouperOrAggr) {
            
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;
            var sumAggregatorLabel = SumAggregatorLabel;

            return Create(
                listener,
                new [] {
                    new FilterDef<int>(
                        I18n.Translate("equals"), 
                        (filterParam, x) => x == filterParam),
                    new FilterDef<int>(
                        I18n.Translate("doesn't equal"), 
                        (filterParam, x) => x != filterParam),
                    new FilterDef<int>(
                        I18n.Translate("is bigger than"), 
                        (filterParam, x) => x > filterParam),
                    new FilterDef<int>(
                        I18n.Translate("is smaller than"), 
                        (filterParam, x) => x < filterParam)
                },
                new List<AggregatorDef<int>> {
                    new AggregatorDef<int>(sumAggregatorLabel, x => I18n.Localize(x.Sum())), 
                    new AggregatorDef<int>(I18n.Translate("Count"), x => I18n.Localize(x.Count())), 
                    new AggregatorDef<int>(I18n.Translate("Average"), 
                        x => I18n.Localize((decimal)x.Average(), DecimalFormat.AsNumber)), 
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<int>> {
                    new GrouperDef<int>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<int>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y, y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, y => I18n.Localize(y), y => I18n.ParseInt(y));
                    return val;
                },
                default(int),
                default(int),
                Comparer<int>.Default
            );
        }
        
        public static Tuple<HTMLElement,DataGridColumnControllerResult<bool>> ForBool(
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<bool>[] additionalGrouperOrAggr) {

            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;
            
            return Create(
                listener,
                new [] {
                    new FilterDef<bool>(
                        I18n.Translate("is true"),
                        (_, x) => x),
                    new FilterDef<bool>(
                        I18n.Translate("is false"), 
                        (_, x) => !x)
                },
                new List<AggregatorDef<bool>> {
                    new AggregatorDef<bool>(I18n.Translate("Count"), x => I18n.Localize(x.Count())), 
                    new AggregatorDef<bool>(I18n.Translate("Count ☑"), x => I18n.Localize(x.Count(y => y))), 
                    new AggregatorDef<bool>(I18n.Translate("Count ☐"), x => I18n.Localize(x.Count(y => !y))),
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<bool>> {
                    new GrouperDef<bool>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<bool>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y, y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.Widget.Style.Visibility = Visibility.Hidden; //occupies space so filters stay aligned
                    return val;
                },
                default(bool),
                default(bool),
                Comparer<bool>.Default
            );
        }
        
        public static Tuple<HTMLElement,DataGridColumnControllerResult<decimal?>> ForNullableDecimal(
                    DecimalFormat format, 
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<decimal?>[] additionalGrouperOrAggr) {

            var EmptyLabel = I18n.Translate("(empty)");            
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;
            var sumAggregatorLabel = SumAggregatorLabel;

            return Create(
                listener,
                new [] {
                    new FilterDef<decimal?>(
                        I18n.Translate("equals"),
                        (filterParam, x) => x.HasValue && x == filterParam),
                    new FilterDef<decimal?>(
                        I18n.Translate("doesn't equal"), 
                        (filterParam, x) => x.HasValue && x != filterParam),
                    new FilterDef<decimal?>(
                        I18n.Translate("is bigger than"), 
                        (filterParam, x) => x.HasValue && x > filterParam),
                    new FilterDef<decimal?>(
                        I18n.Translate("is smaller than"), 
                        (filterParam, x) => x.HasValue && x < filterParam)
                },
                new List<AggregatorDef<decimal?>> {
                    new AggregatorDef<decimal?>(sumAggregatorLabel, x => 
                        I18n.Localize(x.Sum().GetValueOrDefault(), DecimalFormat.AsNumber)), 
                    new AggregatorDef<decimal?>(I18n.Translate("Count"), x => I18n.Localize(x.Count())), 
                    new AggregatorDef<decimal?>(I18n.Translate("Average"), 
                        x => I18n.Localize(x.Average().GetValueOrDefault(), DecimalFormat.AsNumber)), 
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<decimal?>> {
                    new GrouperDef<decimal?>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<decimal?>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, 
                            y => ObjectUtil.MapNullAs(y, (decimal z) => z.ToString(), EmptyLabel), 
                            y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, 
                        y => !y.HasValue ? "" : I18n.Localize(y.Value, format), 
                        y => string.IsNullOrEmpty(y) ? (decimal?)null : I18n.ParseDecimal(y));
                    return val;
                },
                default(decimal),
                default(decimal),
                Comparer<decimal?>.Default
            );
        }

        public static Tuple<HTMLElement,DataGridColumnControllerResult<decimal>> ForDecimal(
                    DecimalFormat format, 
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<decimal>[] additionalGrouperOrAggr) {
            
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;
            var sumAggregatorLabel = SumAggregatorLabel;

            return Create(
                listener,
                new [] {
                    new FilterDef<decimal>(
                        I18n.Translate("equals"),
                        (filterParam, x) => x == filterParam),
                    new FilterDef<decimal>(
                        I18n.Translate("doesn't equal"), 
                        (filterParam, x) => x != filterParam),
                    new FilterDef<decimal>(
                        I18n.Translate("is bigger than"), 
                        (filterParam, x) => x > filterParam),
                    new FilterDef<decimal>(
                        I18n.Translate("is smaller than"), 
                        (filterParam, x) => x < filterParam)
                },
                new List<AggregatorDef<decimal>> {
                    new AggregatorDef<decimal>(sumAggregatorLabel, x => 
                        I18n.Localize(x.Sum(), DecimalFormat.AsNumber)), 
                    new AggregatorDef<decimal>(I18n.Translate("Count"), x => I18n.Localize(x.Count())), 
                    new AggregatorDef<decimal>(I18n.Translate("Average"), 
                        x => I18n.Localize(x.Average(), DecimalFormat.AsNumber)), 
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<decimal>> {
                    new GrouperDef<decimal>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<decimal>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y, y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("", InputView.TypeText);
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, 
                        y => I18n.Localize(y, format), 
                        y => Convert.ToDecimal(y));
                    return val;
                },
                default(decimal),
                default(decimal),
                Comparer<decimal>.Default
            );
        }
        
        public static Tuple<HTMLElement,DataGridColumnControllerResult<DecimalWithPrecision>> ForDecimalWithPrecision(
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<DecimalWithPrecision>[] additionalGrouperOrAggr) {
            
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;
            var sumAggregatorLabel = SumAggregatorLabel;

            return Create(
                listener,
                new [] {
                    new FilterDef<DecimalWithPrecision>(
                        I18n.Translate("is empty"),
                        (filterParam, x) => DecimalWithPrecision.ComparatorImpl(x, null) == 0),
                    new FilterDef<DecimalWithPrecision>(
                        I18n.Translate("is not empty"),
                        (filterParam, x) => DecimalWithPrecision.ComparatorImpl(x, null) != 0),
                    new FilterDef<DecimalWithPrecision>(
                        I18n.Translate("equals"),
                        (filterParam, x) => DecimalWithPrecision.ComparatorImpl(x, filterParam) == 0),
                    new FilterDef<DecimalWithPrecision>(
                        I18n.Translate("doesn't equal"), 
                        (filterParam, x) => DecimalWithPrecision.ComparatorImpl(x, filterParam) != 0),
                    new FilterDef<DecimalWithPrecision>(
                        I18n.Translate("is bigger than"), 
                        (filterParam, x) => DecimalWithPrecision.ComparatorImpl(x, filterParam) > 0),
                    new FilterDef<DecimalWithPrecision>(
                        I18n.Translate("is smaller than"), 
                        (filterParam, x) => DecimalWithPrecision.ComparatorImpl(x, filterParam) < 0)
                },
                new List<AggregatorDef<DecimalWithPrecision>> {
                    new AggregatorDef<DecimalWithPrecision>(sumAggregatorLabel, x => {
                        var lst = x.Where(y => y != null).ToList();
                        var p = lst.Any() ? lst[0].Precision : 0;
                        return I18n.Localize(
                            lst.Aggregate(0m, (y,z) => y + z.RoundedValue), 
                            DecimalFormatExtensions.GetWithPrecision(p));
                    }), 
                    new AggregatorDef<DecimalWithPrecision>(I18n.Translate("Count"), 
                        x => I18n.Localize(x.Count())), 
                    new AggregatorDef<DecimalWithPrecision>(I18n.Translate("Average"), x => {
                        var lst = x.Where(y => y != null).ToList();
                        var p = lst.Any() ? lst[0].Precision : 0;
                        return I18n.Localize(
                            lst.Aggregate(0m, (y,z) => y + z.RoundedValue) / lst.Count <= 0 ? 1 : lst.Count, 
                            DecimalFormatExtensions.GetWithPrecision(p));
                    }), 
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<DecimalWithPrecision>> {
                    new GrouperDef<DecimalWithPrecision>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<DecimalWithPrecision>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y, y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("", InputView.TypeText);
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, 
                        y => y == null ? "" : I18n.Localize(
                            y.Value, DecimalFormatExtensions.GetWithPrecision(y.Precision)), 
                        y => {
                            if (string.IsNullOrWhiteSpace(y)) {
                                return null;
                            }
                            var v = I18n.ParseDecimal(y);
                            var s = I18n.Localize(v, DecimalFormat.WithFiveDecPlaces);
                            var p = s.Length - s.IndexOf('.') - 1;
                            return new DecimalWithPrecision(v, p);
                        });
                    return val;
                },
                null,
                null,
                new DecimalWithPrecisionDefaultComparer()
            );
        }

        public static Tuple<HTMLElement,DataGridColumnControllerResult<DateTime>> ForDateTime(
                    DateTimeFormat format,
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<DateTime>[] additionalGrouperOrAggr) {
            
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;
           
            return Create(
                listener,
                new [] {
                    new FilterDef<DateTime>(
                        I18n.Translate("equals"),
                        (filterParam, x) => x == filterParam),
                    new FilterDef<DateTime>(
                        I18n.Translate("doesn't equal"), 
                        (filterParam, x) => x != filterParam),
                    new FilterDef<DateTime>(
                        I18n.Translate("is after"), 
                        (filterParam, x) => x > filterParam),
                    new FilterDef<DateTime>(
                        I18n.Translate("is before"), 
                        (filterParam, x) => x < filterParam)
                },
                new List<AggregatorDef<DateTime>> {
                    new AggregatorDef<DateTime>(I18n.Translate("Count"), x => I18n.Localize(x.Count()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<DateTime>> {
                    new GrouperDef<DateTime>(groupEverythingAsOneGroupLabel,
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<DateTime>(I18n.Translate("year"),
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y.Year, y => y.KeyData.ToString())),
                    new GrouperDef<DateTime>(I18n.Translate("month"),
                        x => RecordGroupingUtil.GroupRecordsByKey(x, 
                            y => I18n.Localize(y, DateTimeFormat.YM), 
                            y => y.KeyData.ToString())),
                    new GrouperDef<DateTime>(I18n.Translate("day"),
                        x => RecordGroupingUtil.GroupRecordsByKey(x, 
                            y => I18n.Localize(y, DateTimeFormat.DateOnly), 
                            y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, 
                        y => I18n.Localize(y, format), 
                        y => y == null ? DateTimeExtensions.SmallestDate() : Convert.ToDateTime(y));
                    return val;
                },
                DateTimeExtensions.SmallestDate(),
                DateTimeExtensions.SmallestDate(),
                Comparer<DateTime>.Default
            );
        }
        
        public static Tuple<HTMLElement,DataGridColumnControllerResult<DateTime?>> ForNullableDateTime(
                    DateTimeFormat format,
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<DateTime?>[] additionalGrouperOrAggr) {
            
            var EmptyLabel = I18n.Translate("(empty)");
            var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;

            return Create(
                listener,
                new [] {
                    new FilterDef<DateTime?>(
                        I18n.Translate("equals"),
                        (filterParam, x) => x.HasValue && x == filterParam),
                    new FilterDef<DateTime?>(
                        I18n.Translate("doesn't equal"), 
                        (filterParam, x) => x.HasValue && x != filterParam),
                    new FilterDef<DateTime?>(
                        I18n.Translate("is after"), 
                        (filterParam, x) => x.HasValue && x > filterParam),
                    new FilterDef<DateTime?>(
                        I18n.Translate("is before"), 
                        (filterParam, x) => x.HasValue && x < filterParam)
                },
                new List<AggregatorDef<DateTime?>> {
                    new AggregatorDef<DateTime?>(I18n.Translate("Count"), x => I18n.Localize(x.Count()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<DateTime?>> {
                    new GrouperDef<DateTime?>(groupEverythingAsOneGroupLabel,
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<DateTime?>(I18n.Translate("year"),
                        x => RecordGroupingUtil.GroupRecordsByKey(x, 
                            y => ObjectUtil.MapNullAs(y, z => z.Year.ToString(), EmptyLabel),
                            y => y.KeyData.ToString())),
                    new GrouperDef<DateTime?>(I18n.Translate("month"),
                        x => RecordGroupingUtil.GroupRecordsByKey(x, 
                        y => ObjectUtil.MapNullAs(y, 
                            z => I18n.Localize(z, DateTimeFormat.YM), EmptyLabel),
                        y => y.KeyData.ToString())),
                    new GrouperDef<DateTime?>(I18n.Translate("day"),
                        x => RecordGroupingUtil.GroupRecordsByKey(x, 
                        y => ObjectUtil.MapNullAs(y, 
                            z => I18n.Localize(z, DateTimeFormat.DateOnly), EmptyLabel),
                        y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, 
                        y => !y.HasValue ? "" : I18n.Localize(y.Value, format), 
                        y => string.IsNullOrEmpty(y) ? (DateTime?)null : Convert.ToDateTime(y));
                    return val;
                },
                DateTimeExtensions.SmallestDate(),
                DateTimeExtensions.SmallestDate(),
                Comparer<DateTime?>.Default
            );
        }

        public static Tuple<HTMLElement, DataGridColumnControllerResult<ModelT>> ForTypeTreatedAsString<ModelT>(
                    Func<ModelT, string> textValueProvider, 
                    ITransformationMediator listener,
                    params GrouperDefOrAggregatorDef<string>[] additionalGrouperOrAggr) {

             var groupEverythingAsOneGroupLabel = GroupEverythingAsOneGroupLabel;

             return Create(
                textValueProvider,
                listener,
                new [] {
                    new FilterDef<string>(
                        I18n.Translate("contains"),
                        (filterParam, x) => (x ?? "").ContainsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("doesn't contain"),
                        (filterParam, x) => !(x ?? "").ContainsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("equals"),
                        (filterParam, x) => (x ?? "").EqualsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("doesn't equal"),
                        (filterParam, x) => !(x ?? "").EqualsCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("begins with"), 
                        (filterParam, x) => (x ?? "").StartsWithCaseInsensitive(filterParam ?? "")),
                    new FilterDef<string>(
                        I18n.Translate("ends with"),    
                        (filterParam, x) => (x ?? "").EndsWithCaseInsensitive(filterParam ?? ""))
                },
                new List<AggregatorDef<string>> {
                    new AggregatorDef<string>(I18n.Translate("Count"), x => I18n.Localize(x.Count()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Aggregator != null).Select(x => x.Aggregator)),
                new List<GrouperDef<string>> {
                    new GrouperDef<string>(groupEverythingAsOneGroupLabel, 
                        RecordGroupingUtil.GroupAllRecordsAsOneGroup),
                    new GrouperDef<string>(I18n.Translate("unique value"), 
                        x => RecordGroupingUtil.GroupRecordsByKey(x, y => y, y => y.KeyData.ToString()))
                }.Concat(additionalGrouperOrAggr.Where(x => x.Grouper != null).Select(x => x.Grouper)),
                x => {
                    var val = new InputView("");
                    val.PlaceHolder = I18n.Translate("Filter value");
                    val.BindReadWriteAndInitialize(x, y => y, y => y);
                    return val;
                },
                default(string),
                default(string),
                new StringCompareBasedComparer()
            );
        }
    }
}
