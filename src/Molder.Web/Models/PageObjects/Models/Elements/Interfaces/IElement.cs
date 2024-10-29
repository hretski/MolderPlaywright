using System.Collections.Generic;
using System.Threading.Tasks;
using Molder.Web.Infrastructures;
using Molder.Web.Models.Providers;

namespace Molder.Web.Models.PageObjects.Elements
{
    public interface IElement : IEntity
    {
        # region WebEntity
        IDriverProvider Driver { get; set; }
        IElementProvider ElementProvider { get; set; }
        #endregion
        #region Getters & Setters

        Task<string> Text { get; }
        Task<string> Tag { get; }
        Task<object> Value { get; }
        Task<bool> Loaded { get; }
        Task<bool> NotLoaded { get; }
        Task<bool> Enabled { get;  }
        Task<bool> Disabled { get; }
        Task<bool> Displayed { get;  }
        Task<bool> NotDisplayed { get; }
        Task<bool> Selected { get; }
        Task<bool> NotSelected { get; }
        Task<bool> Editabled { get; }
        Task<bool> NotEditable { get; }
        #endregion
        
        Task SetProviderAsync(IDriverProvider provider);
        Task GetAsync();
        Task<IElement> FindAsync(Node element, How how = How.XPath);
        Task<IEnumerable<IElement>> FindAllAsync(Node element, How how = How.XPath);

        Task ClearAsync();
        Task<string> GetAttributeAsync(string name);
        Task MoveAsync();
        Task PressKeysAsync(string keys);

        Task<bool> IsTextContainsAsync(string text);
        Task<bool> IsTextEqualsAsync(string text);
        Task<bool> IsTextMatchAsync(string text);

        object Clone();
    }
}