namespace WhatsTheWeather.Models.Domain;

public record WeatherRecord(
    WeatherRequest Request,
    Weather Data,
    DateTime Published)
{
    public override int GetHashCode() => Request.GetHashCode();
}
