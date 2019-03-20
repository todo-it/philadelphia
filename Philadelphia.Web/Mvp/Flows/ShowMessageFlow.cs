using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ShowMessageFlow : IFlow<HTMLElement> {
        private readonly InformationalMessageForm _msg;

        public ShowMessageFlow(string message, string title, TextType textType = TextType.TreatAsText) {
            _msg = new InformationalMessageForm(message, title, textType);
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_msg);

            _msg.Ended += (x, unit) => {
                renderer.Remove(x);
                atExit();
            };
        }
    }
}
