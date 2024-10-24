using Molder.Web.Models.PageObjects.Blocks;
using Molder.Web.Models.PageObjects.Elements;
using Molder.Web.Models.PageObjects.Frames;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Pages
{
    public interface IPage
    {
        string Name { get; }
        string Url { get; }
        Node Root { get; set; }
        Node Local { get; set; }

        Task<Block> GetBlockAsync(string name);
        void BackToPage();
        Task<IElement> GetElementAsync(string name);
        Task<IEnumerable<IElement>> GetCollectionAsync(string name);
        IEnumerable<string> GetPrimaryElements();

        Task<IPage> GetDefaultFrameAsync();
        Frame GetParentFrame();
        Frame GetFrame(string name);

        Task GoToPageAsync();
        Task PageTopAsync();
        Task PageDownAsync();

        bool IsLoadElements();
    }
}