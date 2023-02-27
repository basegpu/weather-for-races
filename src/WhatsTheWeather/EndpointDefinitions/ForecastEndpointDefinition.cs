using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Serilog;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.SecretSauce;

namespace WhatsTheWeather.EndpointDefinitions;

public class ForecastEndpointDefinition : IEndpointDefinition
{
	private readonly string _path = "/api/forecast";
	private readonly HttpClient _client = new()
	{
		BaseAddress = new Uri("https://my.meteoblue.com"),
	};
	private string _apiKey = "";
	
	public void DefineEndpoints(WebApplication app)
	{
		app.MapPost(_path, GetWeatherByCoords)
			.Produces<WeatherRecord>(200)
			.Produces<WeatherRequest>(404);
	}

	public void DefineServices(IServiceCollection services)
	{
		var config = services.BuildServiceProvider().GetService<IConfiguration>()!;
		_apiKey = config["Meteoblue:ApiKey"];
	}

	internal async Task<IResult> GetWeatherByCoords(
		[FromBody] WeatherRequest request)
	{
		WeatherRecord? forecast;
		Log.Information("calling meteoblue api to gather new forecast...");
		forecast = await GetForecastFromMeteoblue(request);
		if (forecast != null)
		{
			return Results.Ok(forecast);
		}
		return Results.NotFound(request);
	}

	private async Task<WeatherRecord?> GetForecastFromMeteoblue(
		WeatherRequest request)
	{
		var package = "trend-1h";
		var req = $"packages/{package}?apikey={_apiKey}";
		req += $"&lon={request.Where.Longitude}&lat={request.Where.Latitude}";
		req += $"&asl={request.Where.Altitude}&format=json";
		using HttpResponseMessage response = await _client.GetAsync(req);
    
		if (!response.IsSuccessStatusCode)
		{
			Log.Error($"failing request: {req}");
			return null;
		}
		
		var jsonResponse = await response.Content.ReadAsStringAsync();
		//System.Console.WriteLine(jsonResponse);
		var obj = JObject.Parse(jsonResponse);
		var published = obj["metadata"]["modelrun_updatetime_utc"].Value<DateTime>();
		published = DateTime.SpecifyKind(published, DateTimeKind.Utc);
		var data = obj[package.Replace('-', '_')].ToObject<WeatherResponse>();
		var weather = data.MakeWeather(request.When);
		return new WeatherRecord(request, weather, published);
	}
}