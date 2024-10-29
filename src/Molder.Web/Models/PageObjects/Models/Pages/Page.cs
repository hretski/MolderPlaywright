using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Molder.Controllers;
using Molder.Extensions;
using Molder.Web.Extensions;
using Molder.Web.Infrastructures;
using Molder.Web.Models.PageObjects.Attributes;
using Molder.Web.Models.PageObjects.Blocks;
using Molder.Web.Models.PageObjects.Elements;
using Molder.Web.Models.PageObjects.Frames;
using Molder.Web.Models.Providers;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace Molder.Web.Models.PageObjects.Pages
{
    public class Page : BasePage
    {
        public override string Name => GetType().GetCustomAttribute<PageAttribute>()?.Name;
        public override string Url => _variableController.ReplaceVariables(GetType().GetCustomAttribute<PageAttribute>()?.Url);
        public override Node Root { get; set; }

        public void SetProvider(IDriverProvider provider)
        {
            DriverProvider = provider;
        }

        public void SetVariables(VariableController variableController) => _variableController = variableController;

        public override async Task<Block> GetBlockAsync(string name)
        {
            var root = Local ?? Root;
            var block = root.SearchElementBy(name, ObjectType.Block);
            await (block.Object as Block)?.SetProviderAsync(DriverProvider);
            await (block.Object as Block)?.GetAsync();
            ((Block) block.Object).Root = block;
            Local = block;
            return (Block) block.Object;
        }

        public override async Task<IElement> GetElementAsync(string name)
        {
            var root = Local ?? Root;
            var element = root.SearchElementBy(name);
            ((IElement) element.Object).Root = element;
            await ((IElement) element.Object).SetProviderAsync(DriverProvider);
            //await ((IElement) element.Object).GetAsync();
            return (IElement) element.Object;
        }

        public override async Task<IEnumerable<IElement>> GetCollectionAsync(string name)
        {
            var root = Local ?? Root;
            var collection = root.SearchCollectionBy(name);
            var elements = await DriverProvider.GetElementsAsync(((IElement) collection.Object).Locator,
                ((IElement) collection.Object).How);
            var lst = new List<IElement>();

            foreach (var element in elements)
            {
                IElement obj = (IElement)((IElement) collection.Object).Clone();
                obj.Root = collection;
                await obj.SetProviderAsync(DriverProvider);
                obj.ElementProvider = element;
                lst.Add(obj);
            }
            return lst;
        }

        public override IEnumerable<string> GetPrimaryElements()
        {
            var elements = Root.Childrens.Where(c => ((IElement) c.Object).Optional == false);
            return elements.Select(element => element.Name).ToList();
        }

        #region Работа с фреймами

        public override async Task<IPage> GetDefaultFrameAsync()
        {
            if (Local is not {Type: ObjectType.Frame}) return this;
            
            DriverProvider = await (Local.Object as Frame)?.DefaultAsync();
            Local = null;
            return this;
        }

        public override Frame GetParentFrame()
        {
            throw new System.NotImplementedException();
        }

        public override Frame GetFrame(string name)
        {
            var root = Local ?? Root;
            var frame = root.SearchElementBy(name, ObjectType.Frame);
            (frame.Object as Frame)?.SetProviderAsync(DriverProvider);
            ((Frame) frame.Object).Root = frame;
            Local = frame;
            return frame.Object as Frame;
        }

        #endregion

        public override async Task GoToPageAsync()
        {
            await DriverProvider.GoToUrlAsync(Url);
        }

        public override async Task PageTopAsync()
        {
            var driver = await DriverProvider.GetDriverAsync();
            var action = new Actions((IWebDriver)driver);
            action.SendKeys(Keys.Control).SendKeys(Keys.Home).Build().Perform();
            action.KeyUp(Keys.Control).Perform();
        }

        public override async Task PageDownAsync()
        {
            var driver = await DriverProvider.GetDriverAsync();
            var action = new Actions((IWebDriver)driver);
            action.SendKeys(Keys.Control).SendKeys(Keys.End).Build().Perform();
            action.KeyUp(Keys.Control).Perform();
        }
    }
}