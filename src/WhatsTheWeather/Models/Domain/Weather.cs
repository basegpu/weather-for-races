namespace WhatsTheWeather.Models.Domain;

public record Weather(
    int PictoCode,
	double Temperature,
	double TemperatureSpread,
	double Precipitation,
	double PrecipitationSpread,
    double Windspeed,
    double WindspeedSpread,
    int WindDirection);
