using Philadelphia.Common;
    using Philadelphia.Web;

    namespace PhiladelphiaPowered.Client {
        public class WebClientHelloWorldService : PhiladelphiaPowered.Domain.IHelloWorldService {
            public async System.Threading.Tasks.Task<System.String>SayHello(System.String p0){
                return await HttpRequester.RunHttpRequestReturningPlain<System.String, System.String>(
                    typeof(PhiladelphiaPowered.Domain.IHelloWorldService).FullName,
                    "SayHello", p0);
            }
        }


    
        public class Services {
            public static void Register(IDiContainer container) {
                container.RegisterFactoryMethod<PhiladelphiaPowered.Domain.IHelloWorldService>(injector => new WebClientHelloWorldService(), Philadelphia.Common.LifeStyle.Singleton);
            }
        }
    }
    