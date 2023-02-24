using Serilog;

namespace WhatsTheWeather.Models.Domain;

public record WeatherResponse(
    List<DateTime> Time,
    List<int> PictoCode,
	List<double> Temperature,
	List<double> Temperature_Spread,
	List<double> Precipitation,
	List<double> Precipitation_Spread,
    List<double> Windspeed,
    List<double> Windspeed_Spread,
    List<int> WindDirection
)
{
    public Weather MakeWeather(DateTime when)
	{
		var closest = Time
			.Select( (x, index) => (Item: x, Index: index)) // Save the item index and item in an anonymous class
			.Aggregate((x,y) => Math.Abs((x.Item - when).TotalSeconds) < Math.Abs((y.Item - when).TotalSeconds) ? x : y);
		var index = closest.Index;
		Log.Information(closest.ToString());
        return new Weather(
			PictoCode[index],
			Temperature[index],
			Temperature_Spread[index],
			Precipitation[index],
			Precipitation_Spread[index],
			Windspeed[index],
			Windspeed_Spread[index],
			WindDirection[index]
		);
	}
}
