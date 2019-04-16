using Philadelphia.Common;
    using Philadelphia.Web;

    namespace ControlledByTests.Client {
        public class WebClientHelloWorldService : ControlledByTests.Domain.IHelloWorldService {
            public async System.Threading.Tasks.Task<System.String>SayHello(System.String p0){
                return await HttpRequester.RunHttpRequestReturningPlain<System.String, System.String>(
                    typeof(ControlledByTests.Domain.IHelloWorldService).FullName,
                    "SayHello", p0);
            }
        }
    public class WebClientSerDeserService : ControlledByTests.Domain.ISerDeserService {
            public async System.Threading.Tasks.Task<System.DateTime>ProcessDateTime(System.DateTime p0, System.Boolean p1){
                return await HttpRequester.RunHttpRequestReturningPlain<System.DateTime, System.Boolean, System.DateTime>(
                    typeof(ControlledByTests.Domain.ISerDeserService).FullName,
                    "ProcessDateTime", p0, p1);
            }
            public async System.Threading.Tasks.Task<System.Int32>ProcessInt(System.Int32 p0){
                return await HttpRequester.RunHttpRequestReturningPlain<System.Int32, System.Int32>(
                    typeof(ControlledByTests.Domain.ISerDeserService).FullName,
                    "ProcessInt", p0);
            }
            public async System.Threading.Tasks.Task<System.String>ProcessString(System.String p0){
                return await HttpRequester.RunHttpRequestReturningPlain<System.String, System.String>(
                    typeof(ControlledByTests.Domain.ISerDeserService).FullName,
                    "ProcessString", p0);
            }
        }


    
        public class Services {
            public static void Register(IDiContainer container) {
                container.RegisterFactoryMethod<ControlledByTests.Domain.IHelloWorldService>(injector => new WebClientHelloWorldService(), Philadelphia.Common.LifeStyle.Singleton);
                container.RegisterFactoryMethod<ControlledByTests.Domain.ISerDeserService>(injector => new WebClientSerDeserService(), Philadelphia.Common.LifeStyle.Singleton);
            }
        }
    }
    