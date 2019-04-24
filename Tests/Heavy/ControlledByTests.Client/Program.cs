using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using ControlledByTests.Domain;
using Philadelphia.Web;

namespace ControlledByTests.Client {
    public class HelloWorldFlow : IFlow<HTMLElement> {
        private readonly TextInputForm _askForName;
        private readonly InformationalMessageForm _sayHello;
        private readonly RemoteActionsCallerForm _getAnswer;
        
        public HelloWorldFlow(IHelloWorldService service) {
            _askForName = new TextInputForm(
                "What's your name?", "Hello there",TextType.TreatAsText,"", 
                Validator.IsNotEmptyOrWhitespaceOnly);
            _getAnswer = new RemoteActionsCallerForm(
                x => x.Add(() => 
                    _askForName.Introduced, //service param
                    service.SayHello,
                    y => _sayHello.Init(y))); //consume service reply
            _sayHello = new InformationalMessageForm("", "Server reply");
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
            var di = new DiContainer();
            Services.Register(di); //registers discovered services from model
            di.Register<HelloWorldFlow>(LifeStyle.Transient);

            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();

            var testOrNull = DocumentUtil.GetHashParameterOrNull(MagicsForTests.TestChoiceParamName);

            if (testOrNull == null) {
                Document.Body.AppendChild(new HTMLSpanElement {TextContent = "no test selected"});
            }

            switch (EnumExtensions.GetEnumByLabel<MagicsForTests.ClientSideFlows>(testOrNull)) {
                case MagicsForTests.ClientSideFlows.HelloWorld:
                    di.Resolve<HelloWorldFlow>().Run(renderer);
                    break;

                case MagicsForTests.Serialization.String.Flow: {
                    var iv = new InputView(testOrNull);
                    var inp = LocalValueFieldBuilder.Build("", iv);

                    var isDone = new HTMLSpanElement {Id = MagicsForTests.ResultSpanId};

                    inp.Changed += async (_, __, newValue, errors, isUserChange) => {
                        if (!isUserChange) {
                            return;
                        }

                        var result = await di.Resolve<ISerDeserService>().ProcessString(newValue);

                        //add to make sure that value is of usable type
                        await inp.DoChange(result+MagicsForTests.Serialization.String.ClientAddSuffix, false); 
                        isDone.TextContent = MagicsForTests.ResultSpanReadyValue;
                    };

                    Document.Body.AppendChild(iv.Widget);
                    Document.Body.AppendChild(isDone);
                    break;
                }

                case MagicsForTests.Serialization.Int.Flow: {
                    var iv = new InputView(testOrNull);
                    var inp = LocalValueFieldBuilder.BuildInt(0, iv);
                    var isDone = new HTMLSpanElement {Id = MagicsForTests.ResultSpanId};

                    inp.Changed += async (_, __, newValue, errors, isUserChange) => {
                        if (!isUserChange) {
                            return;
                        }

                        var result = await di.Resolve<ISerDeserService>().ProcessInt(newValue);

                        //add to make sure that value is of usable type
                        await inp.DoChange(result+MagicsForTests.Serialization.Int.ClientAddVal, false); 
                        isDone.TextContent = MagicsForTests.ResultSpanReadyValue;
                    };

                    Document.Body.AppendChild(iv.Widget);
                    Document.Body.AppendChild(isDone);
                    break;
                }

                case MagicsForTests.Serialization.DateTime.FlowUtc: {
                    var iv = new InputView(testOrNull);
                    var inp = LocalValueFieldBuilder.Build(DateTime.Now, iv,
                        dt => dt.ToStringYyyyMmDdHhMm(),
                        str => Convert.ToDateTime(str));
                    var isDone = new HTMLSpanElement {Id = MagicsForTests.ResultSpanId};

                    inp.Changed += async (_, __, newValue, errors, isUserChange) => {
                        if (!isUserChange) {
                            return;
                        }

                        newValue = DateTime.SpecifyKind(newValue, DateTimeKind.Utc);
                        var result = await di.Resolve<ISerDeserService>().ProcessDateTime(newValue, true);

                        //add to make sure that value is of usable type
                        await inp.DoChange(result.AddDays(MagicsForTests.Serialization.DateTime.ClientAddDays), false); 
                        isDone.TextContent = MagicsForTests.ResultSpanReadyValue;
                    };

                    Document.Body.AppendChild(iv.Widget);
                    Document.Body.AppendChild(isDone);
                    break;
                }
                    
                case MagicsForTests.Serialization.DateTime.FlowLocal: {
                    var iv = new InputView(testOrNull);
                    var inp = LocalValueFieldBuilder.Build(DateTime.Now, iv,
                        dt => dt.ToStringYyyyMmDdHhMm(),
                        str => Convert.ToDateTime(str));
                    var isDone = new HTMLSpanElement {Id = MagicsForTests.ResultSpanId};

                    inp.Changed += async (_, __, newValue, errors, isUserChange) => {
                        if (!isUserChange) {
                            return;
                        }

                        newValue = DateTime.SpecifyKind(newValue, DateTimeKind.Local);
                        var result = await di.Resolve<ISerDeserService>().ProcessDateTime(newValue, false);

                        //add to make sure that value is of usable type
                        await inp.DoChange(result.AddDays(MagicsForTests.Serialization.DateTime.ClientAddDays), false); 
                        isDone.TextContent = MagicsForTests.ResultSpanReadyValue;
                    };

                    Document.Body.AppendChild(iv.Widget);
                    Document.Body.AppendChild(isDone);
                    break;
                }
                     
                case MagicsForTests.Serialization.Long.Flow: {
                    var iv = new InputView(testOrNull);
                    var inp = LocalValueFieldBuilder.Build(
                        0L, 
                        iv, 
                        l => l.ToString(),
                        long.Parse);
                    var isDone = new HTMLSpanElement {Id = MagicsForTests.ResultSpanId};

                    inp.Changed += async (_, __, newValue, errors, isUserChange) => {
                        if (!isUserChange) {
                            return;
                        }

                        var result = await di.Resolve<ISerDeserService>().ProcessLong(newValue);

                        //add to make sure that value is of usable type
                        await inp.DoChange(result + MagicsForTests.Serialization.Long.ClientAdd, false);
                        isDone.TextContent = MagicsForTests.ResultSpanReadyValue;
                    };

                    Document.Body.AppendChild(iv.Widget);
                    Document.Body.AppendChild(isDone);
                    break;
                }
                default:
                    Document.Body.AppendChild(new HTMLSpanElement {TextContent = "unsupported test selected"});
                    break;
            }
            
        }
    }
}
