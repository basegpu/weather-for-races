namespace WhatsTheWeather.Models.Domain;

public record Weather(
    int Pictocode,
    int Predictability,
    double TemperatureMin,
    double TemperatureMax,
    double Precipitation,
    double PrecipitationProbability);
