using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Repositories;
using WhatsTheWeather.SecretSauce;

namespace WhatsTheWeather.EndpointDefinitions;

public class WeatherEndpointDefinition : IEndpointDefinition
{
	private readonly string _path = "/api/weather";
	private readonly HttpClient _client = new()
	{
		BaseAddress = new Uri("https://my.meteoblue.com"),
	};
	private string _apiKey = "";
	
	public void DefineEndpoints(WebApplication app)
	{
		app.MapGet(_path + "/{id}" , GetWeatherById)
			.Produces<WeatherRecord>(200)
			.Produces<int>(404);
		app.MapPost(_path, GetWeatherByCoords)
			.Produces<WeatherRecord>(200)
			.Produces<WeatherRecord>(201)
			.Produces<WeatherRequest>(404);
	}

	public void DefineServices(IServiceCollection services)
	{
		var config = services.BuildServiceProvider().GetService<IConfiguration>()!;
		_apiKey = config["Meteoblue:ApiKey"];
		Log.Debug($"meteoblue apikey: {_apiKey}");
		services.AddSingleton<IRepository<int, WeatherRecord>, WeatherRepository>();
	}

	internal IResult GetWeatherById(
		IRepository<int, WeatherRecord> repo,
		[FromRoute] int id)
	{
		return repo.TryGetById(id, out var record) ?
			Results.Ok(record) :
			Results.NotFound(id);
	}

	internal async Task<IResult> GetWeatherByCoords(
		IRepository<int, WeatherRecord> repo,
		[FromBody] WeatherRequest request,
		[FromQuery] bool ForceExternal = false)
	{
		WeatherRecord? forecast;
		if (!ForceExternal)
		{
			// try first locally from repo
			var id = request.GetHashCode();
			if (repo.TryGetById(id, out forecast))
			{
				return Results.Ok(forecast);
			}
		}
		Log.Information("calling meteoblue api to gather new forecast...");
		forecast = await GetForecastFromMeteoblue(request, "trend-day");
		if (forecast != null)
		{
			var id = repo.Add(forecast);
			return Results.Created($"{_path}/{id}", forecast);
		}
		return Results.NotFound(request);
	}

	private async Task<WeatherRecord?> GetForecastFromMeteoblue(
		WeatherRequest request,
		string package)
	{
		var req = $"packages/{package}?apikey={_apiKey}";
		req += $"&lon={request.Where.Longitude}&lat={request.Where.Latitude}";
		req += $"&asl={request.Where.Altitude}&format=json";
		using HttpResponseMessage response = await _client.GetAsync(req);
    
		if (!response.IsSuccessStatusCode)
			return null;
		
		var jsonResponse = await response.Content.ReadAsStringAsync();
		//System.Console.WriteLine(jsonResponse);
		var obj = JObject.Parse(jsonResponse);
		var published = obj["metadata"]["modelrun_updatetime_utc"].Value<DateTime>();
		published = DateTime.SpecifyKind(published, DateTimeKind.Utc);
		var data = obj[package.Replace('-', '_')].ToObject<WeatherResponse>();
		var weather = data.MakeWeather(request.When);

		if (weather == null)
			return null;

		return new WeatherRecord(request, weather, published);
	}
}