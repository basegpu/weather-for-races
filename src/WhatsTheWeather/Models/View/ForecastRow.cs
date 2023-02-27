using WhatsTheWeather.Models.Domain;

namespace WhatsTheWeather.Models.View;

public record ForecastRow(
    int Id,
    DateTime Time,
    int Km,
    Weather Data);