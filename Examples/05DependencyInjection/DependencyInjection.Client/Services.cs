using Philadelphia.Common;
    using Philadelphia.Web;

    namespace DependencyInjection.Client {
        public class WebClientHelloWorldService : DependencyInjection.Domain.IHelloWorldService {
            public async System.Threading.Tasks.Task<System.String>SayHello(System.String p0){
                return await HttpRequester.RunHttpRequestReturningPlain<System.String, System.String>(
                    typeof(DependencyInjection.Domain.IHelloWorldService).FullName,
                    "SayHello", p0);
            }
        }


    
        public class Services {
            public static void Register(IDiContainer container) {
                container.RegisterFactoryMethod<DependencyInjection.Domain.IHelloWorldService>(injector => new WebClientHelloWorldService(), Philadelphia.Common.LifeStyle.Singleton);
            }
        }
    }
    