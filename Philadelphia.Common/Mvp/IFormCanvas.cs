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
    }
}
