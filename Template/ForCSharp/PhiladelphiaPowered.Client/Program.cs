using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using PhiladelphiaPowered.Domain;
using Philadelphia.Web;

namespace PhiladelphiaPowered.Client {
    public class HelloFlow : IFlow<HTMLElement> {
        private readonly TextInputForm _askForName;
        private readonly InformationalMessageForm _sayHello;
        private readonly RemoteActionsCallerForm _getAnswer;
        
        public HelloFlow(IHelloWorldService service) {
            _askForName = new TextInputForm("What's your name?", "Hello there");
            _getAnswer = new RemoteActionsCallerForm(
                x => x.Add(() => 
                    _askForName.Introduced, //service param
                    service.SayHello,
                    y => _sayHello.Init(y))); //consume service reply
            _sayHello = new InformationalMessageForm();
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            _askForName.Ended += async (x, outcome) => {
                renderer.Remove(x);

                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        renderer.AddPopup(_getAnswer);
                        break;

                    case CompletedOrCanceled.Canceled:
                        await _sayHello.Init("You didn't provide name thus server won't be called");
                        renderer.AddPopup(_sayHello);
                        break;
                }
            };

            _getAnswer.Ended += async (x, outcome) => {
                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        break; //message is already prepared

                    case RemoteActionsCallerForm.Outcome.Canceled:
                        await _sayHello.Init("Looks like request was canceled");
                        break;

                    case RemoteActionsCallerForm.Outcome.Interrupted:
                        await _sayHello.Init("Looks like request failed");
                        break;
                }
                
                renderer.Remove(x);
                renderer.AddPopup(_sayHello);
            };

            _sayHello.Ended += (x, unit) => {
                renderer.Remove(x);
                renderer.AddPopup(_askForName);
            };

            renderer.AddPopup(_askForName);
        }
    }

    public class Program {
        [Ready]
        public static void OnReady() {
            var di = new PhillyContainer();
            Services.Register(di); //registers discovered services from model
            di.Register<HelloFlow>(LifeStyle.Transient);

            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();
            
            var helloService = di.Resolve<IHelloWorldService>();

            Document.Title = "PhiladelphiaPowered App";
            di.Resolve<HelloFlow>().Run(renderer);
        }
    }
}
