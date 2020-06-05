using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public interface IFormCanvas<WidgetT> {
        WidgetT Body { set; }
        IEnumerable<WidgetT> Actions { set; }
        Action UserCancel { set; }
        void Show();
        void Hide();
        string Title { set; }
        
        /// <summary>needed because Show() doesn't necessarily mean that form gets focused (e.g. when baseform under popup)</summary>
        void Focus();
    }
}
