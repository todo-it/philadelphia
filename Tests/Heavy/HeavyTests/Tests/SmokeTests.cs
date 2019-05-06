using System;
using System.Threading;
using Xunit;
using ControlledByTests.Domain;
using Newtonsoft.Json;
using Philadelphia.Server.Common;
using Philadelphia.Testing.DotNetCore;
using Philadelphia.Testing.DotNetCore.Selenium;
using Xunit.Abstractions;
using Philadelphia.Common;

namespace HeavyTests.Tests {
    public class SmokeTests {
        private readonly Action<string> _logger;
        const string someName = "somenamehere";
        
        public SmokeTests(ITestOutputHelper logger) {
            _logger = logger.WriteLine;
        }
        
        [Fact]
        public void ServerSentEvents_FullCycle() {
            var flow = MagicsForTests.ClientSideFlows.ServerSentEvents;

            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                flow,
                (assertX, server, browser) => {
                    //
                    //subscribe to notifications
                    //

                    var reqSubsCtx = new SomeNotifFilter {
                        DontAcceptMe = false,
                        AcceptEven = true,
                        AcceptOdd = true,
                        AcceptNegative = true,
                        AcceptPositive = true 
                    };

                    browser.Url = HeavyTestRunnerExtensions.GenerateUrl(
                        flow, 
                        (MagicsForTests.ValueToSend,JsonConvert.SerializeObject(reqSubsCtx
                            )));

                    Poll.WaitForSuccessOrFail(
                        () => browser
                            .FindElementById(MagicsForTests.RunClientSideTestLogSpanId)
                            .Text
                            .Then(string.IsNullOrWhiteSpace));

                    assertX.NoServiceCallsMadeOnServer();

                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestConnectId)
                        .Click();

                    Poll.WaitForSuccessOrFail(
                        () => browser
                            .FindElementById(MagicsForTests.RunClientSideTestLogSpanId)
                            .Text
                            .Then(x => x == "connected"));
                    
                    FilterInvocation onNewClient = null;

                    assertX.InvocationsMadeOnServerAre(_ => true, () => {
                        onNewClient = FilterInvocation
                            .OfMethod((IServerSentEventsService x) => x.RegisterListener(reqSubsCtx))
                            .With(x => x.ResType = ResourceType.ServerSentEventListener);

                        var actualCall = ServiceCall.OfMethod(
                            (IServerSentEventsService x) => x.RegisterListener(reqSubsCtx));

                        return new CsChoice<ServiceCall,FilterInvocation>[]{onNewClient,actualCall};
                    });
                        

                    //
                    // echo message #1
                    //
                    var msg1 = new SomeNotif {Num = 5};
                    browser.Url = HeavyTestRunnerExtensions.GenerateUrl(
                        flow, 
                        (MagicsForTests.ValueToSend,JsonConvert.SerializeObject(msg1)));

                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestSendBtnId)
                        .Click();
                    
                    Poll.WaitForSuccessOrFail(
                        () => browser
                            .FindElementById(MagicsForTests.RunClientSideTestLogSpanId)
                            .Text
                            .Then(x => x.EndsWith("\nreceived: <SomeNotif Num=5 Prop=>")));
                    
                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((IServerSentEventsService x) => x.Publish(msg1)),
                        new ServiceCall {
                            FullInterfaceName = typeof(IServerSentEventsService).FullName,
                            MethodName = nameof(IServerSentEventsService.RegisterListener),
                            Params = new object[]{reqSubsCtx, msg1}
                        });

                    //
                    // echo message #2
                    //
                    var msg2 = new SomeNotif {Num = 7};
                    browser.Url = HeavyTestRunnerExtensions.GenerateUrl(
                        flow, 
                        (MagicsForTests.ValueToSend,JsonConvert.SerializeObject(msg2)));

                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestSendBtnId)
                        .Click();
                    
                    Poll.WaitForSuccessOrFail(
                        () => browser
                            .FindElementById(MagicsForTests.RunClientSideTestLogSpanId)
                            .Text
                            .Then(x => x.EndsWith("\nreceived: <SomeNotif Num=7 Prop=>")));
                    
                    assertX.ServiceCallsMadeOnServerAre(
                        ServiceCall.OfMethod((IServerSentEventsService x) => x.Publish(msg2)),
                        new ServiceCall {
                            FullInterfaceName = typeof(IServerSentEventsService).FullName,
                            MethodName = nameof(IServerSentEventsService.RegisterListener),
                            Params = new object[]{reqSubsCtx, msg2}
                        });
                    
                    //
                    // disconnecting
                    //
                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestDisconnectId)
                        .Click();
                    
                    Thread.Sleep(500); //wait for disconnect being acknowledged or the server side
                    
                    assertX.InvocationsMadeOnServerAre(
                        _ => true,
                        () => {
                            return new CsChoice<ServiceCall,FilterInvocation>[] {
                                FilterInvocation.ExpectOnConnectionAfterFor(onNewClient)
                            };
                        });

                    //
                    // should not receive message now
                    //
                    var logContent = browser
                        .FindElementById(MagicsForTests.RunClientSideTestLogSpanId)
                        .Text;

                    browser.Url = HeavyTestRunnerExtensions.GenerateUrl(
                        flow, 
                        (MagicsForTests.ValueToSend,JsonConvert.SerializeObject(
                            new SomeNotif {Num = 27})));

                    browser
                        .FindElementById(MagicsForTests.RunClientSideTestSendBtnId)
                        .Click();
                    
                    Thread.Sleep(500); //process message forwarding on server

                    Poll.WaitForSuccessOrFail(
                        () => browser
                            .FindElementById(MagicsForTests.RunClientSideTestLogSpanId)
                            .Text
                            .Then(x => x == logContent)); //expects to not receive message
                });
        }

        [Fact]
        public void HelloWorldTest_CyclesThroughWholeProgram() {
            new HeavyTestRunner(_logger).RunServerAndBrowserAndExecute(
                MagicsForTests.ClientSideFlows.HelloWorld, (assertX, server, browser) => {
                
                assertX.DialogIsVisibleInBrowser("Hello there");
                
                browser
                    .FindElementByXPath(XPathBuilder.Dialog("Hello there").InBody("/input[@type='text']"))
                    .SendKeys(someName);
                        
                assertX.NoServiceCallsMadeOnServer();
                
                browser
                    .FindElementByXPath(XPathBuilder.Dialog("Hello there").HasEnabledButtonAction("OK"))
                    .Click();
                
                assertX.DialogIsVisibleInBrowser("Server reply");
                
                assertX.InvocationsMadeOnServerAre(x => x.ResType == ResourceType.RegularPostService, () => {
                    var onBefore = FilterInvocation.OfMethod((IHelloWorldService x) => x.SayHello(someName));
                    var actualCall = ServiceCall.OfMethod((IHelloWorldService x) => x.SayHello(someName));
                    var onAfter = FilterInvocation.ExpectOnConnectionAfterFor(onBefore);

                    return new CsChoice<ServiceCall,FilterInvocation>[]{onBefore,actualCall,onAfter};
                });
                
                assertX.MatchesXPathInBrowser(
                    XPathBuilder
                        .Dialog("Server reply")
                        .HasReadOnlyLabel($"Hello {someName}. How are you?"));
            });
        }

        [Fact]
        public void StartsServer_GetsItsStartConfirmation() {
            new HeavyTestRunner(_logger).RunServerAndExecute(
                _ => {/*nothing here as I just want to sure that server confirmed its start*/});
        }
    }
}
