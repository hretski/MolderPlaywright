using Molder.Web.Exceptions;
using Molder.Web.Extensions;
using Molder.Web.Models.PageObjects.Alerts;
using Molder.Web.Models.PageObjects.Pages;
using Molder.Web.Models.Providers;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Molder.Helpers;
using OpenQA.Selenium.Remote;
using IAlert = Molder.Web.Models.PageObjects.Alerts.IAlert;
using System.Threading.Tasks;

namespace Molder.Web.Models.Browser
{
    public abstract class Browser : IBrowser, IDisposable
    {
        #region Node for current *
        
        private AsyncLocal<Node> _currentPage = new() { Value = null };
        
        #endregion

        private AsyncLocal<IDriverProvider> _provider = new() { Value = new DriverProvider() };

        public IDriverProvider DriverProvider
        {
            get => _provider.Value;
            set => _provider.Value = value;
        }

        public Task<string> Url => DriverProvider.UrlAsync;
        public Task<string> Title => DriverProvider.TitleAsync;
        public Task<int> Tabs => DriverProvider.TabsAsync;

        //public abstract SessionId SessionId { get; protected set; }

        public void SetCurrentPage(string name, bool loading = true)
        {
            Log.Logger().LogInformation($"SetCurrentPage is {name} {(loading ? "with load primary element" : "without load element")}");
            var page = TreePages.Get().SearchPageBy(name);
            _currentPage.Value = page;
            (_currentPage.Value.Object as Page)?.SetProvider(_provider.Value);
            ((Page) _currentPage.Value.Object).Root = page;
            ((Page) _currentPage.Value.Object).Local = null;
            
            if (!loading) return;
            
            try
            {
                ((Page) _currentPage.Value.Object).GoToPage();
            }
            catch
            {
                throw new PageException($"Going to page \"{name}\" at \"{(_currentPage.Value.Object as Page)?.Url}\" failed");
            }
        }

        public void UpdateCurrentPage(string name)
        {
            SetCurrentPage(name, false);
        }

        public IPage GetCurrentPage()
        {
            if (_currentPage == null)
            {
                throw new NullReferenceException("Current page is null.");
            }
            return _currentPage.Value.Object as IPage;
        }

        public async Task Close()
        {
            await DriverProvider.CloseAsync();
        }

        public async Task Quit()
        {
            await DriverProvider.QuitAsync();
        }

        public async Task WindowSize(int width, int height)
        {
            Log.Logger().LogInformation($"Browser size is ({width},{height})");
            await DriverProvider.WindowSizeAsync(width, height);
        }

        public async Task Maximize()
        {
            Log.Logger().LogInformation($"Browser size is maximize");
            await DriverProvider.MaximizeAsync();
        }

        public async Task Back()
        {
            Log.Logger().LogInformation($"Go a back page");
            await DriverProvider.BackAsync();
        }

        public async Task Forward()
        {
            Log.Logger().LogInformation($"Go a forvard page");
            await DriverProvider.ForwardAsync();
        }

        public async Task GoToPage(string url)
        {
            Log.Logger().LogInformation($"GoToUrl {url}");
            await DriverProvider.GoToUrlAsync(url);
        }

        public async Task Refresh()
        {
            Log.Logger().LogInformation($"Refresh page");
            await DriverProvider.RefreshAsync();
        }

        public IAlert Alert()
        {
            return new Alert(DriverProvider);
        }

        public async Task SwitchTo(int number)
        {
            Log.Logger().LogInformation($"Switch to {number} page");
            await DriverProvider.SwitchToAsync(number);
        }

        public Task<byte[]> Screenshot()
        {
             return DriverProvider.ScreenshotAsync();
        }

        public void Dispose() {  }
    }
}