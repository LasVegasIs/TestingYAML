using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

public class RedisConnectionOptions
{
    public string ConnectionString { get; set; } = "TODO: CALL InitializeConnectionString() method with connection string";
}

public class RedisConnection
{
    private Lazy<ConnectionMultiplexer> multiplexer_;

    public RedisConnectionOptions Options { get; }
    public ConnectionMultiplexer Connection { get { return multiplexer_.Value; } }

    public RedisConnection(Action<RedisConnectionOptions> optionsBuilder)
    {
        Options = new RedisConnectionOptions();
        optionsBuilder(Options);
        multiplexer_ = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(Options.ConnectionString));
    }
}

public static class RedisConnectionExtensions
{
    public static IServiceCollection AddRedisConnection(this IServiceCollection collection, Action<RedisConnectionOptions> optionsBuilder)
    {
        collection.AddSingleton((services) => new RedisConnection(optionsBuilder));
        return collection;
    }
}