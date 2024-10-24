using System;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Elements
{
    public class Input : Element
    {
        public Input(string name, string locator, bool optional) : base(name, locator, optional) { }

        public virtual async Task SetTextAsync(string text)
        {
            if (await Enabled && await Displayed)
            {
                await Mediator.ExecuteAsync(async () => await ElementProvider.SendKeysAsync(text));
            }
            else
            {
                throw new ArgumentException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
            }
        }

        public new async Task ClearAsync()
        {
            if (await Enabled && await Displayed)
            {
                await Mediator.ExecuteAsync(async () => await ElementProvider.ClearAsync());
            }
            else
            {
                throw new ArgumentException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
            }
        }
    }
}