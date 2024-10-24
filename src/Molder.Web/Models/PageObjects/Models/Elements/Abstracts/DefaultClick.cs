using Molder.Web.Models.Providers;
using OpenQA.Selenium.Interactions;
using System;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Elements
{
    public abstract class DefaultClick : Element
    {
        public DefaultClick(string name, string locator, bool optional = false) : base(name, locator, optional) { }

        public virtual async Task ClickAsync()
        {
            if (await Enabled && await Displayed)
            {
                await Mediator.ExecuteAsync(async () => await ElementProvider.ClickAsync());
            }
            else
            {
                throw new ArgumentException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
            }
        }

        public virtual async Task DoubleClickAsync()
        {
            if (await Enabled && await Displayed)
            {
                var driver = await Driver.GetDriverAsync();
                var action = new Actions((OpenQA.Selenium.IWebDriver)driver);
                await Mediator.ExecuteAsync(() => action.DoubleClick(((ElementProvider)ElementProvider).WebElement).Build().Perform());
            }
            else
            {
                throw new ArgumentException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
            }
        }

        public virtual async Task ClickAndHold()
        {
            if (await Enabled && await Displayed)
            {
                var driver = await Driver.GetDriverAsync();
                var action = new Actions((OpenQA.Selenium.IWebDriver)driver);
                await Mediator.ExecuteAsync(() => action.ClickAndHold(((ElementProvider)ElementProvider).WebElement).Build().Perform());
            }
            else
            {
                throw new ArgumentException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
            }
        }
    }
}