using System;
using System.IO;
using System.Reflection;
using ControlledByTests.Api;
using ControlledByTests.Domain;
using ControlledByTests.ServerSideImpl;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace HeavyTests.Helpers {
    public class HeavyTestRunner {
        private readonly Action<string> _logger;
        private readonly TimeSpan MaxWaitForServerStart = TimeSpan.FromSeconds(2);
        private readonly TimeSpan MaxWaitForCmdReply = TimeSpan.FromSeconds(1);
        private readonly TimeSpan MaxWaitForDom = TimeSpan.FromSeconds(1);
        private bool _useHeadlessMode = !System.Diagnostics.Debugger.IsAttached;
        private static ICodec _codec = new VerboseNewtonsoftJsonBasedCodec();
        private static readonly string _chromeDriverPath = 
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public HeavyTestRunner(Action<string> logger) {
            _logger = logger;
        }

        public void RunServerAndExecute(Action<ControlledServerController> testBody) {
            using (var server = ControlledServerBuilder.Start(
                _logger, 
                _codec,
                MaxWaitForCmdReply,
                "../../../../ControlledByTests.Server/bin/Debug/netcoreapp2.2",
                "dotnet", "ControlledByTests.Server.dll")) {
                
                var deadline = DateTime.UtcNow.Add(MaxWaitForServerStart);

                var success = false;
                while (DateTime.UtcNow < deadline) {
                    var reply = server.ReadReplyOrNull();
                    
                    if (reply?.Type == ReplyType.ServerStarted) {
                        success = true;
                        break;
                    }
                }
                
                testBody(server);

                server.WriteRequest(CommandRequestUtil.CreateStopServer());

                if (!success) {
                    throw new Exception("server didn't acknowledge start within expected time frame");
                }
            }
        }

        public void RunBrowserAndExecute(string baseUrlPostfix, Action<RemoteWebDriver> testBody) {
            var chromeOptions = new ChromeOptions();
            
            if (_useHeadlessMode) {
                chromeOptions.AddArgument("headless");
            }
            
            using (RemoteWebDriver browser = new ChromeDriver(_chromeDriverPath, chromeOptions)) {
                browser.SetDefaultDomTimeout(MaxWaitForDom);
                browser.Url = 
                    "http://localhost:8090" + 
                    (System.Diagnostics.Debugger.IsAttached ? "/full" : "") + 
                    baseUrlPostfix;
                testBody(browser);
            }
        }

        public void RunServerAndBrowserAndExecute(
                string baseUrlPostfix, 
                Action<AssertX,ControlledServerController,RemoteWebDriver> testBody) {

            RunServerAndExecute(
                server => RunBrowserAndExecute(
                    baseUrlPostfix, 
                    browser => testBody(
                        new AssertX(server, browser, new VerboseNewtonsoftJsonBasedCodec()), 
                        server, browser)));
        }

        public void RunServerAndBrowserAndExecute(
            MagicsForTests.ClientSideFlows testFlow, 
            Action<AssertX,ControlledServerController,RemoteWebDriver> testBody) {

            RunServerAndBrowserAndExecute(
                $"#{MagicsForTests.TestChoiceParamName}={testFlow.ToString()}",
                testBody);
        }
    }
}
