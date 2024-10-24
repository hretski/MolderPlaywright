using System.Collections.Generic;
using System.Linq;
using Molder.Helpers;
using Microsoft.Extensions.Logging;
using Molder.Web.Models.PageObjects.Elements;
using Molder.Web.Models.PageObjects.Blocks;
using Molder.Web.Models.PageObjects.Frames;
using Molder.Controllers;
using Molder.Web.Models.Providers;
using System.Threading.Tasks;

namespace Molder.Web.Models.PageObjects.Pages
{
    public abstract class BasePage : IPage
    {
        protected VariableController _variableController;
        protected IDriverProvider _driverProvider;
        
        public IDriverProvider DriverProvider
        {
            get => _driverProvider;
            set => _driverProvider = value;
        }
        
        public abstract string Url { get; }
        public abstract string Name { get; }
        public abstract Node Root { get; set; }
        public virtual Node Local { get; set; } = null; 

        public abstract Task<Block> GetBlockAsync(string name);
        public void BackToPage() => Local = null;

        public abstract Task<IElement> GetElementAsync(string name);

        public abstract Task<IEnumerable<IElement>> GetCollectionAsync(string name);
        
        public abstract IEnumerable<string> GetPrimaryElements();
        public abstract Task GoToPageAsync();
        public abstract Task PageTopAsync();
        public abstract Task PageDownAsync();
        public bool IsLoadElements()
        {
            var errors = new List<string>();
            var elementsNames = GetPrimaryElements();

            (elementsNames as List<string>)?.ForEach(async name =>
            {
                var element = await GetElementAsync(name);
                if (!(await element.Loaded))
                {
                    errors.Add(name);
                }
            });

            if (!errors.Any()) return true;
            var aggregate = string.Join(", ", errors);
            Log.Logger().LogError($"element/s \"{aggregate}\" not initialize on page \"{Name}\"");
            return false;
        }

        public abstract Task<IPage> GetDefaultFrameAsync();
        public abstract Frame GetParentFrame();
        public abstract Frame GetFrame(string name);
    }
}