using Molder.Web.Models.Mediator;
using Molder.Web.Models.Providers;
using Molder.Web.Models.Settings;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Molder.Web.Exceptions;
using Molder.Web.Extensions;
using Molder.Web.Infrastructures;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Elements
{
    public class Element : IElement, ICloneable
    {
        protected IMediator Mediator { get; set; }

        public IDriverProvider Driver { get; set; } = default;
        public IElementProvider ElementProvider { get; set; } = default;

        public Node Root { get; set; }
        public string Name { get; set; }
        public How How { get; set; } = How.XPath;
        public string Locator { get; set; }

        public By By => How.GetBy(Locator);

        public bool Optional { get; set; }

        public Element(string name, string locator, bool optional = false)
        {
            Name = name;
            Locator = locator;
            Optional = optional;
        }

        public Element(How how, string locator)
        {
            Locator = locator;
            How = how;
        }

        public Task<string> Text => Mediator.ExecuteAsync(async () =>
        {
            string result = await ElementProvider.Text;
            return result;
        }).ContinueWith(task =>
        {
            var tcs = new TaskCompletionSource<string>();

            if (task.IsFaulted)
            {
                tcs.SetException(task.Exception.InnerExceptions);
            }
            else if (task.IsCanceled)
            {
                tcs.SetCanceled();
            }
            else
            {
                tcs.SetResult(task.Result as string);
            }
            return tcs.Task;
        }).Unwrap();
        public Task<object> Value => Mediator.ExecuteAsync(async () => await GetAttributeAsync("value"));
        public Task<string> Tag => Mediator.ExecuteAsync(async () =>
        {
            string result = await ElementProvider.Tag;
            return result;
        }).ContinueWith(task =>
        {
            var tcs = new TaskCompletionSource<string>();

            if (task.IsFaulted)
            {
                tcs.SetException(task.Exception.InnerExceptions);
            }
            else if (task.IsCanceled)
            {
                tcs.SetCanceled();
            }
            else
            {
                tcs.SetResult(task.Result as string);
            }
            return tcs.Task;
        }).Unwrap();
        public Task<bool> Loaded
        {
            get
            {
                object result = Mediator.WaitAsync(() => ElementProvider != null);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> NotLoaded
        { 
            get
            {
                object result = Mediator.WaitAsync(() => ElementProvider == null);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> Enabled
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await ElementProvider.Enabled);
                return Task.FromResult((bool)result);
            }    
    }
        public Task<bool> Disabled
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await ElementProvider.Disabled);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> Displayed
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await ElementProvider.Displayed);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> NotDisplayed
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await ElementProvider.NotDisplayed);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> Selected
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await ElementProvider.Selected);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> NotSelected
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await ElementProvider.NotSelected);
                return Task.FromResult((bool)result);
            }
        }
        public Task<bool> Editabled
        {
            get
            {
                object result = Mediator.WaitAsync(async () => await IsEditabledAsync());
                return Task.FromResult((bool)result);
            }
        }

        public Task<bool> NotEditable
        {
            get
            {
                object result = Mediator.WaitAsync(async () => !await IsEditabledAsync());
                return Task.FromResult((bool)result);
            }
        }

        public void SetProvider(IDriverProvider provider)
        {
            Driver = provider;
            Mediator = new ElementMediator(BrowserSettings.Settings.Timeout);
        }

        public async Task GetAsync()
        {
            ElementProvider = await Mediator.ExecuteAsync(async () => await Driver.GetElementAsync(Locator, How)) as
                IElementProvider;
        }

        public async Task<IElement> FindAsync(Node element, How how = How.XPath)
        {
            var by = how.GetBy(((Element)element.Object).Locator.GetStringByRegex());
            ((Element) element.Object).Mediator = new ElementMediator(BrowserSettings.Settings.Timeout);
            ((Element) element.Object).ElementProvider = (IElementProvider)await ElementProvider.FindElementsAsync(by);
            return (IElement)element.Object;
        }

        public async Task<IEnumerable<IElement>> FindAllAsync(Node element, How how = How.XPath)
        {
            var by = how.GetBy(((Element)element.Object).Locator.GetStringByRegex());
            var elements = await ElementProvider.FindElementsAsync(by);
            var listElement = new List<IElement>();
            foreach (var tmpElement in elements)
            {
                ((Element)element.Object).Mediator = new ElementMediator(BrowserSettings.Settings.Timeout);
                ((Element)element.Object).ElementProvider = tmpElement;
                listElement.Add((IElement)element.Object);
            }
            return listElement.AsReadOnly();
        }

        public async Task ClearAsync()
        {
            try
            {
                await ElementProvider.ClearAsync();
            }
            catch (Exception ex)
            {
                throw new ElementException($"Clear element is return error with message {ex.Message}");
            }
        }

        public async Task<string> GetAttributeAsync(string name)
        {
            var result = await Mediator.ExecuteAsync(async () => await ElementProvider.GetAttributeAsync(name));
            return result as string;
        }
        public async Task MoveAsync()
        {
            var driver = await Driver.GetDriverAsync();
            var action = new Actions((IWebDriver)driver);
            await Task.Run(() => action.MoveToElement(((ElementProvider)ElementProvider).WebElement).Build().Perform());
        }
        public async Task PressKeysAsync(string keys)
        {
            var field = typeof(Keys).GetField(keys, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);
            if (await Enabled && await Displayed)
            {
                await Task.Run(async () => await ElementProvider.SendKeysAsync((string)field?.GetValue(null)));
            }
            else
            {
                throw new ArgumentNullException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
            }
        }

        public async Task<bool> IsTextContainsAsync(string text)
        {
            var result = await Mediator.ExecuteAsync(async () => await ElementProvider.TextContainAsync(text));
            return (bool)result;
        }

        public async Task<bool> IsTextEqualsAsync(string text)
        {
            var result = await Mediator.ExecuteAsync(async () => await ElementProvider.TextEqualAsync(text));
            return (bool)result;
        }

        public async Task<bool> IsTextMatchAsync(string text)
        {
            var result = await Mediator.ExecuteAsync(async () => await ElementProvider.TextMatchAsync(text));
            return (bool)result;
        }

        public async Task WaitUntilAttributeValueEqualsAsync(string attributeName, string attributeValue)
        {
            await Mediator.ExecuteAsync(async () => await ElementProvider.WaitUntilAttributeValueEqualsAsync(attributeName, attributeValue));
        }

        private async Task<bool> IsEditabledAsync()
        {
            return await Task.FromResult(Convert.ToBoolean(await GetAttributeAsync("readonly")));
        }

        #region Get webDriver Element

        protected async Task GetElement(string locator, How how = How.XPath)
        {
            ElementProvider = await GetElementByAsync(locator, how);
        }

        private async Task<IElementProvider> GetElementByAsync(string locator, How how = How.XPath)
        {
            return await Mediator.ExecuteAsync(async () => await Driver.GetElementAsync(locator, how)) as
                IElementProvider;
        }

        #endregion

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}