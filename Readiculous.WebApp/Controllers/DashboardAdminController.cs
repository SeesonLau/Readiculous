using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Readiculous.Services.Interfaces;
using Readiculous.Services.ServiceModels;
using Readiculous.WebApp.Mvc;
using System.Collections.Generic;
using System.Linq;
using static Readiculous.Resources.Constants.Enums;

namespace Readiculous.WebApp.Controllers
{
    public class DashboardAdminController : ControllerBase<DashboardAdminController>
    {
        private readonly IDashboardService _dashboardService;

        public DashboardAdminController(IDashboardService dashboardService, IHttpContextAccessor httpContextAccessor, ILoggerFactory loggerFactory, IConfiguration configuration, IMapper mapper = null) : base(httpContextAccessor, loggerFactory, configuration, mapper)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult AdminScreen()
        {
            var adminDashboardViewModel = _dashboardService.GetAdminDashboardViewModel();
            return View(adminDashboardViewModel);
        }
    }
}
