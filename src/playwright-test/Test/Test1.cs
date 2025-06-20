using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace PlaywrightTests;

public class ElementoInput
{
    [LoadColumn(0)]
    public string Tag { get; set; }
    [LoadColumn(1)]
    public string Role { get; set; }
    [LoadColumn(2)]
    public string Acao { get; set; }
    [LoadColumn(3)]
    public string Type { get; set; }
    [LoadColumn(4)]
    public string Text { get; set; }
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
    private static MLContext _mlContext = new();
    private static ITransformer? _model;
    private static string _datasetPath = "treino_interativo.csv";

    [ClassInitialize]
    public static void InicializarModelo(TestContext context)
    {
        if (!File.Exists(_datasetPath))
        {
            File.WriteAllText(_datasetPath, "Tag,Role,Acao,Type,Text\n"); // Cabeçalho
        }
        TreinarModelo();
    }

    private static void TreinarModelo()
    {
        var data = _mlContext.Data.LoadFromTextFile<ElementoInput>(
            _datasetPath, hasHeader: true, separatorChar: ',');

        var pipeline = _mlContext.Transforms.Conversion
            .MapValueToKey("Label", nameof(ElementoInput.Acao))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Tag"))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Role"))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Type"))
            .Append(_mlContext.Transforms.Categorical.OneHotEncoding("Text"))
            .Append(_mlContext.Transforms.Concatenate("Features", "Tag", "Role", "Type", "Text"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(data);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<ElementoInput, ElementoPrediction>(_model);
    }

    private static void AdicionarExemploTreino(ElementoInput exemplo)
    {
        // Adiciona o exemplo ao CSV
        var linha = $"{exemplo.Tag},{exemplo.Role},{exemplo.Acao},{exemplo.Type},{exemplo.Text}";
        File.AppendAllText(_datasetPath, linha + Environment.NewLine);

        // Re-treina o modelo
        TreinarModelo();
    }

    private string DecidirAcaoML(string tag, string role, string type, string text)
    {
        if (_predictionEngine == null)
            return "ignore";
        var input = new ElementoInput { Tag = tag, Role = role, Type = type, Text = text };
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
            var type = await elemento.GetAttributeAsync("type") ?? "";
            var text = await elemento.InnerTextAsync() ?? "";

            // Predição do modelo
            var acao = DecidirAcaoML(tag, role, type, text);

            // Exemplo: callback para registrar ação real do usuário
            Console.WriteLine($"Elemento: <{tag}> role='{role}' type='{type}' text='{text}'");
            Console.Write("Qual ação você executou (click/type/check/uncheck/select/ignore)? ");
            var acaoUsuario = Console.ReadLine()?.Trim().ToLower() ?? "ignore";

            // Adiciona exemplo e re-treina
            var exemplo = new ElementoInput
            {
                Tag = tag,
                Role = role,
                Acao = acaoUsuario,
                Type = type,
                Text = text
            };
            AdicionarExemploTreino(exemplo);

            // Executa ação prevista (opcional)
            switch (acao)
            {
                case "click":
                    try { await elemento.ClickAsync(); } catch { }
                    break;
                case "type":
                    try { await elemento.TypeAsync("Teste ML"); } catch { }
                    break;
                case "check":
                    try { await elemento.CheckAsync(); } catch { }
                    break;
                case "uncheck":
                    try { await elemento.UncheckAsync(); } catch { }
                    break;
                case "select":
                    try
                    {
                        var options = await elemento.QuerySelectorAllAsync("option");
                        if (options.Count > 0)
                        {
                            var value = await options[0].GetAttributeAsync("value");
                            if (value != null)
                                await elemento.SelectOptionAsync(value);
                        }
                    }
                    catch { }
                    break;
                default:
                    break;
            }
        }
    }
}