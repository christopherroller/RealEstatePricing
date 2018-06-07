using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealEstatePricing
{
    public class Town
    {
        public string TownName { get; set; }
        public string Month {get;set;}
        public decimal TerracePrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal SemiDetatchedPrice { get; set; }
        public decimal DetachedPrice { get; set; }
        public decimal Last12MonthsAvgPercent { get; set; }
        public decimal PastYearsAvgPercent { get; set; }
    }
}
