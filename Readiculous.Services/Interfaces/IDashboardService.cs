using Readiculous.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Readiculous.Services.Interfaces
{
    public interface IDashboardService
    {
        UserDashboardViewModel GetUserDashboardViewModel(string userId);
    }
}
