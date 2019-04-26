using System;
using ControlledByTests.Domain;
using OpenQA.Selenium.Remote;
using Philadelphia.Testing.DotNetCore.Selenium;

namespace HeavyTests {
    public static class HeavyTestRunnerExtensions {
        public static void RunServerAndBrowserAndExecute(
            this HeavyTestRunner runner,
            MagicsForTests.ClientSideFlows testFlow, 
            HeavyTestRunner.Test testBody) {

            runner.RunServerAndBrowserAndExecute(
                $"#{MagicsForTests.TestChoiceParamName}={testFlow.ToString()}",
                testBody);
        }
    }
}