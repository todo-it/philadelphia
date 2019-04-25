using System;
using System.Linq;
using OpenQA.Selenium.Remote;
using Philadelphia.Common;
using Xunit;
using EnumerableExtensions = Philadelphia.Common.EnumerableExtensions;

namespace Philadelphia.Testing.DotNetCore.Selenium {
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
        private void FixParamTypes(ServiceCall expected, ServiceCall actual) {
            if (expected.Params.Length != actual.Params.Length) {
                throw new Exception("bug: different parameter count");
            }

            for (var i=0; i<expected.Params.Length; i++) {
                if (expected.Params[i] is int && actual.Params[i] is long) {
                    actual.Params[i] = Convert.ToInt32(actual.Params[i]);
                }

                if (expected.Params[i] is long && actual.Params[i] is int) {
                    expected.Params[i] = Convert.ToInt32(expected.Params[i]);
                }

                if(expected.Params[i] is decimal && actual.Params[i] is double) {
                    actual.Params[i] = Convert.ToDecimal(actual.Params[i]);
                }
            }
        }

        public void ServiceCallsMadeOnServerAre(params ServiceCall[] expected) {
            var actual = _server
                .ReadAllPendingReplies()
                .Where(x => x.Type == ReplyType.ServiceInvoked)
                .Select(x => x.DecodeServiceCall(_codec))
                .ToList();

            if (expected.Length != actual.Count) {
                throw new Exception($"collections have different size expected={expected.Length} fact={actual.Count}");
            }
            
            expected
                .Indexed()
                .Zip(actual, (exp, act) => (exp.Index, exp.Value, act))
                .ToList()
                .ForEach(x => {
                    var (i, exp, act) = x;
                    FixParamTypes(exp, act);

                    if (!exp.Equals(act)) {
                        throw new Exception($"item {i} is different {exp} != {act}");
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
