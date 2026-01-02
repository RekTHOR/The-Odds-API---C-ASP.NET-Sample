using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

const string apiKey = "YOUR_API_KEY"; // Replace with your API key from https://the-odds-api.com

app.MapGet("/", async () =>
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("https://api.the-odds-api.com/v4/");

    var html = "<html><head><title>The Odds API - C# Sample</title>";
    html += "<style>body{font-family:Arial,sans-serif;max-width:800px;margin:40px auto;padding:20px;}";
    html += "h1{color:#333;}h2{color:#666;border-bottom:2px solid #eee;padding-bottom:10px;}";
    html += ".sport,.match{background:#f9f9f9;padding:15px;margin:10px 0;border-radius:5px;}";
    html += ".bookmaker{margin-left:20px;}.outcome{margin-left:40px;}</style></head><body>";
    html += "<h1>The Odds API - C# / ASP.NET Sample</h1>";

    try
    {
        // Get sports
        html += "<h2>Available Sports</h2>";
        var sports = await client.GetFromJsonAsync<Sport[]>($"sports?apiKey={apiKey}");

        if (sports != null)
        {
            html = sports.Take(10).Aggregate(html,
                (current, sport) =>
                    current +
                    $"<div class='sport'><strong>{sport.Title}</strong> (Key: <code>{sport.Key}</code>, Active: {sport.Active})</div>");
        }

        // Get odds
        html += "<h2>Upcoming Matches</h2>";
        var odds = await client.GetFromJsonAsync<Match[]>(
            $"sports/upcoming/odds?apiKey={apiKey}&regions=us&markets=h2h&oddsFormat=decimal");

        if (odds != null)
        {
            foreach (var match in odds.Take(5))
            {
                html += $"<div class='match'>";
                html += $"<h3>{match.HomeTeam} vs {match.AwayTeam}</h3>";
                html += $"<p>Starts: {match.CommenceTime:yyyy-MM-dd HH:mm}</p>";

                foreach (var bookmaker in match.Bookmakers.Take(3))
                {
                    html += $"<div class='bookmaker'><strong>{bookmaker.Title}:</strong><br>";
                    html = bookmaker.Markets.SelectMany(m => m.Outcomes).Aggregate(html,
                        (current, outcome) =>
                            current + $"<div class='outcome'>{outcome.Name}: {outcome.Price:F2}</div>");

                    html += "</div>";
                }

                html += "</div>";
            }
        }
    }
    catch (Exception ex)
    {
        html += $"<p style='color:red;'><strong>Error:</strong> {ex.Message}</p>";
        html += "<p>Make sure to replace YOUR_API_KEY_HERE with your actual API key!</p>";
    }

    html += "</body></html>";
    return Results.Content(html, "text/html");
});

app.Run();

// Models
record Sport(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("active")] bool Active
);

record Match(
    [property: JsonPropertyName("home_team")]
    string HomeTeam,
    [property: JsonPropertyName("away_team")]
    string AwayTeam,
    [property: JsonPropertyName("commence_time")]
    DateTime CommenceTime,
    [property: JsonPropertyName("bookmakers")]
    List<Bookmaker> Bookmakers
);

record Bookmaker(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("markets")]
    List<Market> Markets
);

record Market(
    [property: JsonPropertyName("outcomes")]
    List<Outcome> Outcomes
);

record Outcome(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("price")] decimal Price
);