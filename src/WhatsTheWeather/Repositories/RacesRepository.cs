using WhatsTheWeather.Models.Domain;

namespace WhatsTheWeather.Repositories;

class RacesRepository : MemoryRepository<Race>, IRepository<int, Race>
{
}