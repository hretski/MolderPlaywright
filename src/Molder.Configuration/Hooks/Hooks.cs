﻿using Molder.Configuration.Helpers;
using Molder.Configuration.Models;
using Molder.Configuration.Extension;
using Molder.Controllers;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TechTalk.SpecFlow;
using Molder.Models.Directory;
using System.Threading;
using Molder.Models.Configuration;
using System.Linq;
using Molder.Helpers;
using Microsoft.Extensions.Logging;
using TagHelper = Molder.Configuration.Helpers.TagHelper;

namespace Molder.Configuration.Hooks
{
    [ExcludeFromCodeCoverage]
    [Binding]
    public class Hooks : Steps
    {
        private VariableController controller;

        private static IDirectory BinDirectory = new BinDirectory();
        private static AsyncLocal<IOptions<IEnumerable<ConfigFile>>> config = new();

        public Hooks(VariableController variableController)
        {
            controller = variableController;
        }

        [BeforeTestRun(Order = -10000000)]
        public static void InitializeConfiguration()
        {
            BinDirectory.Create();
            ConfigurationExtension.Instance.Configuration ??= ConfigurationFactory.Create(BinDirectory);
        }

        [BeforeFeature(Order = -1000000)]
        public static void BeforeFeature(FeatureContext featureContext, VariableController variableController)
        {
            ConfigurationExtension.Instance.Configuration ??= ConfigurationFactory.Create(BinDirectory);
            config.Value = ConfigOptionsFactory.Create(ConfigurationExtension.Instance.Configuration);

            var tags = TagHelper.GetTagsBy(featureContext);
            variableController.AddConfig(config.Value, tags);
            featureContext.Copy(variableController);
        }

        [BeforeScenario(Order = -1000000)]
        public void BeforeScenario(FeatureContext feature, ScenarioContext scenario)
        {
            ConfigurationExtension.Instance.Configuration ??= ConfigurationFactory.Create(BinDirectory);
            config.Value = ConfigOptionsFactory.Create(ConfigurationExtension.Instance.Configuration);

            var tags = TagHelper.GetTagsBy(scenario);
            controller.AddConfig(config.Value, tags);
        }

        [AfterScenario(Order = -1000000)]
        public void AfterScenario(FeatureContext feature)
        {
            Log.Logger().LogInformation("Dictionary with variables is " + (controller.Variables.Any() ? "not empty" : "empty"));
            if (!controller.Variables.Any()) return;

            foreach (var (key, variable) in controller.Variables)
            {
                if(variable!.TypeOfAccess == Molder.Infrastructures.TypeOfAccess.Local)
                {
                    controller.Variables.TryRemove(key, out _);
                }
            }

            controller.Reload(feature);
        }
    }
}