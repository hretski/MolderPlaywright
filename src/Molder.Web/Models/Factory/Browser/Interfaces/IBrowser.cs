using Molder.Web.Models.PageObjects.Pages;
using OpenQA.Selenium.Remote;
using System.Threading.Tasks;
using IAlert = Molder.Web.Models.PageObjects.Alerts.IAlert;

namespace Molder.Web.Models.Browser
{
    public interface IBrowser
    {
        Task<string> Url { get; }
        Task<string> Title { get; }
        Task<int> Tabs { get; }

        Task Close();
        Task Quit();
        Task WindowSize(int width, int height);
        Task Maximize();
        Task Back();
        Task Forward();
        Task GoToPage(string url);
        Task Refresh();

        Task SetCurrentPageAsync(string name, bool loading = true);
        Task UpdateCurrentPageAsync(string name);
        IPage GetCurrentPage();

        Task SwitchTo(int number);
        IAlert Alert();

        void Dispose();
    }
}