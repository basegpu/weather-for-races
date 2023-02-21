namespace WhatsTheWeather.Models.Domain;

public record WeatherRequest(
    Coordinates Where,
    DateTime When)
{
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Where);
        hash.Add(When);
        return hash.ToHashCode();
    }
}
