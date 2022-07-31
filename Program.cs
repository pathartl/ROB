using BetterConsoleTables;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using ROB;
using ROB.Data;
using ROB.Models;
using System.Globalization;
using System.Web;

public class Program
{
    private DiscordSocketClient DiscordClient;
    private PriceChartingClient PriceChartingClient;
    private IConfiguration Configuration;

    private DatabaseContext Context;

    Dictionary<string, string[]> SystemShortnames = new Dictionary<string, string[]>()
    {
        { "nes", new string[] { "Nintendo Entertainment System", "nes", "nintendo-entertainment-system" } },
        { "super-nintendo", new string[] { "Super Nintendo", "snes", "super-nintendo" } },
        { "nintendo-64", new string[] { "Nintendo 64", "n64", "nintendo-64" } },
        { "gamecube", new string[] { "Nintendo Gamecube", "gamecube", "ngc", "gc" } },
        { "wii", new string[] { "Nintendo Wii", "wii", "rvl" } },
        { "wii-u", new string[] { "Nintendo Wii U", "wiiu", "wii-u" } },
        { "nintendo-switch", new string[] { "Nintendo Switch", "switch", "nx", "nintendo-switch" } },
        { "gameboy", new string[] { "Nintendo GameBoy", "gameboy", "gb" } },
        { "gameboy-color", new string[] { "Nintendo GameBoy Color", "gameboy-color", "gbc" } },
        { "gameboy-advance", new string[] { "Nintendo GameBoy Advance", "gba", "gameboy-advance" } },
        { "nintendo-ds", new string[] { "Nintendo DS", "nds", "ds" } },
        { "nintendo-3ds", new string[] { "Nintendo 3DS", "3ds" } },
        { "virtual-boy", new string[] { "Nintendo Virtual Boy", "virtual-boy", "vb" } },
        { "game-&-watch", new string[] { "Nintendo Game & Watch", "gw", "game-&-watch", "game-and-watch" } },
        { "playstation", new string[] { "Sony PlayStation", "ps1", "psx" } },
        { "playstation-2", new string[] { "Sony PlayStation 2", "ps2" } },
        { "playstation-3", new string[] { "Sony PlayStation 3", "ps3" } },
        { "playstation-4", new string[] { "Sony PlayStation 4", "ps4" } },
        { "playstation-5", new string[] { "Sony PlayStation 5", "ps5" } },
        { "psp", new string[] { "Sony PSP", "psp" } },
        { "playstation-vita", new string[] { "Sony PlayStation Vita", "vita", "psvita" } },
        { "sega-master-system", new string[] { "Sega Master System", "sms", "master-system" } },
        { "sega-genesis", new string[] { "Sega Genesis", "gen", "genesis" } },
        { "sega-cd", new string[] { "Sega CD", "segacd" } },
        { "sega-32x", new string[] { "Sega 32X", "sega32x", "32x", "sega-32x" } },
        { "sega-dreamcast", new string[] { "Sega Dreamcast", "dc", "dreamcast" } },
        { "sega-saturn", new string[] { "Sega Saturn", "saturn" } },
        { "sega-game-gear", new string[] { "Sega Game Gear", "gg", "gamegear", "game-gear" } },
        { "xbox", new string[] { "Microsoft Xbox", "xbox" } },
        { "xbox-360", new string[] { "Microsoft Xbox 360", "xbox-360", "360" } },
        { "xbox-one", new string[] { "Microsoft Xbox One", "xbox-one", "xbone" } },
        { "xbox-series-x", new string[] { "Microsoft Xbox Series X", "series-x", "xboxsx" } },
        { "neo-geo", new string[] { "SNK Neo Geo MVS", "mvs", "neogeo" } },
        { "neo-geo-aes", new string[] { "SNK Neo Geo AES", "aes" } },
        { "neo-geo-cd", new string[] { "SNK Neo Geo CD", "neogeo-cd" } },
        { "neo-geo-pocket-color", new string[] { "SNK Neo Geo Pocket Color", "ngpc" } },
        { "atari-2600", new string[] { "Atari 2600", "2600", "atari" } },
        { "atari-5200", new string[] { "Atari 5200", "5200" } },
        { "atari-7800", new string[] { "Atari 7800", "7800" } },
        { "atari-400", new string[] { "Atari 400", "atari400", "atari-400" } },
        { "atari-lynx", new string[] { "Atari Lynx", "lynx" } },
        { "atari-jaguar", new string[] { "Atari Jaguar", "jag", "jaguar" } },
        { "intellivision", new string[] { "Mattel Intellivision", "intellivision" } },
        { "3do", new string[] { "3DO", "3do" } },
        { "amiga", new string[] { "Commodore Amiga", "amiga" } },
        { "commodore-64", new string[] { "Commodore 64", "c64" } },
        { "amiibo", new string[] { "Nintendo Amiibo", "amiibo" } },
        { "cd-i", new string[] { "Philips CD-i", "cdi" } },
        { "colecovision", new string[] { "ColecoVision", "coleco" } },
        { "magnavox-odyssey", new string[] { "Magnavox Odyssey", "odyssey" } },
        { "magnavox-odyssey-2", new string[] { "Magnavox Odyssey 2", "odyssey2" } },
        { "ngage", new string[] { "Nokia N-Gage", "ngage" } },
        { "pc-games", new string[] { "PC Games", "pc" } },
        { "turbografx-cd", new string[] { "NEC TurboGrafx", "turbografx-cd" } },
        { "turbografx-16", new string[] { "NEC TurboGrafx-16", "turbografx" } },
        { "vectrex", new string[] { "Vectrex", "vectrex" } },
        { "wonderswan", new string[] { "Bandai WonderSwan", "swan", "wonderswan" } },
        { "wonderswan-color", new string[] { "Bandai WonderSwan Color", "swanc", "wonderswancolor" } },
    };

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

