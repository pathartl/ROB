using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROB.Models
{
    internal class PriceChartingPrice
    {
        public string Title { get; set; }
        public string System { get; set; }
        public string ImageUrl { get; set; }
        public decimal LoosePrice { get; set; }
        public decimal CompletePrice { get; set; }
        public decimal NewPrice { get; set; }
        // public decimal GradedPrice { get; set; }
        // public decimal BoxOnlyPrice { get; set; }
        // public decimal ManualOnlyPrice { get; set; }

        public object AsTableFriendly()
        {
            return new
            {
                Title = Title,
                System = System,
                Loose = LoosePrice.ToString("C"),
                CIB = CompletePrice.ToString("C"),
                New = NewPrice.ToString("C"),
            };
        }
    }
}
