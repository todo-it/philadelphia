using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Demo.Client {
    public class DropdownsProgram : IFlow<HTMLElement> {
        private readonly DropdownsDemoForm _simplest;

        public DropdownsProgram() {
            _simplest = new DropdownsDemoForm();
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_simplest);
            _simplest.Ended += (x, _) => {
                renderer.Remove(x);
                atExit();
            };
        }
    }
}
