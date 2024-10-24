using Molder.Web.Extensions;
using Molder.Web.Infrastructures;
using Molder.Web.Models.PageObjects.Elements;
using Molder.Web.Models.PageObjects.Frames;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Blocks
{
    public class Block : Element
    {
        public Block(string name, string locator, bool optional) : base(name, locator, optional) { }

        public async Task<Block> GetBlockAsync(string name)
        {
            var block = Root.SearchElementBy(name, ObjectType.Block);

            (block.Object as Block)?.SetProvider(Driver);
            await (block.Object as Block)?.GetAsync();
            ((Block)block.Object).Root = block;
            return (Block)block.Object;
        }

        public async Task<IElement> GetElementAsync(string name)
        {
            var element = Root.SearchElementBy(name);
            (element.Object as Element)?.SetProvider(Driver);
            ((Element)element.Object).Root = element;
            if (Root.Type == ObjectType.Collection)
            {
                var tmpElement = await FindAsync(element);
                tmpElement.Root = element.Root;
                tmpElement.Name = element.Name;
                return tmpElement;
            }
            await (element.Object as Element)?.GetAsync();
            return (IElement)element.Object;
        }

        public Frame GetFrame(string name)
        {
            var frame = Root.SearchElementBy(name, ObjectType.Frame);

            (frame.Object as Frame)?.SetProviderAsync(Driver);
            ((Frame)frame.Object).Root = frame;
            return (Frame)frame.Object;
        }
    }
}