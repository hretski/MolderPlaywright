using Molder.Controllers;
using Molder.Web.Controllers;
using System;
using System.Diagnostics;
using System.Linq;
using Molder.Extensions;
using Molder.Web.Infrastructures;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Molder.Web.Models.PageObjects.Elements;
using Molder.Web.Models.Proxy;
using System.Threading.Tasks;
using BoDi;
using Molder.Web.Models.Settings;
using System.Collections.Generic;
using Molder.Web.Models;
using FluentAssertions;

namespace Molder.Web.Steps
{
    [Binding]
    public class Steps
    {
        private readonly VariableController variableController;
        private readonly FeatureContext featureContext;
        private readonly ScenarioContext scenarioContext;
        private  IObjectContainer objectContainer;
        private readonly Settings settings;
        private readonly IEnumerable<Node> pages;

        public Steps(
            VariableController variableController,
            FeatureContext featureContext,
            ScenarioContext scenarioContext,
            IObjectContainer objectContainer,
            IEnumerable<Node> pages
            )
        {
            this.variableController = variableController;
            this.featureContext = featureContext;
            this.scenarioContext = scenarioContext;
            this.objectContainer = objectContainer;
            this.pages = pages;
        }

        [StepArgumentTransformation]
        public Authentication Authentication(Table table)
        {
            return table.CreateInstance<Authentication>();
        }

        [Given(@"я инициализирую браузер")]
        public void StartBrowser()
        {
            var browser = BrowserController.Create(pages);
            objectContainer.RegisterInstanceAs(browser);
            scenarioContext.ScenarioContainer.RegisterInstanceAs(browser);
        }

        [StepDefinition(@"установлено разрешение окна браузера ([0-9]+) X ([0-9]+)")]
        public async Task SetSizeBrowserWindow(int width, int height)
        {
            await BrowserController.GetBrowser().WindowSize(width, height);
        }

        [StepDefinition(@"я развернул веб-страницу на весь экран")]
        public async Task MaximizeWindow()
        {
            await BrowserController.GetBrowser().Maximize();
        }

