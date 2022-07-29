using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROB.Models
{
    internal class PriceChartingPriceResponse
    {
        public string Url { get; set; }
        public List<PriceChartingPrice> Prices { get; set; }
    }
}
