using System.Text.Json;

namespace WhatsTheWeather.Models.Domain;

public record WeatherRecord(
    WeatherRequest Request,
    Weather Data,
    DateTime Published)
{
    private static JsonSerializerOptions s_opts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public override int GetHashCode() => Request.GetHashCode();

    public static WeatherRecord? FromJson(string json)
        => JsonSerializer.Deserialize<WeatherRecord>(json, s_opts);
}
