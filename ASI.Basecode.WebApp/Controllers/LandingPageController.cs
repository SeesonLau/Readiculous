using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous]
    public class LandingPageController : Controller
    {
        public IActionResult LandingPage()
        {
            return View();
        }
    }
}
