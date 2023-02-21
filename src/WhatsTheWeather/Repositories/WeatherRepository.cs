using WhatsTheWeather.Models.Domain;

namespace WhatsTheWeather.Repositories;

class WeatherRepository : MemoryRepository<WeatherRecord>, IRepository<int, WeatherRecord>
{
}