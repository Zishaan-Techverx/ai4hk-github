using System.Collections.Generic;
using FarmEntity = TpaSodManagement.Database.Entities.Farm;

namespace TpaSodManagement.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public List<FarmEntity> Farms { get; set; } = new();
        public bool IsAuthenticated { get; set; }
        public int FarmCount { get; set; }
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int SaleCount { get; set; }
    }
}

