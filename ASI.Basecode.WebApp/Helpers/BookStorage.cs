using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ASI.Basecode.WebApp.Models;

namespace ASI.Basecode.WebApp
{
    public static class BookStorage
    {
        private static readonly string filePath = Path.Combine("App_Data", "books.json");

        public static List<BookViewModel> LoadBooks()
        {
            if (!File.Exists(filePath))
                return new List<BookViewModel>();

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<BookViewModel>>(json) ?? new List<BookViewModel>();
        }

        public static void SaveBooks(List<BookViewModel> books)
        {
            string json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}
