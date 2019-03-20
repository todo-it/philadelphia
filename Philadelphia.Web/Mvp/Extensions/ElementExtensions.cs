using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ElementExtensions {
        public static IView<HTMLElement> AsIview(this HTMLElement self) {
            return new AdaptAsIview(self);
        }

        /// <summary>input must be focused in order for createTextRange() to work ! </summary>
        /// <param name="self"></param>
        public static void SelectWholeTextAndMoveCursorToEnd(this Element self) {
            //original idea from https://css-tricks.com/snippets/javascript/move-cursor-to-end-of-input/
            
            if (self.HasFieldOrMethod("selectionStart") && self.IsFieldReadable("selectionStart")) {
                var selectionStart = self.GetFieldValue("selectionStart");

                if (Script.TypeOf(selectionStart) != "number") {
                    return;
                }

                var len = self.GetFieldValue("value.length");
                self.SetFieldValue("selectionStart", 0);
                self.SetFieldValue("selectionEnd", len);
                return;
            }

            var createTextRange = BridgeObjectUtil.GetFieldValue(self, "createTextRange");
            if (BridgeObjectUtil.HasFieldOrMethod(self,"select")) {
                BridgeObjectUtil.CallMethod(self, "select");    
            }
            
            if (Script.TypeOf(createTextRange) != "undefined") {
                var range = BridgeObjectUtil.CallSelf(createTextRange);
                BridgeObjectUtil.CallMethod(range, "collapse", false);
                BridgeObjectUtil.CallMethod(range, "select");
            }
        }

        public static HTMLElement GetChildAtOrNull(this HTMLElement self, int index) {
            var el = self.FirstElementChild;
            var i = 0;

            while (el != null) {
                if (i == index) {
                    return el;
                }

                el = el.NextElementSibling;
                i++;
            }

            return null;
        }

        //FIXME bridgedotnet get&sets int here
        public static double GetScrollTop(this Element self) {
            return Convert.ToDouble(self.GetFieldValue("scrollTop"));
        }

        //FIXME bridgedotnet get&sets int here
        public static void SetScrollTop(this Element self, double value) {
            self.SetFieldValue("scrollTop", value);
        }

        public static void ForEachChildElement(this Element self, Action<Element> action) {
            var el = self.FirstElementChild;

            while (el != null) {
                action(el);
                el = el.NextElementSibling;
            }
        }

        public static void RemoveAllCssClasses(this Element self) {
            while (self.ClassList.Length > 0) {
                self.ClassList.Remove(self.ClassList[0]);
            }
        }

        public static void AddClasses(this Element self, params string[] cssClassName) {
            cssClassName.ForEach(x => {
                if (!self.ClassList.Contains(x)) {
                    self.ClassList.Add(x);    
                }
            });
        }

        public static void RemoveClasses(this Element self, params string[] cssClassName) {
            cssClassName.ForEach(x => {
                if (self.ClassList.Contains(x)) {
                    self.ClassList.Remove(x);
                }    
            });
        }

        /// <summary>
        /// shortcut for if (enable) {elem.ClassList.Add(smth)} else {elem.ClassList.Remove(smth)}
        /// </summary>
        public static void AddOrRemoveClass(this Element self, bool shouldAdd, params string[] cssClassName) {
            if (shouldAdd) {
                self.AddClasses(cssClassName);
                return;
            } 

            self.RemoveClasses(cssClassName);
        }

        /// <summary>fluent style Element->SetAttribute</summary>
        public static T WithAttribute<T>(this T self, string attributeName, string attributeValue) where T : Element {
            self.SetAttribute(attributeName, attributeValue);
            return self;
        }

        /// <summary>change attributte value only if different otherwise DOM mutations are called (as overwrites are still reported) == performance loss </summary>
        public static void SetAttributeIfNeeded(this Element self, string attributeName, string attributeValue) {
            if (self.GetAttribute(attributeName) == attributeValue) {
                return;
            }

            self.SetAttribute(attributeName, attributeValue);
        }
        
        public static Element GetParentElementMatching(this Element container, Func<HTMLElement,bool> matches) {
            var el = container.ParentElement;

            while (el != null) {
                if (matches(el)) {
                    return el;
                }
                el = el.ParentElement;
            }

            return null;
        }

        public static Element GetParentElementHavingParent(this Element container, Element neededParent) {
            return container.GetParentElementMatching(x => x.ParentElement == neededParent);
        }

        public static Element GetParentElementOfTypeOrNull(this Element container, string tagName, Element butNoUpperThan = null) {
            if (butNoUpperThan != null) {
                //make sure there's such parent

                var parent = container.ParentElement;

                while (true) {
                    if (parent == butNoUpperThan) {
                        break; //has such parent: ok
                    }
                    if (parent == Document.Body) {
                        return null; // there's no such parent    
                    }
                    parent = parent.ParentElement;
                }
            }
            tagName = tagName.ToUpper();
            return container.GetParentElementMatching(x => x.TagName == tagName);
        }

        public static Element GetParentElementWithClassOrNull(this Element container, string className) {
            return container.GetParentElementMatching(x => x.ClassList.Contains(className));
        }

        public static void InsertAfter(this Element container, Element newElement, Element referenceElement) {
            container.InsertBefore(newElement, referenceElement.NextSibling);
        }

        public static void TryFocusElement(this HTMLElement el) {
            Logger.Debug(typeof(ElementExtensions), "focusing element el={0} having id={1}", el, el.Id);
            el.DispatchEvent(new Event("focus")); //el.Focus() doesn't set isTrusted to false
            if (!el.HasAttribute(Magics.AttrDataOptOutOfWholeTextSelectionOnFocus)) {
                el.SelectWholeTextAndMoveCursorToEnd();
            }
        }

        public static void FindAndFocusOnFirstItem(this HTMLElement self) {
            self.TraverseUntilFirst(el => {
                if (el.TagName != "INPUT" && el.TagName != "TEXTAREA" && el.TagName != "SELECT") {
                    return false;
                }
                
                el.TryFocusElement();
                
                return true;
            });
        }

        public static bool HasParentWithAttribute(this HTMLElement self, string attribName, HTMLElement dontLookUpperThan = null) {
            var parent = self.ParentElement;
            
            while (parent != null) {
                if (parent.HasAttribute(attribName)) {
                    return true;
                }

                if (dontLookUpperThan == parent || parent == Document.Body) {
                    break;
                }

                parent = parent.ParentElement;
            }

            return false;
        }

        public static bool IsInHorizontalPanel(this HTMLElement self, HTMLElement dontLookUpperThan = null) {
            return self.HasParentWithAttribute(Magics.AttrDataIsHorizontalPanel, dontLookUpperThan);
        }

        public static bool IsInVerticalPanel(this HTMLElement self, HTMLElement dontLookUpperThan = null) {
            return self.HasParentWithAttribute(Magics.AttrDataIsVerticalPanel, dontLookUpperThan);
        }
        
        public static bool IsPopupFormView(this Element self) {
            return self.HasAttribute(Magics.AttrDataIsPopup);
        }

        public static void MarkAsFormView(this Element self, bool isPopup) {
            self.SetAttribute(Magics.AttrDataFormview, "");
            if (isPopup) {
                self.SetAttribute(Magics.AttrDataIsPopup, "");
            }
        }

        public static HTMLElement FindFormViewOrNull(this HTMLElement self) {
            if (self.HasAttribute(Magics.AttrDataFormview)) {
                return self;
            }
            if (self == Document.Body || self.ParentElement == null) {
                return null;
            }
            return self.ParentElement.FindFormViewOrNull();
        }

        public static void ActivateMyFormsDefaultButtonIfAny(this HTMLElement self) {
            var form = self.FindFormViewOrNull();
            if (form == null) {
                Logger.Debug(typeof(ElementExtensions), "Element doesn't seem to be contained in any form OR is detached from DOM");
                return;
            }
            var button = form.TraverseUntilFirst(el => el.HasAttribute(Magics.AttrDataDefaultAction));
            if (button == null) {
                Logger.Debug(typeof(ElementExtensions), "Element's form doesn't seem to be have default button");
                return;
            }

            Logger.Debug(typeof(ElementExtensions), "Activating element's form default button");
            button.Click();
        }
        
        public static void MarkAsResizeRecipient(this Element self, bool value) {
            if (value) {
                self.SetAttribute(Magics.AttrDataResizeRecipient, "true");
            } else {
                self.RemoveAttribute(Magics.AttrDataResizeRecipient);    
            }
        }
        
        public static bool IsResizeRecipient(this Element self) {
            return self.HasAttribute(Magics.AttrDataResizeRecipient);
        }
        
        public static void MarkAsMouseEventRecipient(this Element self, bool value) {
            if (value) {
                self.SetAttribute(Magics.AttrDataMouseEventRecipient, "true");
            } else {
                self.RemoveAttribute(Magics.AttrDataMouseEventRecipient);    
            }
        }
        
        public static bool IsMouseEventRecipient(this Element self) {
            return self.HasAttribute(Magics.AttrDataMouseEventRecipient);
        }

        public static void MarkAsTraversable(this Element self, bool value, string forPurposesOrNull = null) {
            if (value) {
                self.RemoveAttribute(Magics.AttrDataNotraverse);
            } else {
                self.SetAttribute(Magics.AttrDataNotraverse, forPurposesOrNull ?? "");    
            }
        }
        
        public static bool IsTraversable(this Element self, string purposeNameOrNull = null) {
            var exists = self.HasAttribute(Magics.AttrDataNotraverse);

            if (!exists) {
                return true;
            }
            
            var blockedPurpose = self.GetAttribute(Magics.AttrDataNotraverse);

            if (string.IsNullOrWhiteSpace(blockedPurpose)) {
                //all purposes blocked
                return false;
            }
            
            //only specific purpose is blocked - others are allowed
            return blockedPurpose != purposeNameOrNull;
        }
        
        /// <summary>
        /// visit all traversable using depth first way. Return count of elements that visitor returned true
        /// </summary>
        public static int TraverseAll(this HTMLElement self, Func<HTMLElement,bool> visitor, string purposeName=null) {
            var result = 0;

            if (visitor(self)) {
                result++;
            }

            if (!self.IsTraversable(purposeName)) {
                return result;   
            }

            foreach (var chld in self.ChildElements()) {
                result += chld.TraverseAll(visitor);
            }

            return result;
        }
        /// <summary>
        /// visit all traversable using depth first way
        /// </summary>
        public static void TraverseAll(
            this HTMLElement self, Action<HTMLElement> visitor, Func<HTMLElement,bool> isTraversable, string purposeName=null) {
            
            if (!isTraversable(self)) {
                return;
            }

            visitor(self);

            if (!self.IsTraversable(purposeName)) {
                return;
            }

            foreach (var chld in self.ChildElements()) {
                chld.TraverseAll(visitor, isTraversable);
            }
        }

        /// <summary>
        /// traverse using depth first way UNTIL first matching element is found. If nothing found null is returned
        /// </summary>
        public static HTMLElement TraverseUntilFirst(this HTMLElement self, Func<HTMLElement,bool> visitor, string purposeNameOrNull = null) {
            return self.TraverseUntilFirst(x => x.IsTraversable(purposeNameOrNull), visitor);
        }

        /// <summary>
        /// find matching sibling(matching html node on the same nesting level). If nothing found null is returned
        /// </summary>
        public static HTMLElement FindFirstMatchingSibling(this HTMLElement self, Func<HTMLElement,bool> isMatching) {
            if (self.ParentElement == null) {
                return null;
            }

            return self.ParentElement.Children.FirstOrDefault(isMatching);
        }

        /// <summary>
        /// traverse using depth first way UNTIL first matching element is found. If nothing found null is returned
        /// </summary>
        public static HTMLElement TraverseUntilFirst(this HTMLElement self, Func<HTMLElement,bool> isTraversable, Func<HTMLElement,bool> visitor) {

            if (visitor(self)) {
                return self;
            }

            if (!isTraversable(self)) {
                return null;
            }

            foreach (var chld in self.ChildElements()) {
                var result = chld.TraverseUntilFirst(isTraversable, visitor);
                if (result != null) {
                    return result;
                }
            }

            return null;
        }

        public static void AddAttachedToDocumentEventListener(this Element self, Action action) {
            DocumentUtil.AddElementAttachedToDocumentListener(self, action);
        }

        public static void AddResizeEventListener(this Element self, Action action) {
            DocumentUtil.AddElementResizeListener(self, action);
        }

        public static Dimensions2D GetClientDimensionsOrNull(this Element self) {
            //don't use clientHeight and clientWidth as they don't have fractional part (are rounded to ceiling) and thus lead to strange bugs
            var rect = self.GetBoundingClientRect();

            if (rect.Height > 0 && rect.Width > 0) {
                return new Dimensions2D(rect.Width, rect.Height);
            }

            return null;
        }

        /// <summary>
        /// width and height - doesn't properly (=slowly) check if element is part of DOM (is attached)
        /// </summary>
        public static Dimensions2D GetOrComputeClientDimensionsOf(this HTMLElement self, HTMLElement ownedBy = null) {
            if (self.ClientHeight > 0 || self.ClientWidth > 0) {
                return new Dimensions2D(self.ClientWidth, self.ClientHeight);
            }

            ownedBy = ownedBy ?? self;

            var oldVis = ownedBy.Style.Visibility;
            var oldPos = ownedBy.Style.Position;

            ownedBy.Style.Visibility = Visibility.Hidden;
            ownedBy.Style.Position = Position.Absolute;

            Document.Body.AppendChild(ownedBy);
            var result = new Dimensions2D(self.ClientWidth, self.ClientHeight);
            Document.Body.RemoveChild(ownedBy);

            ownedBy.Style.Visibility = oldVis;
            ownedBy.Style.Position = oldPos;

            return result;
        }
        
        public static void SetStyle(this HTMLElement self, Dictionary<string,string> elems) {
            foreach (var pair in elems) {
                self.Style.SetProperty(pair.Key, pair.Value);	
            }
        }

        /// <summary>
        /// return negative number if no found
        /// </summary>
        /// <param name="self"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static int IndexOfChild(this Element self, Element child) {
            var chld = self.FirstElementChild;
            var i = 0;

            while (chld != null) {
                if (chld == child) {
                    return i;
                }

                chld = chld.NextElementSibling;
                i++;    
            }

            return -1;
        }

        public static double GetTotalOffsetLeft(this HTMLElement el, double add = 0) {
            var result = el.OffsetLeft;

            while (el.OffsetParent != null) {
                el = el.OffsetParent;
                result += el.OffsetLeft;
            }

            return result;
        }

        public static IEnumerable<HTMLElement> EnumerateSelfAndAncestors(this HTMLElement self) {
            var x = self;
            
            while (x != null) {
                yield return x;
                x = x.ParentElement;
            }
        }

        public static IEnumerable<HTMLElement> ChildElements(this HTMLElement self) {
            return self.ChildNodes
                .Where(x => x.NodeType == NodeType.Element)
                .Select(x => (HTMLElement)x);
        }

        public static void PrependChild(this Element self, Element elementToPrepend) {
            if (self.FirstChild == null) {
                self.AppendChild(elementToPrepend);
                return;
            }
            self.InsertBefore(elementToPrepend, self.FirstChild);
        }

        public static void RemoveAllChildren(this Element self) {
            while (self.FirstChild != null) {
                self.RemoveChild(self.FirstChild);
            }
        }

        /// <summary>breadth-first lookup</summary>
        public static Element FindContainedElementByIdOrNull(this HTMLElement self, string id) {
            var queue = new List<HTMLElement>(self.ChildElements());

            while (queue.Count > 0) {
                var elem = queue[0];
                queue.RemoveAt(0);

                if (elem.Id == id) {
                    return elem;
                } 

                queue.AddRange(elem.ChildElements());
            }

            return null;
        }

        /// <summary>
        /// note: it doesn't support BODY element or anything above it. 
        /// obvious note: node is not its child
        /// </summary>
        /// <param name="self"></param>
        /// <param name="checkedAncestor"></param>
        /// <returns></returns>
        public static bool IsElementOrItsDescendant(this Element self, Element checkedAncestor) {
            return checkedAncestor == self || self.IsDescendantOf(checkedAncestor);
        }
        
        /// <summary>
        /// If this element matches rule OR any of its ancestors matches rule - return that matching element. 
        /// Otherwise return null
        /// </summary>
        /// <param name="self"></param>
        /// <param name="matcher"></param>
        /// <returns></returns>
        public static HTMLElement GetElementOrItsAncestorMatchingOrNull(this HTMLElement self, Func<HTMLElement,bool> matcher) {
            var el = self;

            while (el != null) {
                if (matcher(el)) {
                    return el;
                }
                el = el.ParentElement;
            }

            return null;
        }

        /// <summary>
        /// note: it doesn't support BODY element or anything above it. 
        /// obvious note: node is not its child
        /// </summary>
        /// <param name="self"></param>
        /// <param name="checkedAncestor"></param>
        /// <returns></returns>
        public static bool IsDescendantOf(this Element self, Element checkedAncestor) {
            if (self.ParentElement == null || self.ParentElement == Document.Body) {
                return false;
            }

            if (self.ParentElement == checkedAncestor) {
                return true;
            }

            return self.ParentElement.IsDescendantOf(checkedAncestor);
        }

        /// <summary>
        /// assume that there can be several forms embedded in theselves. 
        /// Available height for element is a Widnow->ClientHeight diminished by all action bars (in current form and all parent forms)
        /// </summary>
        public static int GetAvailableHeightForFormElement(this HTMLElement el, int diminishAvailQty = 0, int extraEmptySpacePerLevel=0) {
            var curType = typeof(ElementExtensions);
            var forms = new List<Tuple<HTMLElement,List<HTMLElement>>>();
            
            var parent = el;
            Logger.Debug(curType, "GetAvailableHeightForFormElement(element id={0})", el.Id);

            while(true) {
                parent = parent.FindFormViewOrNull();

                if (parent == null) {
                    break;
                }
                
                forms.Insert(0, Tuple.Create(parent, new List<HTMLElement>()));
                parent = parent.ParentElement;
            } 
            
            if (!forms.Any()) {
                Logger.Debug(curType, "GetAvailableHeightForFormElement result for formless element {0}", Window.InnerHeight);
                return Window.InnerHeight;
            }

            for (var i=forms.Count-2; i>=0; i--) {
                var siblingsOf = forms[i].Item1;
                var subForms = forms[i].Item2;
                var butNot = forms[i+1].Item1;
                
                Logger.Debug(curType, "GetAvailableHeightForFormElement looking for siblings of {0} that are not {1}", siblingsOf.Id, butNot.Id);
                
                if (butNot.IsInHorizontalPanel(siblingsOf)) {
                    Logger.Debug(curType, "GetAvailableHeightForFormElement determined that {0} is in horizontal panel so ignoring its siblings", siblingsOf.Id);
                    continue;
                }

                siblingsOf.TraverseAll(x => {
                        if (x != siblingsOf && x.HasAttribute(Magics.AttrDataFormview)) {
                            subForms.Add(x);
                            Logger.Debug(curType, "GetAvailableHeightForFormElement found sibling {0}", x.Id);
                        }}, 
                    x => x != butNot && !x.IsPopupFormView());
            }
            
            int? result = null;

            Logger.Debug(curType, "GetAvailableHeightForFormElement element is in form id={0}. Generally it is contained in {1} nested forms", forms.Last().Item1.Id, forms.Count);
            
            foreach (var frm in forms) {     
                Logger.Debug(curType, "GetAvailableHeightForFormElement iteration for form {0} having {1} siblings", frm.Item1.Id, frm.Item2.Count);

                foreach (var subFrm in frm.Item2) {
                    Logger.Debug(curType, "GetAvailableHeightForFormElement has subFrm {0}", subFrm.Id);
                }

                var body = frm.Item1.FirstElementChild.FindFirstMatchingSibling(
                    x => x.ClassList.Contains(Magics.CssClassBody));
                
                var title = body.FindFirstMatchingSibling(x => x.ClassList.Contains(Magics.CssClassTitle));
                var actions = body.FindFirstMatchingSibling(x => x.ClassList.Contains(Magics.CssClassActions));
                
                Logger.Debug(curType, "GetAvailableHeightForFormElement titleId={0} bodyId={1} actionsId={2}",
                    title?.Id, body?.Id, actions?.Id);

                if (body == null) {
                    Logger.Debug(curType, "GetAvailableHeightForFormElement body is null");
                    continue;
                }

                var frmHeight = GetFormHeights(frm.Item1);

                if (!result.HasValue) {    
                    result = Window.InnerHeight - frmHeight.Item2 - diminishAvailQty;
                    Logger.Debug(curType, 
                        "GetAvailableHeightForFormElement initial {0} = {1} - {2} - {3}", result, Window.InnerHeight, frmHeight.Item2, diminishAvailQty);
                } else {
                    Logger.Debug(curType, 
                        "GetAvailableHeightForFormElement next {0} -= {1} + {2}", result, frmHeight.Item2, extraEmptySpacePerLevel);

                    result -= frmHeight.Item2 + extraEmptySpacePerLevel;
                }

                foreach (var subFrm in frm.Item2) {
                    var subFrmHeights = GetFormHeights(subFrm);

                    Logger.Debug(curType, 
                        "GetAvailableHeightForFormElement sibling form {0} diminishes space {1} -= {2}", subFrm.Id, result, subFrmHeights.Item1);

                    result -= subFrmHeights.Item1 + subFrmHeights.Item2;
                }
            }
            
            Logger.Debug(curType, "GetAvailableHeightForFormElement result {0}", result);
            return result.GetValueOrDefault();
        }

        /// <summary>
        /// returns in pixels: 1) Pure form body height px 2) Other occupied space such as scrollbars, actions, title
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        private static Tuple<int,int> GetFormHeights(HTMLElement frm) {   
            var body = frm.FirstElementChild.FindFirstMatchingSibling(
                x => x.ClassList.Contains(Magics.CssClassBody));

            Logger.Debug(typeof(ElementExtensions), "GetFormHeight for form {0} body {1}", frm.Id, body?.Id);
                      
            if (body == null) {
                Logger.Debug(typeof(ElementExtensions), "GetFormHeight body is null");
                return Tuple.Create(0, 0);
            }
            
            var title = body.FindFirstMatchingSibling(x => x.ClassList.Contains(Magics.CssClassTitle));
            var actions = body.FindFirstMatchingSibling(x => x.ClassList.Contains(Magics.CssClassActions));
                
            Logger.Debug(typeof(ElementExtensions), "GetFormHeight for titleId={0} bodyId={1} actionsId={2}",
                title?.Id, body.Id, actions?.Id);

            var formScrollBar = frm.OffsetHeight - frm.ClientHeight;
            var bodyScrollBar = body.OffsetHeight - body.ClientHeight;
            var result = (actions?.OffsetHeight ?? 0) + (title?.OffsetHeight ?? 0) + formScrollBar + bodyScrollBar;
                    
            Logger.Debug(typeof(ElementExtensions), 
                "GetFormHeight result ({0}; {1} = {2} + {3} + {4} + {5})", 
                body.ClientHeight, result, actions?.OffsetHeight ?? 0, title?.OffsetHeight ?? 0, formScrollBar, bodyScrollBar);

            return Tuple.Create(body.ClientHeight, result);
        }

        public static bool IsAttached(this HTMLElement self) {
            if (self == Document.Body) {
                return true;
            }

            if (self.ParentElement == null) {
                return false;
            }

            return self.ParentElement.IsAttached();
        }

        public static bool HasFocus(this HTMLElement self) {
            return self == Document.ActiveElement;
        }
        
        public static void AppendAllChildren(this HTMLElement self, params HTMLElement[] items) {
            items.ForEach(x => self.AppendChild(x));
        }
    }
}
