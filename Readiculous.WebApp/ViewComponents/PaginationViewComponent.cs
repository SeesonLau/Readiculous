using Microsoft.AspNetCore.Mvc;
using Readiculous.WebApp.Models;

namespace Readiculous.WebApp.ViewComponents
{
    public class Pagination : ViewComponent
    {
        public IViewComponentResult Invoke(
            int currentPage,
            int totalPages,
            int pageSize,
            int totalItems, // This must be passed if your PaginationModel constructor requires it
            string action = null, // Make these nullable if they might not always be provided
            string controller = null,
            string routeId = null,
            string routeBookSearch = null,
            string searchString = null,
            string searchType = null,
            string sortOrder = null)
        {
            // Create the PaginationModel instance using the passed parameters
            var model = new PaginationModel(totalItems, currentPage, pageSize);

            // Pass additional route values to the _Pagination.cshtml view via ViewData
            // The _Pagination.cshtml will use these to construct correct URLs.
            ViewData["action"] = action;
            ViewData["controller"] = controller;
            ViewData["routeId"] = routeId;
            ViewData["routeBookSearch"] = routeBookSearch;
            ViewData["searchString"] = searchString;
            ViewData["searchType"] = searchType;
            ViewData["sortOrder"] = sortOrder;

            return View(model); // Pass the constructed model to the default view (_Pagination.cshtml)
        }
    }
}
