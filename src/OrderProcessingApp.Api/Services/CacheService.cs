using StackExchange.Redis;
using System.Text.Json;
using OrderProcessingApp.Api.Interfaces;

namespace OrderProcessingApp.Api.Services;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IServer _server;
    private readonly ILogger<CacheService> _logger;
    private readonly TimeSpan _defaultExpiration;

    public CacheService(IConnectionMultiplexer redis, IConfiguration configuration, ILogger<CacheService> logger)
    {
        _database = redis.GetDatabase();
        _server = redis.GetServer(redis.GetEndPoints().First());
        _logger = logger;
        
        var defaultTtl = configuration.GetValue<int>("Cache:DefaultTTLSeconds", 3600);
        _defaultExpiration = TimeSpan.FromSeconds(defaultTtl);
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            
            if (!value.HasValue)
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            var ttl = expiration ?? _defaultExpiration;
            
            await _database.StringSetAsync(key, serialized, ttl);
            _logger.LogDebug("Cached value for key: {Key} with TTL: {TTL}s", key, ttl.TotalSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Removed cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var keys = _server.Keys(pattern: pattern).ToArray();
            
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keys.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
        }
    }
}
