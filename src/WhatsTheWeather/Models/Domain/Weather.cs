namespace WhatsTheWeather.Models.Domain;

public record Weather(
    int Pictocode,
    double TemperatureMin,
    double TemperatureMax,
    double Precipitation,
    double PrecipitationProbability,
    int Predictability,
    int PredictabilityClass);
