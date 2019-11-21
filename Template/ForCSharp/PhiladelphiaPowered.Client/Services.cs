
using Philadelphia.Common;

namespace PhiladelphiaPowered.Client {

    public class WebClientHelloWorldService : PhiladelphiaPowered.Domain.IHelloWorldService {
        private readonly IHttpRequester _httpRequester;
        public WebClientHelloWorldService(IHttpRequester httpRequester) { _httpRequester = httpRequester; }
        public async System.Threading.Tasks.Task<System.String>SayHello(System.String p0){
            return await _httpRequester.RunHttpRequestReturningPlain<System.String, System.String>(
                typeof(PhiladelphiaPowered.Domain.IHelloWorldService).FullName,
                "SayHello", p0);
        }
    }




    public class Services {
        public static void Register(IDiRegisterOnlyContainer container) {
            container.RegisterAlias<PhiladelphiaPowered.Domain.IHelloWorldService, WebClientHelloWorldService>(Philadelphia.Common.LifeStyle.Singleton);
        }
    }
}
