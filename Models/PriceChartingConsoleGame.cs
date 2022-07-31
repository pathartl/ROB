using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROB.Models
{
    internal class PriceChartingConsoleGame
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "consoleUri")]
        public string ConsoleUri { get; set; }

        [JsonProperty(PropertyName = "hasProduct")]
        public bool HasProduct { get; set; }

        [JsonProperty(PropertyName = "productName")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "productUri")]
        public string ProductUri { get; set; }
    }
}
