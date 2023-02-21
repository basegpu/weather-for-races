using Microsoft.AspNetCore.Mvc;
using WhatsTheWeather.Models.Domain;
using WhatsTheWeather.Repositories;
using WhatsTheWeather.SecretSauce;

namespace WhatsTheWeather.EndpointDefinitions;

public class RacesEndpointDefinition : IEndpointDefinition
{
	private readonly string _path = "/api/races";
	
	public void DefineEndpoints(WebApplication app)
	{
		app.MapGet(_path, GetAll).Produces<IDictionary<int, Race>>(200);
		app.MapGet(_path + "/{name}/{year}", GetRaceByNameAndYear).Produces<Race>(200).Produces(404);
		app.MapGet(_path + "/{id}", GetRaceById).Produces<Race>(200).Produces(404);
	}

	public void DefineServices(IServiceCollection services)
	{
		var repo = new RacesRepository();
		var vasa = new Race(
			"Vasaloppet",
			new DateTime(2023, 3, 5, 8, 0, 0),
			new List<Checkpoint>{
				new Checkpoint("Sälen", 0, new Coordinates(61.150, 13.267, 367)),
				new Checkpoint("Mangsbodarna", 24, new Coordinates(61.083, 13.617, 404)),
				new Checkpoint("Evertsberg", 48, new Coordinates(61.133, 13.950, 442)),
				new Checkpoint("Hökberg", 71, new Coordinates(61.067, 14.317, 250)),
				new Checkpoint("Mora", 90, new Coordinates(61.007, 14.543, 162))
			});
		repo.Add(vasa);
		services.AddSingleton<IRepository<int, Race>>(repo);
	}

	internal IResult GetAll(IRepository<int, Race> repo)
	{
		return Results.Ok(repo.GetAll());
	}

	internal IResult GetRaceByNameAndYear(
		IRepository<int, Race> repo,
		[FromRoute] string name,
		[FromRoute] int year)
	{
		var races = repo.GetAll()
			.Select(kvp => kvp.Value)
			.Where(race =>
				string.Equals(race.Name, name, StringComparison.InvariantCultureIgnoreCase) &&
				race.Start.Year == year);
		if (races.Count() == 0)
		{
			return Results.NotFound($"no race found with name {name} in {year}.");
		}
		return Results.Ok(races.First());
	}

	internal IResult GetRaceById(
		IRepository<int, Race> repo,
		int id)
	{
		return repo.TryGetById(id, out var race) ? Results.Ok(race) : Results.NotFound();
	}
}