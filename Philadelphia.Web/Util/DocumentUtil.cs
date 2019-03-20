using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class DocumentUtil {
        private static SpecificChildListChangedObserver _specificChildList;
        private static SpecificResizeObserver _specificResize;
        private static GeneralAttrChangedObserver _generalAttrChanged;
        private static GeneralChildListChangedObserver _generalChildList;
        private static TooltipManager _tooltip;        
        private static bool _isInitialized;
        private static MouseObserver _mouseObserver;
        private static string _oldHash;
        private static IDictionary<string,string> _oldHashParams;
        private static readonly string[] StandardHtmlFocusableTagNames = {"INPUT","SELECT","TEXTAREA","OPTION"};

        public static HTMLElement GetActiveDialogOrNull() {
            return GetVisibleDialogs().LastOrDefault();
        }

        public static IEnumerable<HTMLElement> GetVisibleDialogs() {
            return 
                Document.Body.ChildNodes
                    .Select(rawX => {
                        if (!(rawX is HTMLElement)) {
                            return null;
                        }

                        var x = (HTMLElement)rawX;
                        if (!x.ClassList.Contains(typeof(ModalDialogFromCanvas).FullName)) {
                            return null;
                        }

                        return x;
                    })
                    .Where(x => x != null);
        }

        public static void RemoveCookie(string name, string path="/") {
            var expires = new Bridge.Html5.Date(1970,1,1).ToUTCString();
            Document.Cookie = $"{name}=;path={path};Max-Age=-9999;expires={expires};";
        }

        public static void SetCookie(string name, string value, string path="/", int validDaysSinceNow=1) {
            var x = new Bridge.Html5.Date();
            x.SetDate(x.GetDate() + validDaysSinceNow);
            var expires = x.ToUTCString();
            Document.Cookie = $"{name}={value};path={path};expires="+expires;
        }

        public static HTMLElement CreateElementHavingClassName(string elementName, string className) {
            var result = Document.CreateElement(elementName);
            result.ClassName = className;
            return result;
        }

        public static void AddElementAttachedToDocumentListener(Element el, Action action) {
            _specificChildList.RegisterListenerOnAdded(el, action);
        }

        public static void AddElementResizeListener(Element el, Action action) {
            _specificResize.RegisterListener(el, action);
        }
        
        /// <param name="attributeName">case insensitive</param>
        public static void AddAttributeChangedListener(string attributeName, Action<HTMLElement> action) {
            _generalAttrChanged.RegisterListener(attributeName, action);
        }
        
        public static void AddGeneralElementAttachedToDocumentListener(Action<HTMLElement> action) {
            _generalChildList.RegisterListenerOnAdded(action);
        }
        
        public static void AddMouseDownListener(Element target, Action<MouseEvent> action) {
            _mouseObserver.AddMouseDownListener(target, action);
        }

        public static void AddMouseUpListener(Element target, Action<MouseEvent> action) {
            _mouseObserver.AddMouseUpListener(target, action);
        }
        
        public static void AddMouseClickListener(Element target, Action<MouseEvent> action) {
            _mouseObserver.AddMouseClickListener(target, action);
        }

        public static void AddMouseMoveListener(Element target, Action<MouseEvent> action) {
            _mouseObserver.AddMouseMoveListener(target, action);
        }

        public static string GetHashParameter(string hashParam) {
            return GetHashParameters()[hashParam];
        }

        public static bool HasHashParameter(string hashParam) {
            return GetHashParameters().ContainsKey(hashParam);
        }
        
        public static string GetHashParameterOrNull(string hashParam) {
            var x = GetHashParameters();
            return !x.ContainsKey(hashParam) ? null : x[hashParam];
        }

        public static IDictionary<string,string> GetHashParameters(string customDocumentUrl = null) {
            var hsh = new URL(customDocumentUrl ?? Document.URL).hash;
            if (_oldHash == hsh) {
                return _oldHashParams;
            }
            
            var hashParams = new URL("http://example.com?" + hsh.Substring(1));
            
            _oldHashParams = hashParams.searchParams
                .keys()
                .AsIEnumerable()
                .ToDictionary(x => x, x => hashParams.searchParams.get(x));
            _oldHash = hsh;

            return _oldHashParams;
        }

        public static void Initialize() {
            if (_isInitialized) {
                return;
            }
            _isInitialized = true;
            
            _specificChildList = new SpecificChildListChangedObserver();    
            _generalChildList = new GeneralChildListChangedObserver();
            _specificResize = new SpecificResizeObserver();
            _generalAttrChanged = new GeneralAttrChangedObserver();
            _mouseObserver = new MouseObserver();
            _tooltip = new TooltipManager();
            
            Document.OnKeyDown += ev => {
                switch (ev.KeyCode) {
                    case Magics.KeyCodeEscape: {
                        var activeDialogOrNull = GetActiveDialogOrNull();

                        Logger.Debug(typeof(Toolkit), "Handling ESC key hasDialog?={0}", activeDialogOrNull != null);
                        var clickable = activeDialogOrNull?.TraverseUntilFirst(x => x.HasAttribute(Magics.AttrDataEscListener));
                        if (clickable != null) {
                            ev.PreventDefault();
                            clickable.Click();
                        }
                        break; 
                    }

                    case Magics.KeyCodeEnter: {
                        var isInInput = 
                            Document.ActiveElement != null && 
                            StandardHtmlFocusableTagNames.Contains(Document.ActiveElement.TagName);
                        var mayHandleEnter = 
                            Document.ActiveElement != null &&
                            !Document.ActiveElement.HasAttribute(Magics.AttrDataHandlesEnter);
                        var activeDialogOrNull = GetActiveDialogOrNull();

                        Logger.Debug(typeof(Toolkit), 
                            "Handling ENTER key have focused input/textArea/select?={0} hasDialog?={1} mayHandle?={2}", 
                            isInInput, activeDialogOrNull != null, mayHandleEnter);

                        if (activeDialogOrNull!= null && mayHandleEnter) {
                            var clickable = activeDialogOrNull.TraverseUntilFirst(x => 
                                x.HasAttribute(Magics.AttrDataDefaultAction));

                            if (clickable != null) {
                                ev.PreventDefault();
                                ev.StopImmediatePropagation();
                                ev.StopPropagation();
                                clickable.Click();
                            }
                        }

                        break;
                    }
                }
            };            
        }
    }
}
