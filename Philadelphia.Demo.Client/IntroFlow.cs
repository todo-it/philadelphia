using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class IntroFlow : IFlow<HTMLElement> {
        private readonly bool _skipWelcome;
        private readonly InformationalMessageForm _introduction;

        public IntroFlow(bool skipWelcome) {
            _skipWelcome = skipWelcome;
            _introduction = new InformationalMessageForm(
                @"This is Philadelphia Toolkit Demo App!<br>
                Click OK and then use top menu for further demos<br><br>
                <span class='grayedOut'>
                    Notice that when you resize browser then forms stays centered. <br>
                    You can also drag this dialog freely<br>
                    Press enter to automatically activate default form button in this topmost dialog.
                </span>", 
                "Welcome",
                TextType.TreatAsHtml);
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            if (_skipWelcome) {
                atExit();
                return;
            }

            renderer.AddPopup(_introduction);
            
            //what to do when _introduction form wants to quit?
            _introduction.Ended += (x, unit) => {
                renderer.Remove(x); //user wanted to quit so we allow it
                atExit(); //as this is exit step from this simplistic IFlow
            };
        }
    }
}
