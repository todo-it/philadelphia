using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;

namespace Philadelphia.Web {
    public class DefaultInputLogic {
        public static void SetDisabledReasons(Element input, ISet<string> value) {
            if (value.Any()) {
                input.SetAttribute(Magics.AttrDataDisabledTooltip, string.Join("\n", value));
            } else {
                input.RemoveAttribute(Magics.AttrDataDisabledTooltip);
            }
        }

        public static ISet<string> GetErrors(Element input) {
            return new HashSet<string>(
                (input.GetAttribute(Magics.AttrDataErrorsTooltip) ?? "")
                .Split('\n')
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static void SetErrorsTooltip(Element input, ISet<string> value) {
            if (value.Any()) {
                input.SetAttribute(Magics.AttrDataErrorsTooltip, string.Join("\n", value));
            } else {
                input.RemoveAttribute(Magics.AttrDataErrorsTooltip);
            }
        }

        public static void SetErrors(Element container, Element input, bool isUserInput, ISet<string> value) {
            SetErrorsTooltip(input, value);
      
            container.ClassList.Remove("userSuccess");
            container.ClassList.Remove("programmaticSuccess");
            container.ClassList.Remove("userError");
            container.ClassList.Remove("programmaticError");

            container.ClassList.Add(
                (isUserInput ? "user" : "programmatic") +
                (value.Any() ? "Error" : "Success")
            );		
        }
    }
}
