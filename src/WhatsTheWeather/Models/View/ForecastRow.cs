using WhatsTheWeather.Models.Domain;

namespace WhatsTheWeather.Models.View;

public record ForecastRow(
    DateTime Time,
    int Km,
    Weather Data);