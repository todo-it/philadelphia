using System;
using System.Collections.Generic;
using System.Linq;
using ControlledByTests.Api;
using OpenQA.Selenium.Remote;
using Philadelphia.Common;
using Xunit;

namespace HeavyTests.Helpers {
    public class AssertX {
        ControlledServerController _server;
        private readonly RemoteWebDriver _browser;
        private readonly ICodec _codec;

        public AssertX(ControlledServerController server, RemoteWebDriver browser, ICodec codec) {
            _server = server;
            _browser = browser;
            _codec = codec;
        }

        public void InvocationsMadeOnServerAre(
                Func<FilterInvocation,bool> isRelevant, 
                Func<CsChoice<ServiceCall,FilterInvocation>[]> expectedProvider) {

            InvocationsMadeOnServerAre(isRelevant, expectedProvider());
        }

        public void InvocationsMadeOnServerAre(
                Func<FilterInvocation,bool> isRelevant, 
                params CsChoice<ServiceCall,FilterInvocation>[] expected) {

            var factRaw = _server.ReadAllPendingReplies();

            var fact = factRaw
                .Where(x => x.Type == ReplyType.ServiceInvoked || x.Type == ReplyType.FilterInvoked)
                .Select(x => {
                    switch (x.Type) {
                        case ReplyType.ServiceInvoked: 
                            return CsChoice<ServiceCall,FilterInvocation>.Create(x.DecodeServiceCall(_codec));

                        case ReplyType.FilterInvoked:
                            return CsChoice<ServiceCall,FilterInvocation>.Create(x.DecodeFilterInvoked(_codec));

                        default: throw new Exception("unreachable code reached");
                    }
                })
                .Where(x => !x.Is<FilterInvocation>() || isRelevant(x.As<FilterInvocation>()))
                .ToList();
            
            if (expected.Length != fact.Count) {
                throw new Exception($"collections have different size expected={expected.Length} fact={fact.Count}");
            }

            for (var i = 0; i<expected.Length; i++) {
                var rawExp = expected[i];
                var rawFact = fact[i];
                
                if (!rawExp.Equals(rawFact)) {
                    throw new Exception($"item {i} is different {rawExp} != {rawFact}");
                }

                if (rawExp.Is<FilterInvocation>()) {
                    var exp = rawExp.As<FilterInvocation>();
                    var fac = rawFact.As<FilterInvocation>();
                    
                    if (fac.Guid == null) {
                        throw new Exception("expected that fact FilterInvocation has guid populated");
                    }

                    exp.Guid = fac.Guid;
                }
            }
        }
        
        public void NoInvocationsMadeOnServer() {
            InvocationsMadeOnServerAre(_ => true);
        }
        
        //workaround for "object deserialized as long"
        //https://stackoverflow.com/questions/17918686/how-can-i-deserialize-integer-number-to-int-not-to-long
        private void FixParamTypes(ServiceCall fst, ServiceCall snd) {
            if (fst.Params.Length != snd.Params.Length) {
                throw new Exception("bug: different parameter count");
            }

            for (var i=0; i<fst.Params.Length; i++) {
                if (fst.Params[i] is int && snd.Params[i] is long) {
                    snd.Params[i] = Convert.ToInt32(snd.Params[i]);
                }
                if (fst.Params[i] is long && snd.Params[i] is int) {
                    fst.Params[i] = Convert.ToInt32(fst.Params[i]);
                }
            }
        }

        public void ServiceCallsMadeOnServerAre(params ServiceCall[] expected) {
            var fact = _server
                .ReadAllPendingReplies()
                .Where(x => x.Type == ReplyType.ServiceInvoked)
                .Select(x => x.DecodeServiceCall(_codec))
                .ToList();

            if (expected.Length != fact.Count) {
                throw new Exception($"collections have different size expected={expected.Length} fact={fact.Count}");
            }
            
            var i = 0;
            expected
                .Zip(fact, (e,f) => (i++, e, f))
                .ToList()
                .ForEach(x => {
                    FixParamTypes(x.Item2, x.Item3);

                    if (!x.Item2.Equals(x.Item3)) {
                        throw new Exception($"item {x.Item1} is different {x.Item2} != {x.Item3}");
                    }
                });
        }
        
        public void NoServiceCallsMadeOnServer() {
            ServiceCallsMadeOnServerAre();
        }

        public void DialogIsVisibleInBrowser(string title) {
            Assert.True(
                _browser
                    .FindElementByXPath(XPathBuilder.Dialog(title))
                    .IsMatched());
        }

        public void InputHasValue(XPathBuilder xpath, string expectedValue) {
            Assert.Equal(
                expectedValue, 
                _browser.FindElementByXPath(xpath).GetAttribute("value"));
        }

        public void MatchesXPathInBrowser(XPathBuilder xpath) {
            Assert.True(_browser.FindElementByXPath(xpath).IsMatched());
        }
    }
}
