using Microsoft.AspNetCore.Mvc.RazorPages;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Models.View;
using WhatsTheWeather.Repositories;

namespace WhatsTheWeather.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRepository<int, Race> _raceRepo;
    private readonly HttpClient _api;

    public string RaceName { get; private set; }
    public int RaceYear { get; private set; }
    public TimeSpan RaceDuration { get; private set; }
    public Race Race { get; private set; }
    public Dictionary<string, ForecastRow> Rows { get; private set; } = new();

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
        // to be set from UI later
        RaceName = "Vasaloppet";
        RaceYear = 2023;
        RaceDuration = TimeSpan.FromHours(6);
        Race = GetRaceAsync(RaceName, RaceYear).Result;
        // init the forecast dict and fill with meteo data
        var totalDistance = Race.Checkpoints.Last().Distance;
        foreach (var checkpoint in Race.Checkpoints)
        {
            var record = GetRecordAsync(checkpoint.Location, Race.Start, false).Result;
            var forecast = new ForecastRow(
                Race.Start + RaceDuration * (double)checkpoint.Distance/totalDistance,
                checkpoint.Distance,
                record.Data);
            Rows.Add(checkpoint.Name, forecast);
        }
    }

    public void OnGet()
    {
        // for now get meteo data again (in case it was updated in internal api)
        foreach (var checkpoint in Race.Checkpoints)
        {
            var record = GetRecordAsync(checkpoint.Location, Race.Start, false).Result;
            UpdateForecast(checkpoint.Name, record.Data);
        }
    }

    private async Task<Race> GetRaceAsync(string name, int year)
    {
        // get the race
        var response = await _api.GetAsync($"races/{name}/{year}");
        response.EnsureSuccessStatusCode();
        var race = Race.FromJson(await response.Content.ReadAsStringAsync());
        if (race == null)
        {
            var msg = $"Race {name} in year {year} not found.";
            _logger.LogError(msg);
            throw new KeyNotFoundException(msg);
        }
        return race;
    }

    private async Task<WeatherRecord> GetRecordAsync(Coordinates location, DateTime time, bool update)
    {
        var request = new WeatherRequest(location, time);
        var response = await _api.PostAsJsonAsync<WeatherRequest>($"weather?ForceExternal={update}", request);
        response.EnsureSuccessStatusCode();
        var record = WeatherRecord.FromJson(await response.Content.ReadAsStringAsync());
        if (record == null)
        {
            var msg = $"No forecast found for {request}.";
            _logger.LogError(msg);
            throw new KeyNotFoundException(msg);
        }
        return record;
    }

    private void UpdateForecast(string name, Weather data)
    {
        if (Rows.TryGetValue(name, out var forecast))
        {
            Rows[name] = forecast with { Data = data};
        }
    }
}
