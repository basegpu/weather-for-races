using Microsoft.AspNetCore.Mvc;
using Serilog;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Repositories;
using WhatsTheWeather.SecretSauce;

namespace WhatsTheWeather.EndpointDefinitions;

public class WeatherEndpointDefinition : IEndpointDefinition
{
	private readonly string _path = "/api/weather";
	
	public void DefineEndpoints(WebApplication app)
	{
		app.MapGet(_path + "/{id}" , GetWeatherById)
			.Produces<WeatherRecord>(200)
			.Produces<int>(404);
		app.MapPost(_path, GetWeatherByCoords)
			.Produces<WeatherRecord>(200)
			.Produces<WeatherRequest>(404);
		app.MapPut(_path, UpdateWeather)
			.Produces<int>(201);
	}

	public void DefineServices(IServiceCollection services)
	{
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

	internal IResult GetWeatherByCoords(
		IRepository<int, WeatherRecord> repo,
		[FromBody] WeatherRequest request)
	{
		var id = request.GetHashCode();
		return repo.TryGetById(id, out var record) ?
			Results.Ok(record) :
			Results.NotFound(id);
	}

	internal IResult UpdateWeather(
		IRepository<int, WeatherRecord> repo,
		[FromBody] WeatherRecord record)
	{
		var id = record.GetHashCode();
		var logMsg = repo.TryGetById(id, out _) ?
			"overriding existing weather record..." :
			"adding new weather record...";
		Log.Information(logMsg);
	
		id = repo.Update(record);
		return Results.Created($"{_path}/{id}", id);
	}
}