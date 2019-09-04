using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class Program {
        private static DiContainer _di;
        
        [Ready]
        public static void OnReady() {
            _di = new DiContainer();
            Services.Register(_di); //registers discovered services from model
            _di.RegisterAlias<IHttpRequester, BridgeHttpRequester>(LifeStyle.Singleton);
            _di.Register<MainMenuFlow>(LifeStyle.Transient);

            Toolkit.InitializeToolkit(null, x => _di.Resolve<ISomeService>().DataGridToSpreadsheet(x));
            Document.Title = "Philadelphia Toolkit Demo App";

            var forcedCulture = DocumentUtil.GetHashParameterOrNull("forcedCulture");
            I18n.ConfigureImplementation(() => new ToStringLocalization(forcedCulture));

            var renderer = Toolkit.DefaultFormRenderer();
            
            new IntroFlow(Document.URL.Contains("skipWelcome")).Run(
                renderer, 
                //when IntroFlow ends start MainMenuFlow
                //in this simplistic demo it is impossible to quit MainMenuFlow
                () => _di.Resolve<MainMenuFlow>().Run(renderer) 
            );
        }
    }
}
