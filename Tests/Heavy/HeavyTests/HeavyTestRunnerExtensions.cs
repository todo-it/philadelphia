using System;
using System.Collections.Generic;
using System.Linq;
using ControlledByTests.Domain;
using OpenQA.Selenium.Remote;
using Philadelphia.Common;
using Philadelphia.Testing.DotNetCore.Selenium;

namespace HeavyTests {
    public static class HeavyTestRunnerExtensions {
        public static string GenerateUrl(
                MagicsForTests.ClientSideFlows flow, params (string key,string val)[] args) {
            
            var result = 
                "http://localhost:8090" + 
                (System.Diagnostics.Debugger.IsAttached ? "/full" : "") +
                $"#{MagicsForTests.TestChoiceParamName}={flow.ToString()}";
            
            if (!args.Any()) {
                return result;
            }

            return result + "&" + 
                   string.Join(
                       "&", 
                       args.Select(kv => Uri.EscapeDataString(kv.key) +"="+Uri.EscapeDataString(kv.val)));
        }

        public static void RunServerAndBrowserAndExecute(
            this HeavyTestRunner runner,
            MagicsForTests.ClientSideFlows testFlow, 
            HeavyTestRunner.Test testBody) {

            runner.RunServerAndBrowserAndExecute(
                GenerateUrl(testFlow),
                testBody);
        }
    }
}