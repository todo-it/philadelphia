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
        private static readonly string[] StandardHtmlFocusableTagNames = {"INPUT","SELECT","TEXTAREA","OPTION"};
        
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

        public static string GetHashParameter(string hashParam) => GetHashParameters()[hashParam];
        public static string GetQueryParameter(string queryParam) => GetQueryParameters()[queryParam];
        public static bool HasHashParameter(string hashParam) => GetHashParameters().ContainsKey(hashParam);
        public static bool HasQueryParameter(string queryParam) => GetQueryParameters().ContainsKey(queryParam);
        
        public static string GetHashParameterOrNull(string hashParam) {
            var x = GetHashParameters();
            return !x.ContainsKey(hashParam) ? null : x[hashParam];
        }

        public static string GetQueryParameterOrNull(string queryParam) {
            var x = GetQueryParameters();
            return !x.ContainsKey(queryParam) ? null : x[queryParam];
        }

        public static IDictionary<string, string> GetHashParameters(string customDocumentUrl = null) {
            return GetQueryParameters(
                "http://example.com?" + new URL(customDocumentUrl ?? Document.URL).hash.Substring(1));
        }

        public static IDictionary<string,string> GetQueryParameters(string customDocumentUrl = null) {
            var u = new URL(customDocumentUrl ?? Document.URL);
                
            return u.searchParams
                .keys()
                .AsIEnumerable()
                .ToDictionary(x => x, x => u.searchParams.get(x));
        }

        public static bool TryCloseTopMostForm() {
            var activeDialogOrNull = Document.Body.GetActiveFormOrNull();

            Logger.Debug(typeof(Toolkit), "TryCloseTopMostForm() hasDialog?={0}", activeDialogOrNull != null);

            if (activeDialogOrNull?.IsCloseable == true) {
                activeDialogOrNull.InvokeClose();
                return true;
            }
            
            Logger.Debug(typeof(Toolkit), "TryCloseTopMostForm() no dialog or not closeable");
            return false;
        }

        public static bool TryActivateDefaultButtonInTopMostForm() {
            var isInInput = 
                Document.ActiveElement != null && 
                StandardHtmlFocusableTagNames.Contains(Document.ActiveElement.TagName);
            var mayHandleEnter = 
                Document.ActiveElement != null &&
                Document.ActiveElement.GetBoolAttribute(Magics.AttrDataHandlesEnter) != true;
            var activeDialogOrNull = Document.Body.GetActiveFormOrNull();

            Logger.Debug(typeof(Toolkit), 
                "TryActivateDefaultButtonInTopMostForm() have focused input/textArea/select?={0} hasDialog?={1} mayHandle?={2}", 
                isInInput, activeDialogOrNull != null, mayHandleEnter);

            if (activeDialogOrNull!= null && mayHandleEnter) {
                var defaultButtonOrNull = activeDialogOrNull.DefaultButtonOrNull;
                Logger.Debug(typeof(Toolkit), 
                    "TryActivateDefaultButtonInTopMostForm() has defaultButton?={0}", defaultButtonOrNull != null);

                if (defaultButtonOrNull != null) {
                    defaultButtonOrNull.Click();
                    return true;
                }
            }

            return false;
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
                    case Magics.KeyCodeEscape:
                        if (TryCloseTopMostForm()) {
                            ev.PreventDefault();
                        }
                        break; 
                    
                    case Magics.KeyCodeEnter:
                        if (TryActivateDefaultButtonInTopMostForm()) {
                            ev.PreventDefault(); //TODO is it needed?
                            ev.StopImmediatePropagation();
                            ev.StopPropagation(); //TODO is it needed?
                        }

                        break;
                }
            };            
        }
    }
}
