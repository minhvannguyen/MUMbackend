namespace MUMbackend.Services
{
    using Microsoft.Extensions.ML;
    using MUMbackend.Infrastructure;
    using MUMbackend.ToxicModelTrainer;

    public class ToxicCommentService
    {
        private readonly PredictionEnginePool<ToxicData, ToxicPrediction> _pool;

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

        // ✅ Constructor duy nhất (DI inject pool)
        public ToxicCommentService(PredictionEnginePool<ToxicData, ToxicPrediction> pool)
        {
            _pool = pool;
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

            // 2️⃣ Check AI model (thread-safe)
            var result = _pool.Predict(new ToxicData
            {
                Text = text
            });

            Console.WriteLine($"TEXT: '{text}'");
            Console.WriteLine($"PREDICT: {result.Prediction}");
            Console.WriteLine($"SCORE: {result.Score}");

            return result.Score > 0.75;
        }
    }
}