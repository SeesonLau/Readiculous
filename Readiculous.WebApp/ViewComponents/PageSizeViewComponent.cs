using Microsoft.AspNetCore.Mvc;
using Readiculous.WebApp.Models;

namespace Readiculous.WebApp.ViewComponents
{
    public class PageSizeViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int currentPageSize)
        {
            var model = new PageSizeModel
            {
                CurrentPageSize = currentPageSize
            };
            return View(model);
        }
    }
}
