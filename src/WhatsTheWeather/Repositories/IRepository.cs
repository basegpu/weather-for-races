namespace WhatsTheWeather.Repositories;

public interface IRepository<TKey, TValue>
{
	TKey Add(TValue entity);
	bool TryGetById(TKey id, out TValue? entity);
	IDictionary<TKey, TValue> GetAll();
}