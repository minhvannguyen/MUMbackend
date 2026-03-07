namespace MUMbackend.ToxicModelTrainer
{
    using Microsoft.ML;
    using Microsoft.ML.Data;

    public class TrainToxicModel
    {
        public static void Train()
        {
            var mlContext = new MLContext();

            var data = mlContext.Data.LoadFromTextFile<ToxicData>(
                "toxic-comments.csv",
                hasHeader: true,
                separatorChar: ',');

            var pipeline =
                mlContext.Transforms.Text.FeaturizeText("Features", nameof(ToxicData.Text))
                .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            var model = pipeline.Fit(data);

            mlContext.Model.Save(model, data.Schema, "toxic-model.zip");

            Console.WriteLine("Model trained successfully!");
        }
    }

    public class ToxicData
    {
        [LoadColumn(0)]
        public string Text { get; set; } = "";

        [LoadColumn(1)]
        public bool Label { get; set; }
    }
}
