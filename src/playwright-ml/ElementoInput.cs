using System;
using Microsoft.ML;
using Microsoft.ML.Data;

public class ElementoInput
{
    [LoadColumn(0)]
    public string Tag { get; set; }
    
    [LoadColumn(1)]
    public string Role { get; set; }
    
    [LoadColumn(2)]
    public string Acao { get; set; }
}

public class ElementoPrediction
{
    [ColumnName("PredictedLabel")]
    public string Acao { get; set; }
}

class Program
{
    static void Main()
    {
        var mlContext = new MLContext();

        // Carregar dados
        var data = mlContext.Data.LoadFromTextFile<ElementoInput>(
            "treino.csv", hasHeader: true, separatorChar: ',');

        // Transformações
        var pipeline = mlContext.Transforms.Conversion
            .MapValueToKey("Label", nameof(ElementoInput.Acao))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("Tag"))
            .Append(mlContext.Transforms.Categorical.OneHotEncoding("Role"))
            .Append(mlContext.Transforms.Concatenate("Features", "Tag", "Role"))
            .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // Treinar modelo
        var model = pipeline.Fit(data);

        // Salvar modelo
        mlContext.Model.Save(model, data.Schema, "modelo-mlnet.zip");

        Console.WriteLine("Modelo treinado e salvo como modelo-mlnet.zip");
    }
}