using System;
using System.IO;
using System.Reflection;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

namespace Philadelphia.Testing.DotNetCore.Selenium {
    public class HeavyTestRunner {
        public delegate void Test(ClientServerAssert assert, ControlledServerController server, RemoteWebDriver browser);

        private readonly Action<string> _logger;
        private readonly TimeSpan MaxWaitForServerStart = TimeSpan.FromSeconds(2);
        private readonly TimeSpan MaxWaitForCmdReply = TimeSpan.FromSeconds(1);
        private readonly TimeSpan MaxWaitForDom = TimeSpan.FromSeconds(1);
        private readonly bool _useHeadlessMode = !System.Diagnostics.Debugger.IsAttached;
        private static readonly ICodec _codec = new VerboseNewtonsoftJsonBasedCodec();
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
                    "../../../../ControlledByTests.Server/bin/Debug/netcoreapp3.1",
                    "dotnet", 
                    "ControlledByTests.Server.dll")) {
                
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

        public void RunBrowserAndExecute(string url, Action<RemoteWebDriver> testBody) {
            var chromeOptions = new ChromeOptions();
            
            if (_useHeadlessMode) {
                chromeOptions.AddArgument("headless");
            }
            
            using (RemoteWebDriver browser = new ChromeDriver(_chromeDriverPath, chromeOptions)) {
                browser.SetDefaultDomTimeout(MaxWaitForDom);
                browser.Url = url;
                testBody(browser);
            }
        }

        public void RunServerAndBrowserAndExecute(
                string baseUrlPostfix, 
                Test testBody) {

            RunServerAndExecute(
                server => RunBrowserAndExecute(
                    baseUrlPostfix, 
                    browser => testBody(
                        new ClientServerAssert(server, browser, new VerboseNewtonsoftJsonBasedCodec()), 
                        server, browser)));
        }
    }
}
