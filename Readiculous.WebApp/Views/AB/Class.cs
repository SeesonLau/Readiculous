using Microsoft.AspNetCore.Mvc;
using Readiculous.Services.ServiceModels;
using System;
/*
public IActionResult GenreViewModal(string id, int page = 1, string bookSearch = null)
{
    var genre = _genreService.GetGenreEditById(id);
    if (genre == null)
    {
        return NotFound();
    }

    var allBooks = _genreService.GetBooksByGenreId(id);

    if (!string.IsNullOrWhiteSpace(bookSearch))
    {
        allBooks = allBooks.Where(b => b.Title != null && b.Title.ToLower().Contains(bookSearch.ToLower())).ToList();
    }

    ViewData["BookSearch"] = bookSearch;
    int pageSize = 10;
    int totalBooks = allBooks.Count;
    int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);
    page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

    var books = allBooks.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    var viewModel = new GenreBooksViewModel
    {
        Genre = genre,
        Books = books,
        CurrentPage = page,
        TotalPages = totalPages,
        PageSize = pageSize
    };

    return View(viewModel);
}*/