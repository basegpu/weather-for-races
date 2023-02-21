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

		services.AddSingleton<IRepository<int, WeatherRecord>, WeatherRepository>();
	}

	internal IResult GetWeatherById(
		IRepository<int, WeatherRecord> repo,
		[FromRoute] int id
	)
	{
		return repo.TryGetById(id, out var record) ?
			Results.Ok(record) :
			Results.NotFound(id);
	}

	internal async Task<IResult> GetWeatherByCoords(
		IRepository<int, WeatherRecord> repo,
		[FromBody] WeatherRequest request
	)
	{
		var id = request.GetHashCode();
		Log.Debug($"request hash {id}");
		if (repo.TryGetById(id, out var record))
		{
			return Results.Ok(record);
		}
		Log.Information("calling meteoblue api to gather new forecast...");
		var weather = await GetWeather(request, "trend-day");
		if (weather == null)
		{
			Results.NotFound(request);
		}
		record = new WeatherRecord(request, weather!, DateTime.Now);
		id = repo.Add(record);
		return Results.Created($"{_path}/{id}", record);
	}

	private async Task<Weather?> GetWeather(
		WeatherRequest request,
		string package
	)
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
		var data = obj[package.Replace('-', '_')].ToObject<WeatherResponse>();

		return data.MakeWeather(request.When);
	}
}