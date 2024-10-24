using System;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Elements
{
    public class A : DefaultClick
    {
        public A(string name, string locator, bool optional = false) : base(name, locator, optional) { }

        public Task<string> Href => GetHrefAsync();

        private async Task<string> GetHrefAsync()
        {
            if (await Enabled && await Displayed)
            {
                var result = await Mediator.ExecuteAsync(async () => await ElementProvider.TextMatchAsync("href"));
                return result as string;
            }

            throw new ArgumentException($"Проверьте, что элемент \"{Name}\" Enabled и Displayed");
        }
    }
}