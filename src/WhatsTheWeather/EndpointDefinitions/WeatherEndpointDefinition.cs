using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
		app.MapPost(_path, GetWeatherByCoords).Produces<Weather>(200).Produces(404);
	}

	public void DefineServices(IServiceCollection services)
	{
		var config = services.BuildServiceProvider().GetService<IConfiguration>();
		_apiKey = config["Meteoblue:ApiKey"];
	}

	internal async Task<IResult> GetWeatherByCoords(
		[FromBody] WeatherRequest request
	)
	{
		var package = "trend-day";
		var req = $"packages/{package}?apikey={_apiKey}";
		req += $"&lon={request.Where.Longitude}&lat={request.Where.Latitude}";
		req += $"&asl={request.Where.Altitude}&format=json";
		using HttpResponseMessage response = await _client.GetAsync(req);
    
		if (!response.IsSuccessStatusCode)
		{
			System.Console.WriteLine(req);
			return Results.NotFound();
		}
		
		var jsonResponse = await response.Content.ReadAsStringAsync();
		//System.Console.WriteLine($"{jsonResponse}\n");
		var obj = JObject.Parse(jsonResponse);
		var data = obj["trend_day"];
		var index = -1;
		foreach (var day in data["time"])
		{
			var dt = DateTime.Parse(day.Value<string>());
			if (dt > request.When)
			{
				break;
			}
			index++;
		}

		var weather = new Weather(
			data["pictocode"][index].Value<int>(),
			data["predictability"][index].Value<int>());

		return Results.Ok(weather);
	}
}