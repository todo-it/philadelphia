using System;
using ControlledByTests.Domain;
using OpenQA.Selenium.Remote;
using Philadelphia.Testing.DotnetCore.Selenium;

namespace HeavyTests {
    public static class HeavyTestRunnerExtensions {
        public static void RunServerAndBrowserAndExecute(
            this HeavyTestRunner runner,
            MagicsForTests.ClientSideFlows testFlow, 
            Action<AssertX,ControlledServerController,RemoteWebDriver> testBody) {

            runner.RunServerAndBrowserAndExecute(
                $"#{MagicsForTests.TestChoiceParamName}={testFlow.ToString()}",
                testBody);
        }
    }
}