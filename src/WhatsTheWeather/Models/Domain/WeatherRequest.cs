namespace WhatsTheWeather.Models.Domain;

public record WeatherRequest(Coordinates Where, DateTime When);
