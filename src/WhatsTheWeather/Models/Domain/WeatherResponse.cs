namespace WhatsTheWeather.Models.Domain;

public record WeatherResponse(
    List<DateTime> Time,
    List<int> PictoCode,
    List<int> Predictability,
	List<double> Temperature_Min,
	List<double> Temperature_Max,
	List<double> Precipitation,
	List<double> Precipitation_Probability
)
{
    public Weather? MakeWeather(DateTime when)
	{
		var index = Time.FindIndex(dt => dt.Date == when.Date);
        return (index < 0) ? null : new Weather(
			PictoCode[index],
			Predictability[index],
			Temperature_Min[index],
			Temperature_Max[index],
			Precipitation[index],
			Precipitation_Probability[index]
		);
	}
}
