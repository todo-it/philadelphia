using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public static class CommonDropdowns {
        public static SingleChoiceDropDown<SomeTraitType?> BuildSomeTraitType() {
            return new SingleChoiceDropDown<SomeTraitType?>(
                "SomeTrait", x => x.HasValue ? x.Value.ToString() : "", 
                UnboundDataGridColumnBuilder
                    .For<SomeTraitType?>("Choose one")
                    .WithValueAsText(x => x, x => x.HasValue ? x.Value.ToString() : "")
                    .Build());
        }
    }
}
