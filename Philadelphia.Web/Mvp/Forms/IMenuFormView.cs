using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface IMenuFormView  : IFormView<HTMLElement> {
        IMenuBarView MenuBar {get; }
    }
}
