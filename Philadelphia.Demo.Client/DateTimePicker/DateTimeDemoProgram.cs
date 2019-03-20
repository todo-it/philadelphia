using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Demo.Client {
    public class DateTimeDemoProgram : IFlow<HTMLElement> {
        private readonly DateTimeDemoForm _frm;

        public DateTimeDemoProgram() {
            _frm = new DateTimeDemoForm();
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_frm);
            _frm.Ended += (x, _) => {
                renderer.Remove(x);
                atExit();
            };
        }
    }
}
