using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Playwright;
using System.Threading.Tasks;
using System.Linq;

namespace Test
{
    [TestClass]
    public class BigTableE2ETests
    {
        private static IBrowser _browser;
        private static IPage _page;
        private static IPlaywright _playwright;

        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
            var page = await _browser.NewPageAsync();
            // Altere para a URL real da view
            await page.GotoAsync("https://localhost:5001/BigTable/Index");
            _page = page;
        }

        [ClassCleanup]
        public static async Task ClassCleanup()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [TestMethod]
        public async Task Deve_Exibir_Tabela_Com_Dados()
        {
            // Verifica se a tabela existe
            var table = await _page.QuerySelectorAsync("table");
            Assert.IsNotNull(table, "Tabela não encontrada na página.");

            // Verifica se há linhas de dados (tbody > tr)
            var rows = await _page.QuerySelectorAllAsync("tbody tr");
            Assert.IsTrue(rows.Count > 0, "Nenhuma linha de dados encontrada na tabela.");
        }

        [TestMethod]
        public async Task Deve_Paginar_Tabela()
        {
            // Verifica se existe link para próxima página
            var nextLink = await _page.QuerySelectorAsync("a:text('Próxima')");
            Assert.IsNotNull(nextLink, "Link de próxima página não encontrado.");

            // Clica e verifica se a página muda
            await nextLink.ClickAsync();
            await _page.WaitForTimeoutAsync(500);

            // Verifica se a tabela ainda está presente
            var table = await _page.QuerySelectorAsync("table");
            Assert.IsNotNull(table, "Tabela não encontrada após paginação.");
        }
    }
}