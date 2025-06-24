using ASI.Basecode.WebApp.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ASI.Basecode.WebApp.Helpers
{
    public static class ReviewStorage
    {
        private static readonly string FilePath = "App_Data/reviews.json";

        public static List<ReviewModel> LoadReviews()
        {
            if (!File.Exists(FilePath))
            {
                return new List<ReviewModel>();
            }

            var json = File.ReadAllText(FilePath);
            return JsonConvert.DeserializeObject<List<ReviewModel>>(json) ?? new List<ReviewModel>();
        }

        public static void SaveReviews(List<ReviewModel> reviews)
        {
            var json = JsonConvert.SerializeObject(reviews, Formatting.Indented);
            File.WriteAllText(FilePath, json);
        }
    }
}
