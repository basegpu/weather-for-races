using Microsoft.AspNetCore.Mvc.RazorPages;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Models.View;
using WhatsTheWeather.Repositories;

namespace WhatsTheWeather.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IRepository<int, Race> _raceRepo;
    private readonly IRepository<int, WeatherRecord> _weatherRepo;
    private readonly HttpClient _api;

    public string RaceName { get; private set; }
    public int RaceYear { get; private set; }
    public TimeSpan RaceDuration { get; private set; }
    public Race Race { get; private set; }
    public Dictionary<string, ForecastRow> Rows { get; private set; } = new();

    public IndexModel(
        ILogger<IndexModel> logger,
        IRepository<int, Race> raceRepo,
        IRepository<int, WeatherRecord> weatherRepo,
        IConfiguration config)
    {
        _logger = logger;
        _raceRepo = raceRepo;
        _weatherRepo = weatherRepo;
        var dataBackend = "https://whats-the-weather.azurewebsites.net/api/";
        _api = new HttpClient{
            BaseAddress = new Uri(dataBackend)
        };
        _logger.LogInformation($"api for data: {_api.BaseAddress.AbsoluteUri}");
        // to be set from UI later
        RaceName = "Vasaloppet";
        RaceYear = 2023;
        RaceDuration = TimeSpan.FromHours(6);
        var raceId = Race.MakeHashCode(RaceName, RaceYear);
        Race = GetRace(RaceName, RaceYear);
        // init the forecast dict and fill with meteo data
        var totalDistance = Race.Checkpoints.Last().Distance;
        foreach (var checkpoint in Race.Checkpoints)
        {
            var record = GetLatestForecast(checkpoint.Location, Race.Start).Result;
            var id = _weatherRepo.Update(record);
            var forecast = new ForecastRow(
                id,
                Race.Start + RaceDuration * (double)checkpoint.Distance/totalDistance,
                checkpoint.Distance,
                record.Data);
            Rows.Add(checkpoint.Name, forecast);
        }
    }

    public void OnGet()
    {
        foreach (var checkpoint in Race.Checkpoints)
        {
            if (Rows.TryGetValue(checkpoint.Name, out var forecast))
            {
                // update with latest data
                if (_weatherRepo.TryGetById(forecast.Id, out var newForecast))
                {
                    Rows[checkpoint.Name] = forecast with { Data = newForecast!.Data};
                }
            }
        }
    }

    private Race GetRace(string name, int year)
    {
        var raceId = Race.MakeHashCode(name, year);
        if (_raceRepo.TryGetById(raceId, out var race))
        {
            return race!;
        }
        var msg = $"Race {name} in year {year} not found.";
        _logger.LogError(msg);
        throw new KeyNotFoundException(msg);
    }

    private async Task<WeatherRecord> GetLatestForecast(Coordinates location, DateTime time)
    {
        var request = new WeatherRequest(location, time);
        var response = await _api.PostAsJsonAsync<WeatherRequest>($"weather", request);
        if (!response.IsSuccessStatusCode)
        {
            response = await _api.PostAsJsonAsync<WeatherRequest>($"forecast", request);
        }
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
}
