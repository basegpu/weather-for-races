using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WhatsTheWeather.Models.Domain;
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
		app.MapPost(_path, GetWeatherByCoords).Produces<Weather>(200).Produces<WeatherRequest>(404);
	}

	public void DefineServices(IServiceCollection services)
	{
		var config = services.BuildServiceProvider().GetService<IConfiguration>()!;
		_apiKey = config["Meteoblue:ApiKey"];
	}

	internal async Task<IResult> GetWeatherByCoords(
		[FromBody] WeatherRequest request
	)
	{
		var response = await GetWeather(request, "trend-day");
		
		return (response == null) ?
			Results.NotFound(request) :
			Results.Ok(response);
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