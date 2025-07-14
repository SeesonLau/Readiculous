using System.Collections.Generic;

namespace Readiculous.Services.ServiceModels
{
    public class GenreBooksViewModel
    {
        public GenreViewModel Genre { get; set; }
        public List<BookListItemViewModel> Books { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalBooksCount { get; set; }
        public List<GenreListItemViewModel> AllGenres { get; set; }
    }
} 