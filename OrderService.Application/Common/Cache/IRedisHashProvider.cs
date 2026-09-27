namespace OrderService.Application.Common.Cache
{
    public interface IRedisHashProvider
    {
        Task<bool> SaveAsync<T>(string key, string field, T value, TimeSpan? expiry = null);
        Task<bool> UpdateAsync<T>(string key, string field, T value, TimeSpan? expiry = null);
        Task<bool> CheckExistAsync<T>(string key, Expression<Func<T, bool>> predicate);
        Task<T?> GetAsync<T>(string key, string field);
        Task<List<T>> GetAllAsync<T>(string key);
        Task<bool> DeleteFieldAsync(string key, string? field);
    }
}
