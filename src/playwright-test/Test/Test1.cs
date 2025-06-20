using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace PlaywrightTests;

public class ElementoInput
{
    public string Tag { get; set; }
    public string Role { get; set; }
}

public class ElementoPrediction
{
    [ColumnName("PredictedLabel")]
    public string Acao { get; set; }
}

[TestClass]
public class MLWebExplorerTest : PageTest
{
    private static PredictionEngine<ElementoInput, ElementoPrediction>? _predictionEngine;

    [ClassInitialize]
    public static void InicializarModelo(TestContext context)
    {
        var mlContext = new MLContext();
        var model = mlContext.Model.Load("modelo-mlnet.zip", out _);
        _predictionEngine = mlContext.Model.CreatePredictionEngine<ElementoInput, ElementoPrediction>(model);
    }

    private string DecidirAcaoML(string tag, string role)
    {
        if (_predictionEngine == null)
            return "ignore";
        var input = new ElementoInput { Tag = tag, Role = role };
        var prediction = _predictionEngine.Predict(input);
        return prediction.Acao;
    }

    [TestMethod]
    public async Task ExplorarInterfaceWeb()
    {
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = false
        };
        await using var browser = await Playwright.Chromium.LaunchAsync(launchOptions);
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync("https://playwright.dev");

        var elementos = await page.QuerySelectorAllAsync("a,button,input");

        foreach (var elemento in elementos)
        {
            var tag = await elemento.EvaluateAsync<string>("el => el.tagName.toLowerCase()");
            var role = await elemento.GetAttributeAsync("role") ?? "";

            var acao = DecidirAcaoML(tag, role);

            switch (acao)
            {
                case "click":
                    try
                    {
                        await elemento.ClickAsync();
                        Console.WriteLine($"Clicou em: <{tag}> com role '{role}'");
                    }
                    catch { }
                    break;
                case "type":
                    try
                    {
                        await elemento.TypeAsync("Teste ML");
                        Console.WriteLine($"Digitou em: <{tag}> com role '{role}'");
                    }
                    catch { }
                    break;
                default:
                    break;
            }
        }
    }
}