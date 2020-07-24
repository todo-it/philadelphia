using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Tests.Client.Model;
using Philadelphia.Tests.Client.UI;
using Philadelphia.Web;

namespace Philadelphia.Tests.Client {
    public class Program {
        [Ready]
        public static void OnReady() {
            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();

            Document.Title = "Philadelphia.Tests App";
            renderer.ReplaceMaster(new TestRunnerForm(TestModelFactory.CreateFromAssembly(typeof(Program).Assembly)));
        }
    }
}
