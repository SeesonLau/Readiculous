using Microsoft.AspNetCore.Mvc;
using Readiculous.WebApp.Models;

namespace Readiculous.WebApp.ViewComponents
{ 
    public class PaginationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(PaginationModel model)
        {
            return View(model);
        }
    }
}
