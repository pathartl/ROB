using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROB.Models
{
    internal class PriceChartingConsoleGameReponse
    {
        [JsonProperty(PropertyName = "products")]
        public IEnumerable<PriceChartingConsoleGame> Games { get; set; }
    }
}
