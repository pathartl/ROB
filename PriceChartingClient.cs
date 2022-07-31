using HtmlAgilityPack;
using Newtonsoft.Json;
using ROB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ROB
{
    internal class PriceChartingClient
    {
        private string BaseUrl = "https://www.pricecharting.com/";
        private HttpClient HttpClient;

        public PriceChartingClient()
        {
            HttpClient = new HttpClient();
        }

        private Dictionary<string, IEnumerable<PriceChartingConsoleGame>> AllGamesResponseCache = new Dictionary<string, IEnumerable<PriceChartingConsoleGame>>();

        public async Task<PriceChartingPriceResponse> GetGamePrices(string system, string query)
        {
            var response = new PriceChartingPriceResponse()
            {
                Url = $"search-products?q={HttpUtility.UrlEncode(query)}&type=prices&broad-category=video-games&sort=popularity",
                Prices = new List<PriceChartingPrice>()
            };

            var html = await GetHtml(response.Url);
            var dom = new HtmlDocument();

            // Clean
            html = html.Replace("\n", " ").Replace("&nbsp;", " ");

            dom.LoadHtml(html);

            var priceTable = dom.GetElementbyId("games_table");

            // Search results, find game belonging to system
            if (priceTable != null)
            {
                var link = priceTable.SelectSingleNode($"//a[contains(@href, '{BaseUrl}game/{system}')]");

                if (link == null)
                    throw new Exception("Game not found");

                response.Url = link.GetAttributeValue("href", "");

                html = await HttpClient.GetStringAsync(response.Url);

                // Clean
                html = html.Replace("\n", " ").Replace("&nbsp;", " ");

                return ScrapeGameResponse(html, response.Url);
            }

            return ScrapeGameResponse(html, response.Url);
        }

        public async Task<PriceChartingPriceResponse> GetSearchPrices(string query)
        {
            var response = new PriceChartingPriceResponse()
            {
                Url = $"search-products?q={HttpUtility.UrlEncode(query)}&type=prices&broad-category=video-games&sort=popularity",
                Prices = new List<PriceChartingPrice>()
            };

            var html = await GetHtml(response.Url);
            var dom = new HtmlDocument();

            html = html.Replace("\n", " ").Replace("&nbsp;", " ");

            dom.LoadHtml(html);

            response.Url = $"{BaseUrl}{response.Url}";

            var priceTable = dom.GetElementbyId("games_table");

            if (priceTable != null)
            {
                var priceRows = priceTable.Descendants("tbody")
                    .First()
                    .Descendants("tr");

                foreach (var row in priceRows)
                {
                    var columns = row.Descendants("td");

                    var price = new PriceChartingPrice()
                    {
                        Title = columns.ElementAt(0).InnerText.Trim(),
                        System = columns.ElementAt(1).InnerText.Trim(),
                        LoosePrice = ParsePrice(columns.ElementAt(2).InnerText),
                        CompletePrice = ParsePrice(columns.ElementAt(3).InnerText),
                        NewPrice = ParsePrice(columns.ElementAt(4).InnerText)
                    };

                    response.Prices.Add(price);
                }

                return response;
            }

            return ScrapeGameResponse(html, response.Url);
        }

        private PriceChartingPriceResponse ScrapeGameResponse(string html, string url = "")
        {
            var dom = new HtmlDocument();
            var response = new PriceChartingPriceResponse()
            {
                Prices = new List<PriceChartingPrice>(),
                Url = url
            };

            response.Url = $"{BaseUrl}{response.Url}";

            html = html.Replace("\n", " ").Replace("&nbsp;", " ");

            dom.LoadHtml(html);

            var gamePage = dom.GetElementbyId("game-page");

            if (gamePage != null)
            {
                var priceColumns = dom.GetElementbyId("price_data").Descendants("tbody").First().Descendants("tr").First().Descendants("td");
                var heading = dom.GetElementbyId("product_name");
                var system = heading.Descendants("a").First().InnerText;
                var title = heading.InnerText.Replace(system, "");
                var image = gamePage.SelectSingleNode($"//img[contains(@itemprop, 'image')]").GetAttributeValue("src", "");

                response.Prices.Add(new PriceChartingPrice()
                {
                    Title = title.Trim(),
                    System = system.Trim(),
                    ImageUrl = image.Trim(),
                    LoosePrice = ParsePrice(priceColumns.ElementAt(0).Descendants("span").First().InnerText),
                    CompletePrice = ParsePrice(priceColumns.ElementAt(1).Descendants("span").First().InnerText),
                    NewPrice = ParsePrice(priceColumns.ElementAt(2).Descendants("span").First().InnerText),
                });

                return response;
            }

            throw new Exception("Couldn't scrape game");
        }

        public async Task<IEnumerable<PriceChartingConsoleGame>> GetAllGames(string system)
        {
            int cursor = 0;
            int lastPageCount = 50;
            int pageCount = 50;

            if (AllGamesResponseCache.ContainsKey(system))
                return AllGamesResponseCache[system];

            var games = new List<PriceChartingConsoleGame>();

            while (lastPageCount == 50)
            {
                var response = await GetJson<PriceChartingConsoleGameReponse>($"console/{system}?sort=name&cursor={cursor}&format=json");

                lastPageCount = response.Games.Count();

                games.AddRange(response.Games);
                    
                cursor += pageCount;
            }

            AllGamesResponseCache[system] = games;

            return games;
        }

        private async Task<string> GetHtml(string route)
        {
            var response = await HttpClient.GetStringAsync($"{BaseUrl}{route}");

            return response;
        }

        private async Task<T> GetJson<T>(string route)
        {
            var response = await HttpClient.GetStringAsync($"{BaseUrl}{route}");

            return JsonConvert.DeserializeObject<T>(response);
        }

        private static decimal ParsePrice(string input)
        {
            input = input.Trim();

            if (string.IsNullOrEmpty(input))
                return 0M;

            try
            {
                var price = decimal.Parse(input.TrimStart('$'));

                return price;
            }
            catch
            {
                return 0M;
            }
        }
    }
}
