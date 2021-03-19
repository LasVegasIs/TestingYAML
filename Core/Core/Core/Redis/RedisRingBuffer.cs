using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Services
{
    public class RedisRingBufferOptions
    {
        public string Key { get; set; }
        public int Capacity { get; set; }
    };

    public class RedisRingBuffer<T>
        where T : class
    {
        private RedisConnection redisConnection_;
        private int redisListLimit_;

        public RedisRingBufferOptions Options;

        public RedisRingBuffer(RedisConnection redisConnection, Action<RedisRingBufferOptions> optionsBuilder)
        {
            redisConnection_ = redisConnection;
            Options = new RedisRingBufferOptions
            {
                Key = $"{typeof(T).Name}Buffer"
                //Key = $"RingBuffer_{typeof(T).Name}"
            };
            optionsBuilder(Options);

            redisListLimit_ = Options.Capacity - 1;
            if (redisListLimit_ < 0)
            {
                redisListLimit_ = 0;
            }
        }

        public Task StoreAsync(T data)
        {
            return StoreAsync(new T[] { data });
        }

        public async Task StoreAsync(IEnumerable<T> data)
        {
            IDatabase database = redisConnection_.Connection.GetDatabase();
            var itemsAsJson = data.Select(x => new RedisValue(JsonConvert.SerializeObject(x))).ToArray();

            ITransaction transaction = database.CreateTransaction();
            _ = transaction.ListLeftPushAsync(Options.Key, itemsAsJson);
            _ = transaction.ListTrimAsync(Options.Key, 0, redisListLimit_);
            _ = await transaction.ExecuteAsync();
        }

        /// <summary>
        ///  Get stored data, silently ignores errors
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetDataAsync(int count)
        {
            IDatabase database = redisConnection_.Connection.GetDatabase();
            RedisValue[] list = await database.ListRangeAsync(Options.Key, 0, count);
            var lst = list
                .Select(x =>
                {
                    try { return JsonConvert.DeserializeObject<T>(x); }
                    catch (Exception) { return null; }
                }).Where(x => x != null);
            return lst;
        }

        public async Task<IEnumerable<T>> GetAllDataAsync()
        {
            return await GetDataAsync(redisListLimit_);
        }

        public async Task ClearAsync()
        {
            IDatabase database = redisConnection_.Connection.GetDatabase();
            await database.KeyDeleteAsync(Options.Key);
        }
    }

    public static class RedisConnectionExtensions
    {
        public static IServiceCollection AddRedisRingBuffer<T>(this IServiceCollection collection, Action<RedisRingBufferOptions> optionsBuilder)
            where T : class
        {
            collection.AddSingleton((services) => new RedisRingBuffer<T>(services.GetRequiredService<RedisConnection>(), optionsBuilder));
            return collection;
        }
    }
}
