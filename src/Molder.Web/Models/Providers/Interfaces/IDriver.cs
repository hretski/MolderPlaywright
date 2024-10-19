using Molder.Web.Infrastructures;
using Molder.Web.Models.Settings;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Molder.Web.Models.Providers
{
    public interface IDriverProvider
    {
        // Свойства состояния браузера
        Task<string> PageSourceAsync { get; }
        Task<string> TitleAsync { get; }
        Task<string> UrlAsync { get; }
        Task<int> TabsAsync { get; }
        Task<string> CurrentWindowHandleAsync { get; }
        Task<ReadOnlyCollection<string>> WindowHandlesAsync { get; }

        // Методы управления драйвером
        Task CreateDriverAsync(BrowserType browserType);
        Task CloseAsync();
        Task QuitAsync();
        Task WindowSizeAsync(int width, int height);
        Task MaximizeAsync();
        Task BackAsync();
        Task ForwardAsync();
        Task GoToUrlAsync(string url);
        Task RefreshAsync();
        Task SwitchToAsync(int number);

        // Методы взаимодействия с элементами
        Task<IElementProvider> GetElementAsync(string locator, How how);
        Task<IEnumerable<IElementProvider>> GetElementsAsync(string locator, How how);
        Task<IAlertProvider> GetAlertAsync();

        // Методы взаимодействия с фреймами
        Task<IDriverProvider> GetDefaultFrameAsync();
        Task<IDriverProvider> GetParentFrameAsync();
        Task<IDriverProvider> GetFrameAsync(int id);
        Task<IDriverProvider> GetFrameAsync(string name);
        Task<IDriverProvider> GetFrameAsync(By by);

        // Метод для создания скриншота
        Task<byte[]> ScreenshotAsync();
    }
}
