using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Threading.Tasks;

namespace Molder.Web.Models.Providers
{
    public interface IElementProvider
    {
        Task<bool> Displayed { get; }
        Task<bool> NotDisplayed { get; }
        Task<bool> Selected { get; }
        Task<bool> NotSelected { get; }
        Task<bool> Enabled { get; }
        Task<bool> Disabled { get; }
        Task<bool> Editabled { get; }
        Task<bool> NotEditabled { get; }
        Task<bool> Loaded { get; }
        Task<bool> NotLoaded { get; }
        
        Task<Point> Location { get; }
        Task<string> Text { get; }
        Task<string> Tag { get; }

        Task ClearAsync();
        Task ClickAsync();
        Task<string> GetAttributeAsync(string name);
        Task<string> GetCssAsync(string name);
        Task SendKeysAsync(string keys);

        Task<bool> TextEqualAsync(string text);
        Task<bool> TextContainAsync(string text);
        Task<bool> TextMatchAsync(string text);
        Task WaitUntilAttributeValueEqualsAsync(string attributeName, string attributeValue);

        Task<IElementProvider> FindElementAsync(By by);
        Task<ReadOnlyCollection<IElementProvider>> FindElementsAsync(By by);
    }
}
