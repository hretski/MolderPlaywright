using Microsoft.Extensions.Logging;
using Molder.Controllers;
using Molder.Helpers;
using Molder.Models.Configuration;
using Molder.Web.Helpers;
using Molder.Web.Infrastructures;
using Molder.Web.Models;
using Molder.Web.Models.Settings;
using Molder.Web.Extensions;
using TechTalk.SpecFlow;
using BoDi;
using System.Collections.Concurrent;

namespace Molder.Web.Hooks
{
    [Binding]
    public class Hooks : TechTalk.SpecFlow.Steps
    {
        private static readonly ConcurrentBag<IObjectContainer> TestTreadContainers = new();
        [BeforeTestRun(Order = -9000000)]
        public static void InitializeConfiguration()
        {
            var settings = ConfigOptionsFactory.Create(ConfigurationExtension.Instance.Configuration);
            if (settings.Value is null)
            {
                Log.Logger().LogInformation($@"appsettings is not contains {Constants.CONFIG_BLOCK} block. Standard settings selected.");
                BrowserSettings.Settings = new Settings();
            }
            else
            {
                Log.Logger().LogInformation($@"appsettings contains {Constants.CONFIG_BLOCK} block. Settings selected.");
                BrowserSettings.Settings = settings.Value;
            }
        }

        [BeforeScenario(Order = 10000)]
        public static void BeforeFeature(VariableController variableController, IObjectContainer objectContainer)
        {
            var pageObject = new PageObject(variableController);

            objectContainer.RegisterInstanceAs(pageObject.Pages);
        }

        [BeforeScenario(Order = -10)]
        public void BeforeScenario(ScenarioContext scenario)
        {
            var testThreadContainer = scenario.ScenarioContainer.Resolve<IObjectContainer>();
            TestTreadContainers.Add(testThreadContainer);
        }

        [AfterScenario]
        public void AfterScenario(ScenarioContext scenario)
        {
            if (scenario.TestError != null)
            {
                //TODO
                // Add create screenshot: FeatureDirectory/ScenarioDirectory/time_ScenarioName_StepName.png 
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
           var containers = TestTreadContainers.ToArray();
            foreach (var container in containers)
            {
                container.Dispose();
            }
        }
    }
}
