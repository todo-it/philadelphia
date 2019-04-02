using System;
using System.Collections.Generic;

namespace HeavyTests.Helpers {
    public class XPathBuilder {
        private readonly List<string> _elems;
        private bool _dialogAsRoot;
        private bool IsClosed {get; set; }

        private XPathBuilder(string root, bool dialogAsRoot) {
            _elems = new List<string>{root};
            _dialogAsRoot = dialogAsRoot;
        }

        public static XPathBuilder Dialog(string title) {
            return new XPathBuilder(
                $"//div[@data-formview]//div[@class='headerTitle' and text() = '{title}']",
                true);
        }
        
        public static XPathBuilder Custom(string fullPath) {
            var res = new XPathBuilder(fullPath, false) {
                IsClosed = true
            };
            return res;
        }
        public XPathBuilder HasButtonAction(string title, bool enabled) {
            //HACK: xpath doesn't seem to have ability to look for items in space-separated-string
            
            if (IsClosed) {
                throw new Exception("current xpath cannot be extended further");
            }

            if (_dialogAsRoot) {
                _elems.Add("../../div[@class='actions']");
            }
            _elems.Add(
                $"/span[contains(@class, 'Philadelphia.Web.InputTypeButtonActionView'){(!enabled ? "" : " and contains(@class, 'enabled')")}]/span[text()='{title}']");

            IsClosed = true;
            return this;
        }
        
        public XPathBuilder HasEnabledButtonAction(string title) {
            return HasButtonAction(title, true);
        }
        
        public XPathBuilder HasReadOnlyLabel(string expectedText) {
            return InBody(
                $"/div[@class='Philadelphia.Web.LabellessReadOnlyView' and text() = '{expectedText}']");
        }

        public XPathBuilder InBody(string xpath) {
            if (IsClosed) {
                throw new Exception("current xpath cannot be extended further");
            }

            if (!_dialogAsRoot) {
                throw new Exception("may only be invoked for dialogs");
            }
            _elems.Add("../../div[@class='body']");
            _elems.Add(xpath);
            
            IsClosed = true;

            return this;
        }

        public string AsString() {
            return string.Join("/", _elems);
        }

        //commented out so that extension with default timeout can be used
        //public static implicit operator string(XPathBuilder x) {
        //    return x.AsString();
        //}
    }
}
