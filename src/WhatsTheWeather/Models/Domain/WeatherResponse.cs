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
    public Weather? MakeWeather(DateTime when)
	{
		var index = Time.FindIndex(dt => when < dt) - 1;
        return (index < 0) ? null : new Weather(
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
