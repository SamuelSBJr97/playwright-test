using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;

namespace PlaywrightTests;

[TestClass]
public class ExampleTest : PageTest
{
    [TestMethod]
    public async Task HasTitle()
    {
        await Page.GotoAsync("https://playwright.dev");

        // Expect a title "to contain" a substring.
        await Expect(Page).ToHaveTitleAsync(new Regex("Playwright"));
    }

    [TestMethod]
    public async Task MyTest()
    {
        await Page.GotoAsync("https://playwright.dev/");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "GitHub repository" })).ToBeVisibleAsync();
        var page1 = await Page.RunAndWaitForPopupAsync(async () =>
        {
            await Page.GetByRole(AriaRole.Link, new() { Name = "GitHub repository" }).ClickAsync();
        });
        await Page.GotoAsync("https://github.com/microsoft/playwright");
    }
}