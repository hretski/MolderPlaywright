using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Elements
{
    public class File : Input
    {
        public File(string name, string locator, bool optional) : base(name, locator, optional) { }

        public override async Task SetTextAsync(string text)
        {
            if (await Driver.GetDriverAsync() is IAllowsFileDetection allowsDetection)
            {
                allowsDetection.FileDetector = new LocalFileDetector();
            }

            await Mediator.ExecuteAsync(async () => await ElementProvider.SendKeysAsync(text));
        }
    }
}