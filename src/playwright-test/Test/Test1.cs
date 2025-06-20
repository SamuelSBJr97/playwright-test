using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using System.Threading.Tasks;
using System.Linq;

namespace Test
{
    [TestClass]
    public class PageTagTests
    {
        private static IBrowser _browser;
        private static IPage _page;
        private static IPlaywright _playwright;

        // Lista de tags de formulário HTML
        private static readonly string[] FormTags = new[]
        {
            "form", "input", "select", "textarea", "button", "label", "fieldset", "legend", "datalist", "output", "optgroup", "option"
        };

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var page = await _browser.NewPageAsync();
            // Substitua pela URL da página a ser testada
            await page.GotoAsync("https://sua-url.com");
            _page = page;
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [DataTestMethod]
        [DynamicData(nameof(GetFormTags), DynamicDataSourceType.Method)]
        public async Task TagDeFormulario_Deve_Estar_Visivel(string tag)
        {
            var elements = await _page.QuerySelectorAllAsync(tag);
            Assert.IsTrue(elements.Any(), $"Tag <{tag}> não encontrada na página.");

            foreach (var element in elements)
            {
                bool isVisible = await element.IsVisibleAsync();
                Assert.IsTrue(isVisible, $"Elemento <{tag}> não está visível.");
            }
        }

        [TestMethod]
        public async Task Deve_Existir_Opcao_De_Salvar()
        {
            // Busca por botões ou inputs do tipo submit ou com texto relacionado a salvar
            var saveButtons = await _page.QuerySelectorAllAsync("button, input[type=submit], input[type=button]");
            bool found = false;

            foreach (var btn in saveButtons)
            {
                var type = await btn.GetAttributeAsync("type");
                var text = (await btn.InnerTextAsync()).ToLowerInvariant();

                if ((type == "submit" || type == "button") ||
                    text.Contains("salvar") || text.Contains("save") || text.Contains("gravar"))
                {
                    bool isVisible = await btn.IsVisibleAsync();
                    if (isVisible)
                    {
                        found = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(found, "Nenhuma opção de salvar (submit/button) visível encontrada na página.");
        }

        public static System.Collections.Generic.IEnumerable<object[]> GetFormTags()
        {
            foreach (var tag in FormTags)
            {
                yield return new object[] { tag };
            }
        }
    }
}