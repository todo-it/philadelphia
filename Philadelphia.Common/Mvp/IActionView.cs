using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public interface IActionView<WidgetT> : IView<WidgetT> {
        bool Enabled { get; set;}
        bool OpensNewTab { set; }
        
        bool IsPressed {get; }
        bool StaysPressed {set; }
        ISet<string> DisabledReason { set; } 

        ActionViewState State { set; }

        event Action Triggered;
    }
}
