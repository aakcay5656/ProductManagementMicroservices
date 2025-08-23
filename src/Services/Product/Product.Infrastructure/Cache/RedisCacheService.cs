using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Product.Core.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Product.Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly StackExchange.Redis.IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
        {
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var cachedValue = await _database.StringGetAsync(key);

                if (!cachedValue.HasValue)
                {
                    return null;
                }

                var result = JsonSerializer.Deserialize<T>(cachedValue!, _jsonOptions);
                _logger.LogDebug("Cache hit for key: {Key}", key);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var jsonValue = JsonSerializer.Serialize(value, _jsonOptions);

                await _database.StringSetAsync(key, jsonValue, expiration);

                _logger.LogDebug("Cache set for key: {Key}, Expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogDebug("Cache removed for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
                var keys = server.Keys(pattern: pattern).ToArray();

                if (keys.Length > 0)
                {
                    await _database.KeyDeleteAsync(keys);
                    _logger.LogDebug("Cache removed for pattern: {Pattern}, Keys: {KeyCount}", pattern, keys.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }
    }
}
