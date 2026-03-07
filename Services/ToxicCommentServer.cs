namespace MUMbackend.Services
{
    using Microsoft.ML;
    using MUMbackend.Infrastructure;
    using MUMbackend.ToxicModelTrainer;

    public class ToxicCommentService
    {
        private readonly PredictionEngine<ToxicData, ToxicPrediction> _engine;

        private readonly List<string> _blacklist = new()
    {
        "địt",
        "địt mẹ",
        "dm",
        "đm",
        "đụ",
        "cc",
        "cặc",
        "lồn",
        "lol",
        "vl",
        "vcl",
        "ngu",
        "óc chó",
        "súc vật"
    };

        public ToxicCommentService()
        {
            var mlContext = new MLContext();

            var modelPath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "MLModels",
    "toxic-model.zip"
);

            var model = mlContext.Model.Load(modelPath, out var modelInputSchema);

            _engine = mlContext.Model.CreatePredictionEngine<ToxicData, ToxicPrediction>(model);
        }

        public bool IsToxic(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.ToLower();

            // 1️⃣ Check blacklist trước
            foreach (var word in _blacklist)
            {
                if (text.Contains(word))
                    return true;
            }

            // 2️⃣ Check AI model
            var result = _engine.Predict(new ToxicData
            {
                Text = text
            });

            return result.Prediction;
        }
    }
}
