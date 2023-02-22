using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Repositories;

namespace WhatsTheWeather.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRepository<int, Race> _raceRepo;
    public string? RaceName { get; private set; }
    public int? RaceYear { get; private set; }
    public DateTime? Start { get; private set; }

    public IndexModel(
        ILogger<IndexModel> logger,
        IRepository<int, Race> raceRepo)
    {
        _logger = logger;
        _raceRepo = raceRepo;
    }

    public async Task OnGetAsync()
    {
        // to be set from UI later
        RaceName = "Vasaloppet";
        RaceYear = 2023;
        // calling internal API
        var client = new HttpClient{
            BaseAddress = new Uri("https://whats-the-weather.azurewebsites.net/api/")
        };
        _logger.LogInformation($"calling api: {client.BaseAddress.AbsoluteUri}");
        // get the race
        var response = await client.GetAsync($"races/{RaceName}/{RaceYear}");
        response.EnsureSuccessStatusCode();
        var raceJson = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var race = JsonSerializer.Deserialize<Race>(raceJson, options);
        if (race == null)
        {
            _logger.LogError($"Race {RaceName} in year {RaceYear} not found.");
            return;
        }
        Start = race.Start;
    }
}
