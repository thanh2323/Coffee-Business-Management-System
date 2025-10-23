using CoffeeShop.Application.Interface.IRepo;
using CoffeeShop.Domain.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace CoffeeShop.Infrastructure.Repository
{
    public class TempOrderRepository : ITempOrderRepository
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private const string TEMP_ORDER_PREFIX = "order:temp:";
        private const string IDEMPOTENCY_PREFIX = "payment:processed:";
        private const int DEFAULT_TTL_MINUTES = 15;

        public TempOrderRepository(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = redis.GetDatabase();
        }

        public async Task<TempOrder?> GetAsync(string tempOrderId)
        {
            var key = TEMP_ORDER_PREFIX + tempOrderId;
            var json = await _database.StringGetAsync(key);
            
            if (!json.HasValue)
                return null;

            try
            {
                return JsonSerializer.Deserialize<TempOrder>(json!);
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SetAsync(TempOrder tempOrder, TimeSpan? expiry = null)
        {
            var key = TEMP_ORDER_PREFIX + tempOrder.TempOrderId;
            var json = JsonSerializer.Serialize(tempOrder);
            var ttl = expiry ?? TimeSpan.FromMinutes(DEFAULT_TTL_MINUTES);
            
            return await _database.StringSetAsync(key, json, ttl);
        }

        public async Task<bool> DeleteAsync(string tempOrderId)
        {
            var key = TEMP_ORDER_PREFIX + tempOrderId;
            return await _database.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string tempOrderId)
        {
            var key = TEMP_ORDER_PREFIX + tempOrderId;
            return await _database.KeyExistsAsync(key);
        }

        public async Task SetIdempotencyAsync(string reference, TimeSpan? expiry = null)
        {
            var key = IDEMPOTENCY_PREFIX + reference;
            var ttl = expiry ?? TimeSpan.FromHours(24);
            await _database.StringSetAsync(key, "1", ttl);
        }

        public async Task<bool> IsIdempotentAsync(string reference)
        {
            var key = IDEMPOTENCY_PREFIX + reference;
            return await _database.KeyExistsAsync(key);
        }
    }
}
