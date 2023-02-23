using Microsoft.AspNetCore.Mvc.RazorPages;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Repositories;

namespace WhatsTheWeather.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRepository<int, Race> _raceRepo;
    private readonly HttpClient _api;

    public string? RaceName { get; private set; }
    public int? RaceYear { get; private set; }
    public DateTime? Start { get; private set; }
    public Dictionary<string, (int Km, Weather Data)> Forecast { get; private set; } = new();

    public IndexModel(
        ILogger<IndexModel> logger,
        IRepository<int, Race> raceRepo,
        IConfiguration config)
    {
        _logger = logger;
        _raceRepo = raceRepo;
        var uri = "https://whats-the-weather.azurewebsites.net/api/";
        _api = new HttpClient{
            BaseAddress = new Uri(uri)
        };
        _logger.LogInformation($"api for data: {_api.BaseAddress.AbsoluteUri}");
    }

    public async Task OnGetAsync()
    {
        // to be set from UI later
        RaceName = "Vasaloppet";
        RaceYear = 2023;
        // get the race
        var response = await _api.GetAsync($"races/{RaceName}/{RaceYear}");
        response.EnsureSuccessStatusCode();
        var race = Race.FromJson(await response.Content.ReadAsStringAsync());
        if (race == null)
        {
            _logger.LogError($"Race {RaceName} in year {RaceYear} not found.");
            return;
        }
        Start = race.Start;
        // get the meteo forecast for all checkpoints
        foreach (var checkpoint in race.Checkpoints)
        {
            var request = new WeatherRequest(checkpoint.Location, race.Start);
            response = await _api.PostAsJsonAsync<WeatherRequest>("weather", request);
            response.EnsureSuccessStatusCode();
            var record = WeatherRecord.FromJson(await response.Content.ReadAsStringAsync());
            if (record == null)
            {
                _logger.LogError($"No forecast found for {request}.");
                return;
            }
            Forecast.Add(checkpoint.Name, (checkpoint.Distance, record.Data));
        }
    }
}
