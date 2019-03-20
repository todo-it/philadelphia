using Bridge;
using Philadelphia.Web;

namespace HelloWorld.Client {
    public class Program {
        [Ready]
        public static void OnReady() {
            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();
            
            var msg = new InformationalMessageForm("Hello world");
            msg.Ended += (x, _) => renderer.Remove(x);

            renderer.AddPopup(msg);
        }
    }
}
