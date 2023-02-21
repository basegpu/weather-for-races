using System.Collections.Concurrent;

namespace WhatsTheWeather.Repositories;

class MemoryRepository<T> : IRepository<int, T>
{ 
    protected readonly ConcurrentDictionary<int, T> _repo = new();
    protected readonly ConcurrentQueue<int> _orderedKeys = new();
    private readonly object _injectLock = new();

    public int Add(T entity)
    {
        var hash = entity!.GetHashCode();
        lock (_injectLock)
        {
            if (!_orderedKeys.Contains(hash))
            {
                _orderedKeys.Enqueue(hash);
                _repo.GetOrAdd(hash, (hash) => entity);
            }
        }
        return hash;
    }

    public IDictionary<int, T> GetAll()
    {
        lock (_injectLock)
        {
            return _orderedKeys.ToList().ToDictionary(k => k, k => _repo[k]);
        }
    }

    public bool TryGetById(int id, out T? entity)
    {
        if (_repo.TryGetValue(id, out var obj))
        {
            entity = obj;
            return true;
        }
        entity = default(T);
        return false;
    }
}
