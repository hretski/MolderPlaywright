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
using System.Collections;
using System.Collections.Generic;

namespace Molder.Web.Models.Browser
{
    public abstract class Browser : IBrowser, IDisposable
    {
        #region Node for current *

        public IEnumerable<Node> Pages { get; init; }

        public Node CurrentPage { get; set; }

        #endregion

        public IDriverProvider DriverProvider { get; set; } = new DriverProvider();

        public Task<string> Url => DriverProvider.UrlAsync;
        public Task<string> Title => DriverProvider.TitleAsync;
        public Task<int> Tabs => DriverProvider.TabsAsync;

        //public abstract SessionId SessionId { get; protected set; }

        public async Task SetCurrentPageAsync(string name, bool loading = true)
        {
            Log.Logger().LogInformation($"SetCurrentPage is {name} {(loading ? "with load primary element" : "without load element")}");
            var page = Pages?.SearchPageBy(name);
            CurrentPage = page;

            var objectPage = CurrentPage?.Object as Page;
            ArgumentNullException.ThrowIfNull(objectPage, nameof(CurrentPage));

            objectPage?.SetProvider(DriverProvider);
            objectPage.Root = page;
            objectPage.Local = null;

            if (!loading) return;
            
            try
            {
                await ((Page) CurrentPage.Object).GoToPageAsync();
            }
            catch
            {
                throw new PageException($"Going to page \"{name}\" at \"{(CurrentPage.Object as Page)?.Url}\" failed");
            }
        }

        public async Task UpdateCurrentPageAsync(string name)
        {
            await SetCurrentPageAsync(name, false);
        }

        public IPage GetCurrentPage()
        {
            if (CurrentPage == null)
            {
                throw new NullReferenceException("Current page is null.");
            }
            return CurrentPage.Object as IPage;
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