        [Given(@"я перехожу на страницу \""(.+)\""")]
        public async Task SetCurrentPageAsync(string name)
        {
            await BrowserController.GetBrowser().SetCurrentPageAsync(name);
        }

        [StepDefinition(@"я обновляю текущую страницу на \""(.+)\""")]
        public async Task UpdateCurrentPageAsync(string name)
        {
            await BrowserController.GetBrowser().UpdateCurrentPageAsync(name);
        }

        [StepDefinition(@"я обновляю веб-страницу")]
        public async Task Refresh()
        {
            await BrowserController.GetBrowser().Refresh();
        }

        [StepDefinition(@"я сохраняю адрес активной веб-страницы в переменную \""(.+)\""")]
        public async Task SaveUrlActivePage(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var url = await BrowserController.GetBrowser().Url;
            variableController.SetVariable(varName, url.GetType(), url);
        }

        [StepDefinition(@"я сохраняю заголовок активной веб-страницы в переменную \""(.+)\""")]
        public async Task SaveTitleActiveWebPage(string varName)
        {
            variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var title = await BrowserController.GetBrowser().Title;
            variableController.SetVariable(varName, title.GetType(), title);
        }

        [StepDefinition(@"я закрываю веб-страницу")]
        public async Task CloseWebPage()
        {
            await BrowserController.GetBrowser().Close();
        }

        [StepDefinition(@"я закрываю браузер")]
        public void CloseBrowser()
        {
            BrowserController.Quit();
        }

        [StepDefinition(@"совершен переход в начало веб-страницы")]
        public async Task GoPageTop()
        {
            await BrowserController.GetBrowser().GetCurrentPage().PageTopAsync();
        }

        [StepDefinition(@"совершен переход в конец веб-страницы")]
        public async Task GoPageDownAsync()
        {
            await BrowserController.GetBrowser().GetCurrentPage().PageDownAsync();
        }

        [StepDefinition(@"выполнен переход на вкладку номер ([1-9]+)")]
        public async Task GoToTabByNumberAsync(int number)
        {
            (number--).Should().BePositive("неверно задан номер вкладки");
            int tabsCount = await BrowserController.GetBrowser().Tabs;
            number.Should().BeLessOrEqualTo(tabsCount,
                "выбранной вкладки не существует");

            await BrowserController.GetBrowser().SwitchTo(number);
        }

        #region Проверка работы с Alert
        [StepDefinition(@"я создаю переменную \""(.+)\"" с текстом из диалогового окна на веб-странице")]
        public void SetVariableValueOfAlertText(string varName)
        {
            this.variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var alert = BrowserController.GetBrowser().Alert();
            this.variableController.SetVariable(varName, alert.Text.GetType(), alert.Text);
        }

        [StepDefinition(@"выполнено нажатие на \""(Accept|Dismiss)\"" в диалоговом окне на веб-странице")]
        public void AlertClick(string key)
        {
            var alert = BrowserController.GetBrowser().Alert();

            switch (key)
            {
                case "Accept":
                    alert.Accept();
                    break;
                case "Dismiss":
                    alert.Dismiss();
                    break;
                default:
                    /// TODO добавить текст ошибки
                    throw new ArgumentOutOfRangeException("Key for alert is not valid");
            }
        }

        [Then(@"я убеждаюсь, что на веб-странице появилось диалоговое окно")]
        public void CheckAlert()
        {
            var alert = BrowserController.GetBrowser().Alert();
            alert.Should().NotBeNull("диалоговое окно не найдено");
        }
        #endregion

        [Then(@"текущая страница загрузилась")]
        public void PageIsLoaded()
        {
            var loaded = BrowserController.GetBrowser().GetCurrentPage().IsLoadElements();
            loaded.Should().BeTrue($"страница \"{BrowserController.GetBrowser().GetCurrentPage().Name}\" не загрузилась");
        }

        #region Проверка адреса активной веб страницы
        [Then(@"адрес активной веб-страницы содержит значение \""(.+)\""")]
        public async Task WebPageUrlContainsExpectedAsync(string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = variableController.ReplaceVariables(expected) ?? expected;
            var url = await BrowserController.GetBrowser().Url;
            url.Should().Contain(expected, $"адрес активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Url}\" не содержит \"{expected}\"");
        }
        

        [Then(@"адрес активной веб-страницы не содержит значение \""(.+)\""")]
        public async Task WebPageUrlNotContainsExpectedAsync(string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            var url = await BrowserController.GetBrowser().Url;
           url.Should().NotContain(expected, $"адрес активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Url}\" содержит \"{expected}\"");
        }

        [Then(@"адрес активной веб-страницы равен значению \""(.+)\""")]
        public async Task WebPageUrlEqualExpectedAsync(string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            var url = await BrowserController.GetBrowser().Url;
            url.Should().Be(expected, $"адрес активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Url}\" не равен \"{expected}\"");
        }

        [Then(@"адрес активной веб-страницы не равен значению \""(.+)\""")]
        public async Task WebPageUrlNotEqualExpectedAsync(string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            var url = await BrowserController.GetBrowser().Url;
            url.Should().NotBe(expected, $"адрес активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Url}\" равен \"{expected}\"");
        }

        #endregion
        #region Проверка заголовка активной веб страницы
        [Then(@"заголовок веб-страницы равен значению \""(.+)\""")]
        public async Task WebPageTitleIsEqualAsync(string title)
        {
            title.Should().NotBeNull($"значение \"expected\" не задано");
            title = this.variableController.ReplaceVariables(title) ?? title;
            var titleActual = await BrowserController.GetBrowser().Title;
            titleActual.Should().Be(title, $"заголовок активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Title}\" не равен \"{title}\"");
        }

        [Then(@"заголовок веб-страницы не равен значению \""(.+)\""")]
        public async Task WebPageTitleIsNotEqualAsync(string title)
        {
            title.Should().NotBeNull($"значение \"expected\" не задано");
            title = this.variableController.ReplaceVariables(title) ?? title;
            var titleActual = await BrowserController.GetBrowser().Title;
            titleActual.Should().NotBe(title, $"заголовок активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Title}\" равен \"{title}\"");
        }

        [Then(@"заголовок веб-страницы содержит значение \""(.+)\""")]
        public async Task WebPageTitleIsContainsAsync(string title)
        {
            title.Should().NotBeNull($"значение \"expected\" не задано");
            title = this.variableController.ReplaceVariables(title) ?? title;
            var titleActual = await BrowserController.GetBrowser().Title;
            titleActual.Should().Contain(title, $"заголовок активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Title}\" не содержит \"{title}\"");
        }

        [Then(@"заголовок веб-страницы не содержит значение \""(.+)\""")]
        public async Task WebPageTitleIsNotContainsAsync(string title)
        {
            title.Should().NotBeNull($"значение \"expected\" не задано");
            title = this.variableController.ReplaceVariables(title) ?? title;
            var titleActual = await BrowserController.GetBrowser().Title;
            titleActual.Should().NotContain(title, $"заголовок активной веб страницы \"{BrowserController.GetBrowser().GetCurrentPage().Name}\":\"{BrowserController.GetBrowser().Title}\" содержит \"{title}\"");
        }
        
        #endregion
        
        #region Elements

        [StepDefinition(@"я перемещаюсь к элементу \""(.+)\"" на веб-странице")]
        public async Task ScrollToElementAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            await element.MoveAsync();
        }

        [StepDefinition(@"выполнено нажатие на элемент \""(.+)\"" на веб-странице")]
        public async Task ClickToWebElementAsync(string name)
        {
            var stopwatch = new Stopwatch();
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            stopwatch.Start();
            try
            {
                var isDisplayed = await element.Displayed;
                while (!isDisplayed && stopwatch.Elapsed.Ticks < (TimeSpan.TicksPerSecond * Constants.TIC_IN_SEC))
                {
                    isDisplayed = await element.Displayed;
                }

                (element is DefaultClick).Should().BeTrue($"элемент \"{name}\" имеет отличный от Click профиль");
                (element as DefaultClick)?.ClickAsync();
                stopwatch.Stop();
            }
            catch
            {
                stopwatch.Stop();
            }
        }

        [StepDefinition(@"выполнено двойное нажатие на элемент \""(.+)\"" на веб-странице")]
        public async Task DoubleClickToWebElement(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is DefaultClick).Should().BeTrue($"элемент \"{name}\" имеет отличный от Click профиль");
            await (element as DefaultClick)?.DoubleClickAsync();
        }

        [StepDefinition(@"выполнено нажатие с удержанием на элементе \""(.+)\"" на веб-странице")]
        public async Task ClickAndHoldToWebElementAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is DefaultClick).Should().BeTrue($"элемент \"{name}\" имеет отличный от Click профиль");
            (element as DefaultClick)?.ClickAndHold();
        }

        [StepDefinition(@"я ввожу в поле \""(.+)\"" веб-страницы значение \""(.+)\""")]
        public async Task InputValueIntoField(string name, string text)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is Input).Should().BeTrue($"элемент \"{name}\" имеет отличный от Input профиль");
            
            text.Should().NotBeNull($"значение \"text\" не задано");
            text = this.variableController.ReplaceVariables(text) ?? text;
            await (element as Input)?.SetTextAsync(text);
        }

        [StepDefinition(@"я очищаю поле \""(.+)\"" веб-страницы")]
        public async Task ClearField(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is Input).Should().BeTrue($"элемент \"{name}\" имеет отличный от Input профиль");
            await (element as Input)?.ClearAsync();
        }

        [StepDefinition(@"я создаю переменную \""(.+)\"" с текстом из элемента \""(.+)\"" на веб-странице")]
        public async Task SetVariableValueOfElementTextAsync(string varName, string name)
        {
            this.variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            this.variableController.SetVariable(varName, element.Text.GetType(), await element.Text);
        }

        [StepDefinition(@"я создаю переменную \""(.+)\"" со значением из элемента \""(.+)\"" на веб-странице")]
        public async Task SetVariableValueOfElementValueAsync(string varName, string name)
        {
            this.variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");

            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            this.variableController.SetVariable(varName, element.Value.GetType(), await element.Value);
        }

        [StepDefinition(@"я сохраняю значение атрибута \""(.+)\"" элемента \""(.+)\"" веб-страницы в переменную \""(.+)\""")]
        public async Task StoreWebElementValueOfAttributeInVariableAsync(string attribute, string name, string varName)
        {
            this.variableController.Variables.Should().NotContainKey(varName, $"переменная \"{varName}\" уже существует");
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var attributeValue = await element.GetAttributeAsync(attribute);
            this.variableController.SetVariable(varName, attributeValue.GetType(), attributeValue);
        }

        [StepDefinition(@"загружен файл из переменной \""(.+)\"" в элемент \""(.+)\"" на веб-странице")]
        public async Task LoadFileToElementAsync(string varName, string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);

            this.variableController.Variables.Should().ContainKey(varName, $"переменная \"{varName}\" не существует");
            var path = this.variableController.GetVariableValueText(varName);
            path.Should().NotBeNull($"путь к файлу \"{varName}\" пустой");
            (element as File)?.SetTextAsync(path);
        }

        [StepDefinition(@"нажата клавиша \""(.+)\"" на элементе \""(.+)\"" на веб-странице")]
        public async Task PressKeyToWebElementAsync(string key, string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            await element.PressKeysAsync(key);
        }

        [Then(@"на веб-странице значение элемента \""(.+)\"" пусто")]
        [Then(@"на веб-странице значение элемента \""(.+)\"" пустая строка")]
        [Then(@"на веб-странице значение элемента \""(.+)\"" равно пустой строке")]
        [Then(@"на веб-странице значение элемента \""(.+)\"" равно null")]
        [Then(@"на веб-странице значение элемента \""(.+)\"" заполнено пробелами")]
        public async Task WebElementValueIsEmptyAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Value;
            elementValue.ToString().Should().BeNullOrWhiteSpace($"значение элемента \"{name}\" не пустое");
        }

        [Then(@"на веб-странице текст элемента \""(.+)\"" пустой")]
        [Then(@"на веб-странице текст элемента \""(.+)\"" пустая строка")]
        [Then(@"на веб-странице текст элемента \""(.+)\"" равен пустой строке")]
        [Then(@"на веб-странице текст элемента \""(.+)\"" равен null")]
        [Then(@"на веб-странице текст элемента \""(.+)\"" заполнен пробелами")]
        public async Task WebElementTextIsEmptyAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Text;
            elementValue.Should().BeNullOrWhiteSpace($"текст элемента \"{name}\" не пустой");
        }

        [Then(@"на веб-странице значение элемента \""(.+)\"" заполнено")]
        [Then(@"на веб-странице значение элемента \""(.+)\"" не равно null")]
        [Then(@"на веб-странице значение элемента \""(.+)\"" содержит символы, отличные от пробелов")]
        public async Task WebElementValueIsNotEmptyAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Value;
            elementValue.ToString().Should().NotBeNullOrWhiteSpace($"значение элемента \"{name}\" пусто или не существует");
        }

        [Then(@"на веб-странице текст элемента \""(.+)\"" заполнен")]
        [Then(@"на веб-странице текст элемента \""(.+)\"" не равно null")]
        [Then(@"на веб-странице текст элемента \""(.+)\"" содержит символы, отличные от пробелов")]
        public async Task WebElementTextIsNotEmptyAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Text;
            elementValue.Should().NotBeNullOrWhiteSpace($"текст элемента \"{name}\" пустой или не существует");
        }

        #endregion
        
        #region Проверка на Contains и Equal со значением и переменной для текста и значения элемента
        [Then(@"на веб-странице значение элемента \""(.+)\"" содержит значение \""(.+)\""")]
        public async Task WebElementValueContainsValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Value;
            elementValue.ToString().Should().NotBeNullOrWhiteSpace($"значение элемента \"{name}\" пусто или не существует");
            elementValue.ToString().Should().Contain(expected, $"значение элемента \"{name}\":\"{element.Value}\" не содержит \"{expected}\"");
        }

        [Then(@"на веб-странице текст элемента \""(.+)\"" содержит значение \""(.+)\""")]
        public async Task WebElementTextContainsValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementText = await element.Text;
            elementText.Should().NotBeNullOrWhiteSpace($"текст у элемента \"{name}\" пустой или не существует");
        
            elementText.Should().Contain(expected, $"текст элемента \"{name}\":\"{element.Text}\" не содержит \"{expected}\"");
        }

        [Then(@"на веб-странице значение элемента \""(.+)\"" не содержит значение \""(.+)\""")]
        public async Task WebElementValueNotContainsValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Value;
            elementValue.ToString().Should().NotBeNullOrWhiteSpace($"эначение элемента \"{name}\" пусто или не существует");
        
            elementValue.ToString().Should().NotContain(expected, $"значение элемента \"{name}\":\"{element.Value}\" содержит \"{expected}\"");
        }

        [Then(@"на веб-странице текст элемента \""(.+)\"" не содержит значение \""(.+)\""")]
        public async Task WebElementTextNotContainsValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementText = await element.Text;
            elementText.Should().NotBeNullOrWhiteSpace($"эначение элемента \"{name}\" пусто или не существует");
        
            elementText.Should().NotContain(expected, $"текст у элемента \"{name}\":\"{element.Text}\" содержит \"{expected}\"");
        }

        [Then(@"на веб-странице значение элемента \""(.+)\"" равно значению \""(.+)\""")]
        public async Task WebElementValueEqualValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Value;
            elementValue.ToString().Should().NotBeNullOrWhiteSpace($"эначение элемента \"{name}\" пусто или не существует");
        
            elementValue.ToString().Should().Be(expected, $"значение элемента \"{name}\":\"{element.Value}\" не равно \"{expected}\"");
        }

        [Then(@"на веб-странице текст элемента \""(.+)\"" равен значению \""(.+)\""")]
        public async Task WebElementTextEqualValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementText = await element.Text;
            elementText.Should().NotBeNullOrWhiteSpace($"эначение элемента \"{name}\" пусто или не существует");
        
            elementText.Should()
                .Be(expected, $"текст у элемента \"{name}\":\"{element.Text}\" не равен \"{expected}\"");
        }

        [Then(@"на веб-странице значение элемента \""(.+)\"" не равно значению \""(.+)\""")]
        public async Task WebElementValueNotEqualValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementValue = await element.Value;
            elementValue.ToString().Should().NotBeNullOrWhiteSpace($"эначение элемента \"{name}\" пусто или не существует");
        
            elementValue.ToString().Should()
                .NotBe(expected, $"значение элемента \"{name}\":\"{element.Value}\" равно \"{expected}\"");
        }

        [Then(@"на веб-странице текст элемента \""(.+)\"" не равен значению \""(.+)\""")]
        public async Task WebElementTextNotEqualValueAsync(string name, string expected)
        {
            expected.Should().NotBeNull($"значение \"expected\" не задано");
            expected = this.variableController.ReplaceVariables(expected) ?? expected;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var elementText = await element.Text;
            elementText.Should().NotBeNullOrWhiteSpace($"эначение элемента \"{name}\" пусто или не существует");
        
            elementText.Should()
                .NotBe(expected, $"текст у элемента \"{name}\":\"{element.Text}\" равен \"{expected}\"");
        }

        #endregion
        
        #region Проверка свойств элемента на отображение, активность и редактируемость
        [Then(@"элемент \""(.+)\"" отображается на веб-странице")]
        public async Task WebElementIsDisplayedAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isDispalyed = await element.Displayed;
            isDispalyed.Should().BeTrue($"элемент \"{name}\" не отображается");
        }
        
        [Then(@"элемент \""(.+)\"" не отображается на веб-странице")]
        public async Task WebElementIsNotDisplayedAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isDispalyed = await element.NotDisplayed;
            isDispalyed.Should().BeTrue($"элемент \"{name}\" отображается");
        }
        
        [Then(@"на веб-странице элемент \""(.+)\"" активен")]
        public async Task WebElementIsEnabledAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isEnabled = await element.Enabled;
            isEnabled.Should().BeTrue($"элемент \"{name}\" не активен");
        }
        
        [Then(@"на веб-странице элемент \""(.+)\"" неактивен")]
        public async Task WebElementIsDisabledAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isDisabled = await element.Disabled;
            isDisabled.Should().BeTrue($"элемент \"{name}\" активен");
        }
        
        [Then(@"на веб-странице элемент \""(.+)\"" выбран")]
        public async Task WebElementIsSelectedAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isSelected = await element.Selected;
            isSelected.Should().BeTrue($"элемент \"{name}\" не выбран");
        }
        
        [Then(@"на веб-странице элемент \""(.+)\"" не выбран")]
        public async Task WebElementIsNotSelectedAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isNotSelected = await element.NotSelected;
            isNotSelected.Should().BeTrue($"элемент \"{name}\" выбран");
        }
        
        [Then(@"на веб-странице элемент \""(.+)\"" нельзя редактировать")]
        public async Task WebElementIsNotEditableAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isNotEditable = await element.NotEditable;
            isNotEditable.Should().BeTrue($"элемент \"{name}\" доступен для редактирования");
        }
        
        [Then(@"на веб-странице элемент \""(.+)\"" можно редактировать")]
        public async Task WebElementIsEditableAsync(string name)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            var isEditable = await element.Editabled;
            isEditable.Should().BeTrue($"элемент \"{name}\" не доступен для редактирования");
        }
        
        #endregion
        
        #region Работа с Dropdown 
        
        [StepDefinition(@"я выбираю в поле \""(.+)\"" веб-страницы значение \""(.+)\""")]
        public async Task DropdownIntoValueAsync(string name, string value)
        {
            value.Should().NotBeNull($"значение \"expected\" не задано");
            value = this.variableController.ReplaceVariables(value) ?? value;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is Dropdown).Should().BeTrue($"элемент \"{name}\" имеет отличный от Dropdown профиль");
            (element as Dropdown)?.SelectByValue(value);
        }

        [StepDefinition(@"я выбираю в поле \""(.+)\"" веб-страницы текст \""(.+)\""")]
        public async Task DropdownIntoTextAsync(string name, string text)
        {
            text.Should().NotBeNull($"значение \"expected\" не задано");
            text = this.variableController.ReplaceVariables(text) ?? text;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is Dropdown).Should().BeTrue($"элемент \"{name}\" имеет отличный от Dropdown профиль");
            (element as Dropdown)?.SelectByText(text);
        }

        [StepDefinition(@"я выбираю в поле \""(.+)\"" веб-страницы номер значения \""(.+)\""")]
        public async Task DropdownIntoIndexAsync(string name, int index)
        {
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is Dropdown).Should().BeTrue($"элемент \"{name}\" имеет отличный от Dropdown профиль");
            (element as Dropdown)?.SelectByIndex(index);
        }
        
        #endregion
        
        #region Blocks
        [StepDefinition(@"я перехожу на блок \""(.+)\"" на веб-странице")]
        public async Task GoToBlockAsync(string block)
        {
            await BrowserController.GetBrowser().GetCurrentPage().GetBlockAsync(block);
        }
        
        [StepDefinition(@"я возвращаюсь к основной веб-странице")]
        public void BackToPage()
        {
            BrowserController.GetBrowser().GetCurrentPage().BackToPage();
        }
        #endregion
        
        #region Frames
        [StepDefinition(@"я перехожу на фрейм \""(.+)\"" на веб-странице")]
        public void GoToFrame(string frame)
        {
            BrowserController.GetBrowser().GetCurrentPage().GetFrame(frame);
        }
        
        [StepDefinition(@"я перехожу на стандартный фрейм на веб-странице")]
        public async Task GetDefaultFrameAsync()
        {
            await BrowserController.GetBrowser().GetCurrentPage().GetDefaultFrameAsync();
        }
        #endregion

        #region File
        
        [StepDefinition(@"я загружаю в элемент \""(.+)\"" веб-страницы файл \""(.+)\""")]
        public async Task UploadFileIntoFieldAsync(string name, string fullpath)
        {
            fullpath = variableController.ReplaceVariables(fullpath) ?? fullpath;
            
            var element = await BrowserController.GetBrowser().GetCurrentPage().GetElementAsync(name);
            (element is File).Should().BeTrue($"элемент \"{name}\" имеет отличный от File профиль");
            
            (element as File)?.SetTextAsync(fullpath);
        }
        
        #endregion
    }
}