using Microsoft.AspNetCore.Authorization; // Add this at the top
using ASI.Basecode.WebApp.Mvc;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Home Controller
    /// </summary>
    [AllowAnonymous] // ✅ Add this attribute to allow access without login
    public class HomeController : ControllerBase<HomeController>
    {
        public HomeController(IHttpContextAccessor httpContextAccessor,
                              ILoggerFactory loggerFactory,
                              IConfiguration configuration,
                              IMapper mapper = null)
            : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
