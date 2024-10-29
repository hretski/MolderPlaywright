using System.Threading;
using Molder.Web.Models.PageObjects.Blocks;
using Molder.Web.Models.PageObjects.Elements;
using Molder.Web.Models.Mediator;
using Molder.Web.Models.Providers;
using OpenQA.Selenium;
using Molder.Web.Extensions;
using Molder.Web.Infrastructures;
using Molder.Web.Models.Settings;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Frames
{
    public class Frame : Element
    {
        #region Frame Mediator

        private AsyncLocal<IMediator> _frameMediator = new AsyncLocal<IMediator>{ Value = null };

        protected new IMediator mediator
        {
            get => _frameMediator.Value;
            set => _frameMediator.Value = value;
        }

        #endregion
        
        protected string _frameName;
        protected int? _number;
        

        protected Frame(string name, string frameName, int? number, string locator, bool optional = false) : base(name, locator, optional)
        {
            _number = number;
            _frameName = frameName;
        }

        public new async Task SetProviderAsync(IDriverProvider provider)
        {
            ElementProvider = null;
            mediator = new FrameMediator(BrowserSettings.Settings.Timeout);
            Driver = await GetFrameAsync(provider);
        }

        public async Task<IDriverProvider> ParentAsync()
        {
            return await mediator.ExecuteAsync(async () => await Driver.GetParentFrameAsync()) as IDriverProvider;
        }

        public async Task<IDriverProvider> DefaultAsync()
        {
            return await _frameMediator.Value.ExecuteAsync(async () => await Driver.GetDefaultFrameAsync()) as IDriverProvider;
        }

        public async Task<Block> GetBlockAsync(string name)
        {
            var block = Root.SearchElementBy(name, ObjectType.Block);

            await (block.Object as Block)?.SetProviderAsync(Driver);
            await (block.Object as Block)?.GetAsync();
            ((Block) block.Object).Root = block;
            return block.Object as Block;
        }

        public Frame GetFrame(string name)
        {
            var frame = Root.SearchElementBy(name, ObjectType.Frame);

            (frame.Object as Frame)?.SetProviderAsync(Driver);
            ((Frame) frame.Object).Root = frame;
            return frame.Object as Frame;
        }

        public async Task<IElement> GetElementAsync(string name)
        {
            var element = Root.SearchElementBy(name);
            await (element.Object as Element)?.SetProviderAsync(Driver);
            await (element.Object as Element)?.GetAsync();
            ((Element) element.Object).Root = element;
            return (IElement) element.Object;
        }

        private async Task<IDriverProvider> GetFrameAsync(IDriverProvider provider)
        {
            IDriverProvider _driver = default;
            if (_frameName != null)
            {
                _driver = await _frameMediator.Value.ExecuteAsync(async () => await provider.GetFrameAsync(_frameName)) as IDriverProvider;
                return _driver;
            }

            if (_number != null)
            {
                _driver = await _frameMediator.Value.ExecuteAsync(async () => await provider.GetFrameAsync((int)_number)) as IDriverProvider;
                return _driver;
            }

            _driver = await _frameMediator.Value.ExecuteAsync(async () => await provider.GetFrameAsync(By.XPath(Locator))) as IDriverProvider;
            return _driver;
        }
    }
}