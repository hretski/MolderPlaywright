using Molder.Helpers;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Selenium.WebDriver.WaitExtensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using Molder.Web.Exceptions;
using Molder.Web.Extensions;
using Molder.Web.Models.Settings;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;
using OpenQA.Selenium.Interactions;

namespace Molder.Web.Models.Providers
{
    [ExcludeFromCodeCoverage]
    public class ElementProviderSelenium : IElementProvider
    {
        private long? _timeout;
        private By _locator;

        #region WebElement

        private IWebElement _element;

        public IWebElement WebElement
        {
            get 
            {
                try 
                { 
                    _element = WebDriver.FindElement(_locator);
                }

                catch(NoSuchElementException ex)
                {
                    Log.Logger().LogError(ex, $"Element {_locator} not found");
                    return null;
                }

                return _element;

            }
            set => _element = value;
        }

        #endregion

        public IWebDriver WebDriver { get; init; }

        public By Locator
        {
            get => _locator;
            set => _locator = value;
        }


        public ElementProviderSelenium(long? timeout, By locator)
        {
            _timeout = timeout;
            _locator = locator;
        }
        
        public Task<bool> Displayed => Task.FromResult(WebElement.Displayed);

        public Task<bool> NotDisplayed => Task.FromResult(!WebElement.Displayed);

        public Task<bool> Selected => Task.FromResult(WebElement.Selected);

        public Task<bool> NotSelected => Task.FromResult(!WebElement.Selected);

        public Task<bool> Enabled => Task.FromResult(WebElement.Enabled);

        public Task<bool> Disabled => Task.FromResult(!WebElement.Enabled);
        
        public Task<bool> Loaded => Task.FromResult(WebElement is not null);

        public Task<bool> NotLoaded => Task.FromResult(WebElement is null);

        public Task<bool> Editabled => IsEditabledAsync();

        public Task<bool> NotEditabled => Editabled.ContinueWith(x =>
        {
            return !x.Result;
        }, TaskContinuationOptions.ExecuteSynchronously);

        public Task<Point> Location => Task.FromResult(WebElement.Location);

        public Task<string> Text => Task.FromResult(WebElement.Text);

        public Task<string> Tag => Task.FromResult(WebElement.TagName);

        public async Task ClearAsync()
        {
            try
            {
                await Task.Run(() => WebElement.Clear());
            }
            catch (Exception ex)
            {
                throw new ElementException($"Clear element is return error with message {ex.Message}");
            }
        }

        public async Task ClickAsync()
        {
            try
            {
                await Task.Run(() => WebElement.Click());
            }
            catch (Exception ex)
            {
                throw new ElementException($"Click element is return error with message {ex.Message}");
            }
        }

        public async Task<bool> TextEqualAsync(string text)
        {
            try
            {
                await Task.Run(() => WebElement.Wait((int)_timeout).ForText().ToEqual(text));
            }
            catch (WebDriverTimeoutException ex)
            {
                Log.Logger().LogWarning($"\"{WebElement.Text}\" is not equal \"{text}\". Exception is {ex.Message}");
                return false;
            }

            return false;
        }

        public async Task<bool> TextContainAsync(string text)
        {
            try
            {
                await Task.FromResult(WebElement.Wait((int)_timeout).ForText().ToContain(text));
            }
            catch (WebDriverTimeoutException ex)
            {
                Log.Logger().LogWarning($"\"{WebElement.Text}\" is not contain \"{text}\". Exception is {ex.Message}");
                return false;
            }

            return false;
        }

        public async Task<bool> TextMatchAsync(string text)
        {
            try
            {
                await Task.Run(() => WebElement.Wait((int)_timeout).ForText());
            }
            catch (WebDriverTimeoutException ex)
            {
                Log.Logger().LogWarning($"\"{WebElement.Text}\" is not match \"{text}\". Exception is {ex.Message}");
                return false;
            }

            return false;
        }

        public async Task<IElementProvider> FindElementAsync(By by)
        {
            var element = WebElement.FindBy(by, WebDriver, (int) BrowserSettings.Settings.Timeout);
            return await Task.FromResult((IElementProvider)new ElementProviderSelenium(_timeout, by)
            {
                WebElement = element
            });
        }

        public async Task<ReadOnlyCollection<IElementProvider>> FindElementsAsync(By by)
        {
            var elements = WebElement.FindAllBy(by, WebDriver, (int) BrowserSettings.Settings.Timeout);
            var listElement = elements.Select(element => new ElementProviderSelenium(_timeout, by) {WebElement = element}).Cast<IElementProvider>().ToList();
            return await Task.FromResult(listElement.AsReadOnly());
        }

        public async Task<string> GetAttributeAsync(string name)
        {
            try
            {
                Log.Logger().LogDebug($"Get attribute by name \"{name}\"");
                return await Task.FromResult(WebElement.GetAttribute(name));
            }
            catch (Exception ex)
            {
                throw new ElementException($"Get attribute by name \"{name}\" is return error with message {ex.Message}");
            }
        }

        public async Task<string> GetCssAsync(string name)
        {
            try
            {
                return await Task.FromResult(WebElement.GetCssValue(name));
            }
            catch (Exception ex)
            {
                throw new ElementException($"GetCssValue by name \"{name}\" is return error with message {ex.Message}");
            }
        }

        public async Task SendKeysAsync(string keys)
        {
            try
            {
                await Task.Run(() => WebElement.SendKeys(keys));
            }
            catch (Exception ex)
            {
                throw new ElementException($"SendKeys \"{keys}\" in element is return error with message {ex.Message}");
            }
        }

        private async Task<bool> IsEditabledAsync()
        {
            return Convert.ToBoolean(await GetAttributeAsync("readonly"));
        }
        
        public async Task WaitUntilAttributeValueEqualsAsync(string attributeName, string attributeValue)
        {      
            var wait = new WebDriverWait(WebDriver, TimeSpan.FromSeconds((long)BrowserSettings.Settings.Timeout));
            WebElement = wait.Until(_ => WebElement.GetAttribute(attributeName) == attributeValue ? WebElement : throw new ElementException($"Waiting until attribute \"{attributeName}\" becomes value \"{attributeValue ?? "null"}\" is failed"));
            await Task.CompletedTask;
        }

        public async Task MoveToElementAsync()
        {
            try
            {
                var action = new Actions(WebDriver);
                await Task.Run(() => action.MoveToElement(WebElement).Build().Perform());
            }
            catch (Exception ex)
            {
                Log.Logger().LogError($"Move to element is return error with message {ex.Message}");
                await Task.FromException(ex);
            }
           
        }
    }
}