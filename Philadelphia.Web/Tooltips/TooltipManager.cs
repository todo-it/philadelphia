using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// Tooltip should fade in and stay visible in TooltipMode.PermanentErrorsAndDisable
    /// Tooltip should fade in and fade out in TooltipMode.PeekIntoErrors
    /// 
    /// Implementation issue - one cannot use CSS transition or animation to animate 'display: none' into 'display: something else than none'. That's why there are 3 stages in tooltip life:
    /// disabled - 'display: none' where tooltip doesn't overlap with anything
    /// inactive - 'display: "empty" ' where tooltip is fading in our fading out
    /// active - 'display: "empty" '  where tooltip is fully visible
    /// 
    /// Implementation uses Window->setTimout() instead of transitionend. Why? because it seems some events are not fired even if transitioned elements are attached and have display != 'none'
    /// </summary>
    public class TooltipManager {
        private static readonly WeakDictionary<HTMLElement, Tuple<HTMLElement,TooltipMode>> _tooltips 
            = new WeakDictionary<HTMLElement, Tuple<HTMLElement,TooltipMode>>();
        private static readonly WeakDictionary<Element,List<TooltipOper>> _pendingTooltipOps 
            = new WeakDictionary<Element,List<TooltipOper>>();

        private TooltipOper Immediate(string classToRemove, string classToAdd) {
            Logger.Debug(GetType(), "scheduling Immediate oper");
            return new TooltipOper {
                OperationId = UniqueIdGenerator.Generate(),
                DurationMs = 1, //shortest possible time (but not in the same thread so that transitions are applied)
                ClassToRemove = classToRemove,
                ClassToAdd = classToAdd
            };
        }

        private TooltipOper Lasting(string classToRemove, string classToAdd, int milisec) {
            Logger.Debug(GetType(), "scheduling Lasting oper");
            return new TooltipOper {
                OperationId = UniqueIdGenerator.Generate(),
                DurationMs = milisec,
                ClassToRemove = classToRemove,
                ClassToAdd = classToAdd
            };
        }

        public TooltipManager() {
            //when visible element's ERROR tooltip is changed then -show or hide
            DocumentUtil.AddAttributeChangedListener(Magics.AttrDataErrorsTooltip, el => {
                var content = GetTooltipsOfElementOrNull(el);
            
                Logger.Debug(GetType(), "dom attr errors changed tooltip to content={0}", content);
                if (content == null) {
                    HideTooltipOn(el); //no errors anymore so can delete tooltip
                } else {
                    ShowTooltipOn(el, TooltipMode.PeekIntoErrors);
                }
            });

            //when visible element's DISABLED tooltip is changed then -show or hide
            DocumentUtil.AddAttributeChangedListener(Magics.AttrDataDisabledTooltip, el => {
                var content = GetTooltipsOfElementOrNull(el);
            
                Logger.Debug(GetType(), "dom attr errors changed tooltip to content={0}", content);
                if (content == null) {
                    HideTooltipOn(el); //no errors anymore so can delete tooltip
                } else {
                    ShowTooltipOn(el, TooltipMode.PeekIntoErrors);
                }
            });

            //when formerly detached element with ERROR tooltip is attached - show 
            DocumentUtil.AddGeneralElementAttachedToDocumentListener(el => {
                if (!HasTooltips(el)) {
                    return;
                }

                var content = GetTooltipsOfElementOrNull(el);
                Logger.Debug(GetType(), "dom attached changed tooltip to content={0}", content);

                if (content == null) {
                    HideTooltipOn(el); //no errors anymore so can delete tooltip
                } else {
                    ShowTooltipOn(el, TooltipMode.PeekIntoErrors, true);
                }
            });
            
            //mouse hover should show both DISABLED REASONS and ERRORS
            Document.AddEventListener("mouseover", ev => {
                if (!ev.HasHtmlTarget()) {
                    return;
                }

                var htmlTarget = ev.HtmlTarget();
                
                var elemAndContent = GetTooltipsOfElementOrAncestOrNull(htmlTarget);
                Logger.Debug(GetType(), "mouseover tooltip to-be-hidden?={0}", elemAndContent);

                if (elemAndContent != null) {
                    ShowTooltipOn(elemAndContent.Item1, TooltipMode.PermanentErrorsAndDisable);    
                }
            });
            Document.AddEventListener("mouseout", ev => {
                if (!ev.HasHtmlTarget()) {
                    return;
                }

                var htmlTarget = ev.HtmlTarget();
                
                var elemAndContent = GetTooltipsOfElementOrAncestOrNull(htmlTarget);
                Logger.Debug(GetType(), "mouseout tooltip hidding content={0}", elemAndContent);
                
                if (elemAndContent != null) {
                    HideTooltipOn(elemAndContent.Item1);
                }
            });
        }

        private void HideTooltipOn(HTMLElement tooltipOn) {
            Logger.Debug(GetType(), "HideTooltipOn on element id={0}", tooltipOn.Id);

            if (!_tooltips.ContainsKey(tooltipOn)) {
                Logger.Debug(GetType(), "tooltip hide ignored as there's no tooltip assigned");
                return;
            }

            var tt = _tooltips[tooltipOn].Item1;
            Logger.Debug(GetType(), "HideTooltipOn tt id={0}", tt.Id);

            var ops = new List<TooltipOper>();
            _pendingTooltipOps[tt] = ops;
            
            switch(GetTooltipState(tt)) {
                case TooltipState.Disabled: break; //already in the right state

                case TooltipState.EnabledShown:
                    ops.AddRange(
                        Lasting(Magics.CssClassActive, Magics.CssClassInactive, Magics.TooltipFadeOutMs),
                        Immediate(Magics.CssClassInactive, Magics.CssClassDisabled));
                    break;

                case TooltipState.EnabledHidden: //interrupted showning or hiding
                    ops.Add(Immediate(Magics.CssClassInactive, Magics.CssClassDisabled));
                    break;
            }
            
            ScheduleNextOperation(tt);
        }

        private void ShowTooltipOn(HTMLElement tooltipOn, TooltipMode mode, bool forceFullCycle = false) {
            var content = GetTooltipsOfElementOrNull(tooltipOn, mode);
            Logger.Debug(GetType(), "ShowTooltipOn starting forceFullCycle={0} mode={1}", forceFullCycle, mode);

            if (content == null) {
                Logger.Debug(GetType(), "tooltip show ignored as it would be empty");
                return;
            }

            var contAndTt = GetOrCreateTooltipOn(tooltipOn, content, mode);
            
            var cont = contAndTt.Item1;
            var tt = contAndTt.Item2;
            Logger.Debug(GetType(), "ShowTooltipOn gotOrCreated id={0}", tt.Id);
            
            //tooltipOn.ParentElement.Style.Position = Position.Relative;
            tooltipOn.Style.Position = Position.Relative;

            if (!tooltipOn.HasAttribute(Magics.AttrDataMayBeTooltipContainer)) {
                if (tooltipOn.ParentElement != null) {
                    tooltipOn.ParentElement.InsertAfter(cont, tooltipOn);
                }
            } else {
                tooltipOn.AppendChild(cont);
            }
                
            var ops = new List<TooltipOper>();
            _pendingTooltipOps[tt] = ops;
            _tooltips.Set(tt, Tuple.Create(tt, mode));
            
            if (forceFullCycle) {
                tt.ClassList.Remove(Magics.CssClassActive);
                tt.ClassList.Remove(Magics.CssClassInactive);
                tt.ClassList.Add(Magics.CssClassDisabled);
            }

            switch(GetTooltipState(tt)) {
                case TooltipState.EnabledShown: break; //already in the right state

                case TooltipState.Disabled:
                    ops.AddRange(
                        Immediate(Magics.CssClassDisabled, Magics.CssClassInactive),
                        Lasting(Magics.CssClassInactive, Magics.CssClassActive, Magics.TooltipFadeInMs));
                    break;

                case TooltipState.EnabledHidden: //interrupted showning or hiding
                    ops.Add(Lasting(Magics.CssClassInactive, Magics.CssClassActive, Magics.TooltipFadeInMs));
                    break;

                default: throw new ArgumentException("ShowTooltipOn unknown state");
            }

            //autohide?
            if (mode == TooltipMode.PeekIntoErrors) {
                ops.AddRange(
                    Lasting(Magics.CssClassActive, Magics.CssClassInactive, Magics.TooltipFadeOutMs),
                    Immediate(Magics.CssClassInactive, Magics.CssClassDisabled));
            }
            
            Logger.Debug(GetType(), "ShowTooltipOn scheduled {0} opers", ops.Count);

            ScheduleNextOperation(tt);
        }

        private void ScheduleNextOperation(HTMLElement tt) {
            var ops = _pendingTooltipOps[tt];

            if (!ops.Any()) {
                Logger.Debug(GetType(), "ScheduleNextOperation {0} ignored because it was empty", tt.Id);
                return;
            }

            var oper = ops[0];
            
            Logger.Debug(GetType(), "ScheduleNextOperation running operationId={0} content={1}->{2} for id={3} lasting {4}", 
                oper.OperationId, oper.ClassToRemove, oper.ClassToAdd, tt.Id, oper.DurationMs);

            tt.ClassList.Remove(oper.ClassToRemove);
            tt.ClassList.Add(oper.ClassToAdd);

            Window.SetTimeout(() => {
                OperationFinished(tt, oper.OperationId);
            }, oper.DurationMs);
        }
        
        private void OperationFinished(HTMLElement tt, int operationId) {
            var ops = _pendingTooltipOps[tt];

            Logger.Debug(GetType(), "OperationFinished finished operationId={0} for id={1}", 
                operationId, tt.Id);

            if (!ops.Any()) {
                Logger.Debug(GetType(), "OperationFinished ignored because there are no operations pending");
                return;
            }

            if (ops[0].OperationId != operationId) {
                Logger.Debug(GetType(), "OperationFinished ignored because expected operationId != next operation Id {0}!={1}",
                    ops[0].OperationId, operationId);
                return;
            }

            ops.RemoveAt(0);
            if (!ops.Any()) {
                Logger.Debug(GetType(), "OperationFinished no more operations to schedule");
                return;
            }

            ScheduleNextOperation(tt);
        }

        private TooltipState GetTooltipState(HTMLElement tt) {
            if (tt.ClassList.Contains(Magics.CssClassDisabled)) {
                return TooltipState.Disabled;
            }

            if (tt.ClassList.Contains(Magics.CssClassInactive)) {
                return TooltipState.EnabledHidden;
            }

            if (tt.ClassList.Contains(Magics.CssClassActive)) {
                return TooltipState.EnabledShown;
            }

            throw new ArgumentException("GetTooltipState unknown state");
        }

        /// <summary> returns tooltipcontainer and tooltip</summary>
        private Tuple<HTMLElement,HTMLElement> GetOrCreateTooltipOn(HTMLElement tooltipOn, string content, TooltipMode mode) {
            if (_tooltips.ContainsKey(tooltipOn)) {
                var oldTt = _tooltips.Get(tooltipOn).Item1;
                oldTt.TextContent = content;
                return Tuple.Create(oldTt.ParentElement, oldTt);
            }
            
            //tooltips need to be in container so that relative positioning works in both:
            //inline scenario: <label><input><tooltip> and in
            //'display:table' scenario those three have "display: table-cell"
            var ttCont = Document.CreateElement("span");
            ttCont.ClassName = Magics.CssClassTooltipContainer;
                
            var tt = Document.CreateElement("span");
            tt.Id = UniqueIdGenerator.GenerateAsString();
            tt.TextContent = content;
            tt.ClassName = Magics.CssClassTooltip;
            tt.ClassList.Add(Magics.CssClassDisabled);
            
            ttCont.AppendChild(tt);
    
            Logger.Debug(GetType(), "created tooltip to show it {0}", tt.Id);
            _tooltips[tooltipOn] = Tuple.Create(tt, mode);

            return Tuple.Create(ttCont,tt);
        }
        
        private bool HasTooltips(Element el) {
            return el.HasAttribute(Magics.AttrDataErrorsTooltip) || el.HasAttribute(Magics.AttrDataDisabledTooltip);
        }

        private Tuple<HTMLElement,string> GetTooltipsOfElementOrAncestOrNull(HTMLElement el, TooltipMode? mode = null) {
            var res = GetTooltipsOfElementOrNull(el, mode);

            if (res != null) {
                return Tuple.Create(el, res);
            }

            if (el != Document.Body) {
                return el.ParentElement == null ? null : GetTooltipsOfElementOrAncestOrNull(el.ParentElement, mode);
            }

            return null;
        }

        private string GetTooltipsOfElementOrNull(Element el, TooltipMode? mode = null) {
            var res = new List<string>();
            
            if (!mode.HasValue) {
                mode = TooltipMode.PermanentErrorsAndDisable;
            }

            var err = el.GetAttribute(Magics.AttrDataErrorsTooltip);
            var dis = el.GetAttribute(Magics.AttrDataDisabledTooltip);

            switch (mode) {
                case TooltipMode.PeekIntoErrors: 
                    res.AddIfTrue(!string.IsNullOrWhiteSpace(err), err);
                    break;

                case TooltipMode.PermanentErrorsAndDisable: 
                    res.AddIfTrue(!string.IsNullOrWhiteSpace(err), err);
                    res.AddIfTrue(!string.IsNullOrWhiteSpace(dis), dis);
                    break;

                default:
                    Logger.Error(GetType(), "tooltip mode unsupported");
                    throw new Exception("tooltip mode unsupported");
            }
            
            return !res.Any() ? null : string.Join("\n", res);            
        }
    }
}
