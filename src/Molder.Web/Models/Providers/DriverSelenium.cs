using Molder.Web.Extensions;
using OpenQA.Selenium;
using Selenium.WebDriver.WaitExtensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Molder.Helpers;
using Molder.Web.Exceptions;
using Molder.Web.Infrastructures;
using Molder.Web.Models.Settings;
using OpenQA.Selenium.Support.UI;
using WDSE;
using WDSE.ScreenshotMaker;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using SeleniumExtras.WaitHelpers;

namespace Molder.Web.Models.Providers
{
    [ExcludeFromCodeCoverage]
    public class DriverProviderSelenium : IDriverProvider
    {
        #region  WebDriver
        public IWebDriver WebDriver { get; set; }
        #endregion

        public Task<string> PageSourceAsync => Task.FromResult(WebDriver.PageSource);

        public Task<string> TitleAsync => Task.FromResult(WebDriver.Title);

        public Task<string> UrlAsync => Task.FromResult(WebDriver.Url);

        public Task<int> TabsAsync => Task.FromResult(WebDriver.WindowHandles.Count);

        public Task<string> CurrentWindowHandleAsync => Task.FromResult(WebDriver.CurrentWindowHandle);

        public Task<ReadOnlyCollection<string>> WindowHandlesAsync => Task.FromResult(WebDriver.WindowHandles);

        public async Task BackAsync()
        {
            try
            {
                WebDriver?.Navigate().Back();
            }
            catch (Exception ex)
            {
                throw new DriverException($"Navigate().Back() is return error with message {ex.Message}");
            }
            await Task.CompletedTask;
        }

        public async Task CloseAsync()
        {
            WebDriver?.Close();
            await Task.CompletedTask;
        }

        public async Task CreateDriverAsync(BrowserType browserType)
        {
            WebDriver = browserType switch
            {
                BrowserType.CHROME => new ChromeDriver(),
                BrowserType.FIREFOX => new FirefoxDriver(),
                BrowserType.EDGE => new EdgeDriver(),
                _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
            };
            await Task.CompletedTask;
        }

        public async Task<IWebDriver> GetDriverAsync()
        {
            await Task.Delay(1000);
            return WebDriver;
        }

        public async Task ForwardAsync()
        {
            WebDriver.Navigate().Forward();
            await Task.CompletedTask;
        }

        public async Task<IAlertProvider> GetAlertAsync()
        {
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromMilliseconds((long)BrowserSettings.Settings.Timeout));
            var alert = wait.Until(ExpectedConditions.AlertIsPresent());
            return await Task.Run(() => new AlertProviderSelenium() { Alert = alert });
        }

        public async Task<IDriverProvider> GetDefaultFrameAsync()
        {
            var driver = WebDriver.SwitchTo().DefaultContent();
            driver.Wait((int)BrowserSettings.Settings.Timeout).ForPage().ReadyStateComplete();
            return await Task.Run(() => new DriverProviderSelenium() { WebDriver = driver });
        }

        public async Task<IElementProvider> GetElementAsync(string locator, How how)
        {
            var by = how.GetBy(locator);
            var element = WebDriver.FindElement(@by);
            var provider = new ElementProviderSelenium(BrowserSettings.Settings.Timeout, by) 
            {
                WebElement = element,
                WebDriver = WebDriver
            };
            return await Task.FromResult(provider);
        }

        public async Task<IEnumerable<IElementProvider>> GetElementsAsync(string locator, How how)
        {
            var by = how.GetBy(locator);
            var defaultWait = new DefaultWait<IWebDriver>(WebDriver)
            {
                Timeout = TimeSpan.FromSeconds((double)BrowserSettings.Settings.Timeout),
                PollingInterval = TimeSpan.FromMilliseconds(100)
            };
            var elements = defaultWait.Until(_ => WebDriver.FindElements(@by));
            var listElement = elements.Select(element => new ElementProviderSelenium((int)BrowserSettings.Settings.Timeout, by) { WebElement = element, WebDriver = WebDriver }).Cast<IElementProvider>().ToList();
            return await Task.Run(() => listElement);
        }

        public async Task<IDriverProvider> GetFrameAsync(int id)
        {
            Log.Logger().LogDebug($"SwitchTo().Frame by id \"{id}\"");
            var driver = WebDriver.SwitchTo().Frame(id);
            driver.Wait((int)BrowserSettings.Settings.Timeout).ForPage().ReadyStateComplete();
            return await Task.Run(() => new DriverProviderSelenium() { WebDriver = driver });
        }

        public async Task<IDriverProvider> GetFrameAsync(string name)
        {
            Log.Logger().LogDebug($"SwitchTo().Frame by name \"{name}\"");
            WebDriver.Wait((int)BrowserSettings.Settings.Timeout).ForPage().ReadyStateComplete();
            var driver = WebDriver.SwitchTo().Frame(name);
            driver.Wait((int)BrowserSettings.Settings.Timeout).ForPage().ReadyStateComplete();
            return await Task.Run(() => new DriverProviderSelenium() { WebDriver = driver });
        }

        public async Task<IDriverProvider> GetFrameAsync(By by)
        {
            Log.Logger().LogDebug($"SwitchTo().Frame by locator");
            var element = WebDriver.FindElement(by);
            var driver = WebDriver.SwitchTo().Frame(element);
            driver.Wait((int)BrowserSettings.Settings.Timeout).ForPage().ReadyStateComplete();
            return await Task.Run(() => new DriverProviderSelenium() { WebDriver = driver });
        }

        public async Task<IDriverProvider> GetParentFrameAsync()
        {
            var driver = WebDriver.SwitchTo().ParentFrame();
            driver.Wait((int)BrowserSettings.Settings.Timeout).ForPage().ReadyStateComplete();
            return await Task.Run(() => new DriverProviderSelenium() { WebDriver = driver });
        }

        public async Task GoToUrlAsync(string url)
        {
            WebDriver.Navigate().GoToUrl(url);
            await Task.CompletedTask;
        }

        public async Task MaximizeAsync()
        {
            WebDriver.Manage().Window.Maximize();
            await Task.CompletedTask;
        }

        public async Task QuitAsync()
        {
            WebDriver.Quit();
            WebDriver = null;
            await Task.CompletedTask;
        }

        public async Task RefreshAsync()
        {
            WebDriver.Navigate().Refresh();
            await Task.CompletedTask;
        }

        public async Task<byte[]> ScreenshotAsync()
        {
            var screenshotMaker = new ScreenshotMaker();
            screenshotMaker.RemoveScrollBarsWhileShooting();
            return await Task.Run(() => WebDriver.TakeScreenshot(screenshotMaker));
        }

        public async Task SwitchToAsync(int number)
        {
            Log.Logger().LogInformation($"SwitchTo().Window to number");
            WebDriver.SwitchTo().Window(WebDriver.WindowHandles[number]);
            await Task.CompletedTask;
        }

        public async Task WindowSizeAsync(int width, int height)
        {
            Log.Logger().LogDebug($"Set browser window size as ({width},{height})");
            WebDriver.Manage().Window.Size = new System.Drawing.Size(width, height);
            await Task.CompletedTask;
        }
    }
}