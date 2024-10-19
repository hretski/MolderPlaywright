﻿using Microsoft.Extensions.Logging;
using Molder.Helpers;
using Molder.Web.Extensions;
using Molder.Web.Infrastructures;
using Molder.Web.Models.Settings;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using OpenQA.Selenium;

namespace Molder.Web.Models.Browser
{
    public class Chrome : Browser
    {
        //public sealed override SessionId SessionId { get; protected set; }

        public Chrome()
        {
            var options = CreateOptions();
            var timeout = BrowserSettings.Settings.Timeout ?? Constants.DEFAULT_TIMEOUT;
            if(BrowserSettings.Settings.IsRemoteRun())
            {
                Log.Logger().LogInformation($@"Start remote chrome browser...");
                DriverProvider.CreateDriverAsync(BrowserType.CHROME);
                // = (DriverProvider.GetDriver() as RemoteWebDriver)?.SessionId;
                //.Logger().LogInformation($@"Remote chrome browser (SessionId is {SessionId}) is starting with options: {Helpers.Message.CreateMessage(options)}");
                return;
            }
            Log.Logger().LogInformation($@"Start chrome browser...");
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true;
            DriverProvider.CreateDriverAsync(BrowserType.CHROME);
            //SessionId = (DriverProvider.GetDriver() as ChromeDriver)?.SessionId;
            //Log.Logger().LogInformation($@"Local chrome browser (SessionId is {SessionId}) is starting with options: {Helpers.Message.CreateMessage(options)}");
        }

        private ChromeOptions CreateOptions()
        {
            var options = new ChromeOptions();
            
            if (BrowserSettings.Settings.IsRemoteRun())
            {
                //options.AddAdditionalCapability("version", BrowserSettings.Settings.Remote.Version ?? Constants.DEFAULT_VERSION, true);
                //options.AddAdditionalCapability("enableVNC", true, true);
                //options.AddAdditionalCapability("platform", BrowserSettings.Settings.Remote.Platform ?? Constants.DEFAULT_PLATFORM, true);
                //options.AddAdditionalCapability("name", BrowserSettings.Settings.Remote.Project ?? Constants.DEFAULT_PROJECT, true);
            }

            if (BrowserSettings.Settings.IsOptions())
            {
                options.AddArguments(BrowserSettings.Settings.Options);
            }
            
            if(!BrowserSettings.Settings.IsBinaryPath())
            {
                options.BinaryLocation = BrowserSettings.Settings.BinaryLocation;
            }

            if (BrowserSettings.Settings.CheckCapability())
            {
                options.AddCapabilities(BrowserSettings.Settings.Capabilities);
            }
            
            if (BrowserSettings.Settings.CheckUserProfilePreference())
            {
                options.AddUserProfilePreference(BrowserSettings.Settings.UserProfilePreference);
            }
            
            return options;
        }
    }
}