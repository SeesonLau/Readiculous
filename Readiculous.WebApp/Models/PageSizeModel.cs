using System.Collections.Generic;

namespace Readiculous.WebApp.Models
{
    public class PageSizeModel
    {
        public int CurrentPageSize { get; set; } = 10;
        public List<int> AvailablePageSizes { get; set; } = new List<int> { 10, 25, 50, 100 };
    }
}
