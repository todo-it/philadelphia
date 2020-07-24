using System;
using System.Globalization;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using ControlledByTests.Domain;
using Philadelphia.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
        private static void RunSerializationTestFlow<TVal>(
            string defaultTypedVal, 
            Converter<string, TVal> stringToTVal,
            Converter<TVal, string> tValToString,
            Func<TVal, Task<TVal>> send
            ) {
            var inputVal =
                DocumentUtil.GetHashParameterOrNull(MagicsForTests.ValueToSend)
                ??
                defaultTypedVal;

            var clientVal = stringToTVal(inputVal);
            var btn = new HTMLButtonElement { Id = MagicsForTests.RunClientSideTestBtnId, TextContent = "Click to test" };
            var resultSpan = new HTMLSpanElement { Id = MagicsForTests.ResultSpanId };

            btn.OnClick += async e => {
                var result = await send(clientVal);
                resultSpan.TextContent = tValToString(result);
            };

            Document.Body.AppendChild(btn);
            Document.Body.AppendChild(resultSpan);
        }

        [Ready]
        public static void OnReady() {
            var di = new PhillyContainer();
            di.RegisterAlias<IHttpRequester, BridgeHttpRequester>(LifeStyle.Singleton);
            Services.Register(di); //registers discovered services from model
            di.Register<HelloWorldFlow>(LifeStyle.Transient);

            Toolkit.InitializeToolkit();
            
            var testOrNull = DocumentUtil.GetHashParameterOrNull(MagicsForTests.TestChoiceParamName);

            if (testOrNull == null) {
                Document.Body.AppendChild(new HTMLSpanElement {TextContent = "no test selected"});
            }

            switch (EnumExtensions.GetEnumByLabel<MagicsForTests.ClientSideFlows>(testOrNull)) {
                case MagicsForTests.ClientSideFlows.ServerSentEvents: {
                        IServerSentEventsService_RegisterListener_SseSubscriber listener = null;
                        var service = di.Resolve<IServerSentEventsService>();

                        var log = new HTMLDivElement {Id = MagicsForTests.RunClientSideTestLogSpanId};
                        log.Style.WhiteSpace = WhiteSpace.Pre;

                        void LogWriteLine(string x) {
                            Logger.Debug(typeof(Program), "adding log line: {0}", x);
                            log.TextContent = log.TextContent + x + "\n";
                        }

                        void DoConnect() { 
                            if (listener != null) {
                                throw new Exception("already connected");
                            }

                            var notifScopeRaw = DocumentUtil.GetHashParameterOrNull(MagicsForTests.ValueToSend);
                            var notifScope = JsonConvert.DeserializeObject<SomeNotifFilter>(notifScopeRaw);

                            listener = new IServerSentEventsService_RegisterListener_SseSubscriber(
                                () => notifScope, false);

                            listener.OnConnOpen += () => LogWriteLine("connected");
                            listener.OnError += (ev,crs) => LogWriteLine($"connection error {(int)crs}");
                            listener.OnMessage += x => LogWriteLine($"received: {x}");

                            listener.Connect();
                        }

                        var sendMsg = new HTMLButtonElement {
                            TextContent = "send", Id = MagicsForTests.RunClientSideTestSendBtnId };
                        sendMsg.OnClick += async _ => {
                            var msgRaw = DocumentUtil.GetHashParameterOrNull(MagicsForTests.ValueToSend);
                            var msg = JsonConvert.DeserializeObject<SomeNotif>(msgRaw);

                            await service.Publish(msg);
                        };

                        var connectAction = new HTMLButtonElement {
                            TextContent = "connect", Id = MagicsForTests.RunClientSideTestConnectId};
                        connectAction.OnClick += _ => DoConnect();
                        
                        var disconnectAction = new HTMLButtonElement {
                            TextContent = "disconnect", Id = MagicsForTests.RunClientSideTestDisconnectId};
                        disconnectAction.OnClick += _ => {
                            listener.Dispose();
                            listener = null;
                        };

                        Document.Body.AppendChild(log);
                        Document.Body.AppendChild(sendMsg);
                        Document.Body.AppendChild(connectAction);
                        Document.Body.AppendChild(disconnectAction);
                        //TODO add disconnect
                    }
                    break;

                case MagicsForTests.ClientSideFlows.HelloWorld:
                    var renderer = Toolkit.DefaultFormRenderer();

                    di.Resolve<HelloWorldFlow>().Run(renderer);
                    break;

                case MagicsForTests.ClientSideFlows.SerializationTest_String:
                    RunSerializationTestFlow(
                        MagicsForTests.Serialization.String.DefaultTypedVal,
                        s => s,
                        d => d,
                        val => di.Resolve<ISerDeserService>().ProcessString(val));
                    break;
                

                case MagicsForTests.ClientSideFlows.SerializationTest_Int: 
                    RunSerializationTestFlow(
                        MagicsForTests.Serialization.Int.DefaultTypedVal,
                        s => int.Parse(s),
                        d => d.ToString(),
                        val => di.Resolve<ISerDeserService>().ProcessInt(val));
                    break;
                

                case MagicsForTests.ClientSideFlows.SerializationTest_DateTimeUtc: 
                    RunSerializationTestFlow(
                        MagicsForTests.Serialization.DateTimeUTC.DefaultTypedVal,
                        s => Convert.ToDateTime(s),
                        d => d.ToStringYyyyMmDdHhMm(),
                        val => di
                              .Resolve<ISerDeserService>()
                              .ProcessDateTime(DateTime.SpecifyKind(val, DateTimeKind.Utc), true));
                    break;
                    
                case MagicsForTests.ClientSideFlows.SerializationTest_DateTimeLocal: 
                    RunSerializationTestFlow(
                        MagicsForTests.Serialization.DateTimeLocal.DefaultTypedVal,
                        s => Convert.ToDateTime(s),
                        d => d.ToStringYyyyMmDdHhMm(),
                        val => di
                              .Resolve<ISerDeserService>()
                              .ProcessDateTime(DateTime.SpecifyKind(val, DateTimeKind.Local), false));
                    break;

                case MagicsForTests.ClientSideFlows.SerializationTest_Long: 
                    RunSerializationTestFlow(
                        MagicsForTests.Serialization.Long.DefaultTypedVal,
                        s => long.Parse(s),
                        d => d.ToString(),
                        val => di.Resolve<ISerDeserService>().ProcessLong(val));
                    break;
                
                case MagicsForTests.ClientSideFlows.SerializationTest_Decimal:
                    RunSerializationTestFlow(
                        MagicsForTests.Serialization.Decimal.DefaultTypedVal,
                        s => decimal.Parse(s, CultureInfo.InvariantCulture),
                        d => d.ToString(CultureInfo.InvariantCulture),
                        val => di.Resolve<ISerDeserService>().ProcessDecimal(val));
                    break;
                
                default:
                    Document.Body.AppendChild(new HTMLSpanElement {TextContent = "unsupported test selected"});
                    break;
            }
            
        }
    }
}