        DiscordClient = new DiscordSocketClient();
        DiscordClient.Log += Log;

        PriceChartingClient = new PriceChartingClient();

        var token = Configuration["Discord:Token"];

        Context = new DatabaseContext(Configuration.GetConnectionString("Database"));
        Context.Init();

        await DiscordClient.LoginAsync(TokenType.Bot, token);
        await DiscordClient.StartAsync();

        DiscordClient.MessageReceived += HandleMessageAsync;

        await Task.Delay(-1);
    }

    private async Task HandleMessageAsync(SocketMessage message)
    {
        var userMessage = message as SocketUserMessage;

        if (userMessage == null)
            return;

        if (userMessage.Author.IsBot || userMessage.Author.IsWebhook)
            return;

        var splitMessage = userMessage.CleanContent.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (splitMessage.FirstOrDefault() != null && splitMessage.First().StartsWith("!"))
        {
            var command = splitMessage.First();

            switch (command.ToLower())
            {
                case "!addquote":
                    await AddQuote(userMessage);
                    break;

                case "!price":
                    await CheckPrice(userMessage);
                    break;

                case "!guessname":
                    await GuessName(userMessage);
                    break;

                default:
                    await GetQuote(command.TrimStart('!'), userMessage);
                    break;
            }
        }
    }

    private async Task AddQuote(SocketUserMessage message)
    {
        var splitMessage = message.CleanContent.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        Context.AddQuote(splitMessage.Skip(1).First().ToLower(), String.Join(' ', splitMessage.Skip(2)), message.Author.Id.ToString());

        await message.ReplyAsync("Quote added!");
    }

    private async Task GetQuote(string command, SocketUserMessage message)
    {
        var quote = Context.GetRandomQuote(command);

        await message.Channel.SendMessageAsync($"**{quote}**");
    }

    private async Task GuessName(SocketUserMessage message)
    {
        // How much over/under we can be on the word in the title
        var wordSizeSlop = 1;

        try
        {
            var splitMessage = message.CleanContent.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            var potentialNames = new List<string>();

            var games = await PriceChartingClient.GetAllGames(GetSystemFromShortname(splitMessage[1]));
            var wordLengths = splitMessage[2].Split('-').Select(wl => Int32.Parse(wl)).ToList();

            // The count - 1 at the end is for number of spaces
            var maxLengthHigh = wordLengths.Select(wl => wl + wordSizeSlop).Sum() + (wordLengths.Count - 1);
            var maxLengthLow = wordLengths.Where(wl => wl > wordSizeSlop).Select(wl => wl - 2).Sum() + wordLengths.Where(wl => wl <= wordSizeSlop).Sum();

            foreach (var game in games)
            {
                // First skip out if we're not in the ball park at all
                if (game.Title.Length < maxLengthLow || game.Title.Length > maxLengthHigh)
                    continue;

                var valid = true;

                // Split the title, see if it fits our shit
                var words = game.Title.Split(' ');

                // Stupid basic, doesn't match the number of words
                if (words.Length != wordLengths.Count)
                    continue;

                for (int i = 0; i < words.Length; i++)
                {
                    var high = wordLengths[i] + wordSizeSlop;
                    var low = words[i].Length < wordSizeSlop ? 1 : wordLengths[i] - wordSizeSlop;

                    if (words[i].Length > high || words[i].Length < low)
                        valid = false;
                }

                if (valid)
                    potentialNames.Add(game.Title);
            }

            var embed = new EmbedBuilder()
                .WithTitle($"I found {potentialNames.Count} potential match{(potentialNames.Count == 1 ? "" : "es")}")
                .WithDescription($"```{String.Join("\n", potentialNames)}```")
                .WithFooter(footer => footer.Text = "Generated with info from VGPC")
                .WithCurrentTimestamp();

            await message.ReplyAsync(embed: embed.Build());
        }
        catch (Exception e)
        {
            await message.ReplyAsync("Are you stupid or am I broken? Syntax is `!guessname ps1 6-1-2-6`");
        }
    }

    private async Task CheckPrice(SocketUserMessage message)
    {
        try
        {
            var splitMessage = message.CleanContent.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var title = "";

            string system = "";

            if (splitMessage.Length >= 3)
            {
                var potentialSystem = splitMessage[1];

                foreach (var systemShortnames in SystemShortnames)
                {
                    if (systemShortnames.Value.Contains(potentialSystem))
                        system = systemShortnames.Key;
                }

                title = String.Join(' ', splitMessage.Skip(2));
            }
            else
            {
                title = String.Join(' ', splitMessage.Skip(1));
            }

            PriceChartingPriceResponse response;

            if (!String.IsNullOrWhiteSpace(system))
            {
                response = await PriceChartingClient.GetGamePrices(system, title);
            }
            else
            {
                response = await PriceChartingClient.GetSearchPrices(title);
            }

            if (response.Prices.Count > 1)
            {
                await ReplyWithTable(message, response);
            }
            else
            {
                await ReplyWithEmbed(message, response);
            }

        }
        catch (Exception ex)
        {
            await message.Channel.SendMessageAsync("Sorry, I couldn't find that game. Try again!");
        }
    }

    private string GetSystemFromShortname(string input)
    {
        string system = "";

        foreach (var systemShortnames in SystemShortnames)
        {
            if (systemShortnames.Value.Contains(input))
                system = systemShortnames.Key;
        }

        return system;
    }

    private async Task ReplyWithTable(SocketUserMessage message, PriceChartingPriceResponse response)
    {
        var replyId = Guid.NewGuid().ToString();

        response.Prices = response.Prices.Take(10).ToList();

        string reply = GenerateTableReplyText(response);

        var embed = new EmbedBuilder()
            .WithTitle($"{response.Prices.Count} Results Found")
            .WithFooter(footer => footer.Text = "Generated with prices from VGPC")
            .WithCurrentTimestamp()
            .WithImageUrl($"attachment://{replyId}.png")
            .WithUrl(response.Url);

        using (var ms = new MemoryStream())
        {
            Painter.PaintTextImage(reply, ms);

            await message.Channel.SendFileAsync(new FileAttachment(ms, $"{replyId}.png"), embed: embed.Build());
        }
    }

    private async Task ReplyWithEmbed(SocketUserMessage message, PriceChartingPriceResponse response)
    {
        var prices = response.Prices.First();

        var embed = new EmbedBuilder()
            .WithTitle(prices.Title)
            .AddField("Loose", prices.LoosePrice.ToString("C"))
            .AddField("Complete", prices.CompletePrice.ToString("C"))
            .AddField("New", prices.NewPrice.ToString("C"))
            .WithFooter(footer => footer.Text = "Generated with prices from VGPC")
            .WithCurrentTimestamp()
            .WithThumbnailUrl(prices.ImageUrl)
            .WithUrl(response.Url);

        await message.ReplyAsync(embed: embed.Build());
    }

    private string GenerateTableReplyText(PriceChartingPriceResponse response)
    {
        var prices = response.Prices.ToList();
        var table = new Table(
            new ColumnHeader("Title"),
            new ColumnHeader("System"),
            new ColumnHeader("Loose", Alignment.Right),
            new ColumnHeader("CiB", Alignment.Right),
            new ColumnHeader("New", Alignment.Right));

        table.Config = TableConfiguration.Unicode();

        foreach (var price in prices)
        {
            table.AddRow(HttpUtility.HtmlDecode(price.Title), HttpUtility.HtmlDecode(price.System), price.LoosePrice.ToString("C"), price.CompletePrice.ToString("C"), price.NewPrice.ToString("C"));
        }

        return $"{table}";
    }

    private Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());

        return Task.CompletedTask;
    }
}