namespace OrderService.Infrastructure.Common.CacheProvider
{
    public class RedisHashProvider:IRedisHashProvider
    {
        private readonly IDatabase _db;

        public RedisHashProvider(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }
        public async Task<bool> SaveAsync<T>(string key, string field, T value, TimeSpan? expiry = null)
        {
            string serializedValue = JsonSerializer.Serialize(value);

            var tran = _db.CreateTransaction();
            tran.AddCondition(Condition.HashNotExists(key, field));
            _ = tran.HashSetAsync(key, field, serializedValue);

            if (expiry.HasValue)
                await _db.KeyExpireAsync(key, expiry);
            

            return await tran.ExecuteAsync();
        }
        public async Task<bool> UpdateAsync<T>(string key, string field, T value, TimeSpan? expiry = null)
        {
            string serializedValue = JsonSerializer.Serialize(value);

            var tran = _db.CreateTransaction();
            tran.AddCondition(Condition.HashExists(key, field));
            _ = tran.HashSetAsync(key, field, serializedValue);

            if (expiry.HasValue)
                await _db.KeyExpireAsync(key, expiry);

            return await tran.ExecuteAsync();
        }
        public async Task<bool> CheckExistAsync<T>(string key, Expression<Func<T, bool>> predicate)
        {
            var entries = await _db.HashGetAllAsync(key);
            if (entries.Length == 0) return false;

            // Compile predicate
            var compiled = predicate.Compile();

            foreach (var entry in entries)
            {
                var obj = JsonSerializer.Deserialize<T>(entry.Value!);
                if (obj != null && compiled(obj))
                {
                    return true; // Found matching item
                }
            }
            return false;
        }
        public async Task<T?> GetAsync<T>(string key, string field)
        {
            RedisValue value = await _db.HashGetAsync(key, field);
            if (value.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>(value!);
        }

        public async Task<List<T>> GetAllAsync<T>(string key)
        {
            HashEntry[] entries = await _db.HashGetAllAsync(key);
            if (entries == null || entries.Length == 0) return new List<T>();

            return entries
                .Select(e => JsonSerializer.Deserialize<T>(e.Value!))
                .Where(x => x != null)
                .Cast<T>()
                .ToList();
        }

        public async Task<bool> DeleteFieldAsync(string key, string? field)
        {
            var tran = _db.CreateTransaction();
            if(field != null)
            {
                tran.AddCondition(Condition.HashExists(key, field));
                _ = tran.HashDeleteAsync(key, field);
            }
            else
            {
                tran.AddCondition(Condition.KeyExists(key));
                _ = tran.KeyDeleteAsync(key);
            }


                return await tran.ExecuteAsync();
        }
    }
}
