namespace WhatsTheWeather.Models.Domain;

public record WeatherResponse(
    List<DateTime> Time,
    List<int> PictoCode,
    List<int> Predictability
)
{
    public Weather? MakeWeather(DateTime when)
	{
		var index = Time.FindIndex(dt => dt.Date == when.Date);
        return (index < 0) ? null : new Weather(
			PictoCode[index],
			Predictability[index]
		);
	}
}
