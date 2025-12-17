using System.Collections.Generic;
using OrgEntity = TpaSodManagement.Models.Db.Organization;

namespace TpaSodManagement.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<OrgEntity> Organizations { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public int FarmCount { get; set; }
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int SaleCount { get; set; }
    }
}

