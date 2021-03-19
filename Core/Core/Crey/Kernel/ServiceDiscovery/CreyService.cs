using Core.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading;

namespace Crey.Kernel.ServiceDiscovery
{
    public interface ICreyService<TIface> where TIface : class
    {
        TIface Value { get; }
    }

    // helper to allow implementing the ICreyService twice (once for interface, once for implementation)
    public abstract class CreyService<TIface> : ICreyService<TIface>
        where TIface : class
    {
        public abstract TIface Value { get; }
    }

    /// <summary>
    /// Provides lazy initialization semantics and wrapped with methods for convenient registration.
    /// </summary>
    public class CreyService<TIface, TImpl> : CreyService<TIface>, ICreyService<TImpl>
        where TIface : class
        where TImpl : class, TIface
    {
        private readonly IServiceProvider provider_;
        private TImpl service_;

        public CreyService(IServiceProvider provider)
        {
            provider_ = provider;
        }

        public override TIface Value => (this as ICreyService<TImpl>).Value;

        TImpl ICreyService<TImpl>.Value => LazyInitializer.EnsureInitialized(ref service_, () => provider_.AsSelf<TImpl>());
    }


    public static class ICreyServiceExtensions
    {
        public static IServiceCollection AddSingletonCreyService<T, TImpl>(this IServiceCollection collectionBuilder)
            where T : class
            where TImpl : class, T
        {
            collectionBuilder.TryAddSingleton<CreyService<T, TImpl>>();
            collectionBuilder.TryAddSingleton<ICreyService<T>>(x => x.GetRequiredService<CreyService<T, TImpl>>());
            collectionBuilder.TryAddSingleton<ICreyService<TImpl>>(x => x.GetRequiredService<CreyService<T, TImpl>>());
            return collectionBuilder;
        }

        public static IServiceCollection AddScopedCreyService<T, TImpl>(this IServiceCollection collectionBuilder)
           where T : class
           where TImpl : class, T
        {
            collectionBuilder.TryAddScoped<CreyService<T, TImpl>>();
            collectionBuilder.TryAddScoped<ICreyService<T>>(x => x.GetRequiredService<CreyService<T, TImpl>>());
            collectionBuilder.TryAddScoped<ICreyService<TImpl>>(x => x.GetRequiredService<CreyService<T, TImpl>>());
            return collectionBuilder;
        }

        public static IServiceCollection AddTransientCreyService<T, TImpl>(this IServiceCollection collectionBuilder)
          where T : class
          where TImpl : class, T
        {
            collectionBuilder.TryAddTransient<CreyService<T, TImpl>>();
            collectionBuilder.TryAddTransient<ICreyService<T>>(x => x.GetRequiredService<CreyService<T, TImpl>>());
            collectionBuilder.TryAddTransient<ICreyService<TImpl>>(x => x.GetRequiredService<CreyService<T, TImpl>>());
            return collectionBuilder;
        }

        public static IServiceCollection AddSingletonCreyServiceInternal<T>(this IServiceCollection collectionBuilder)
         where T : class
        {
            collectionBuilder.TryAddSingleton<CreyService<T, T>>();
            collectionBuilder.TryAddSingleton<ICreyService<T>>(x => x.GetRequiredService<CreyService<T, T>>());
            return collectionBuilder;
        }


        public static IServiceCollection AddScopedCreyServiceInternal<T>(this IServiceCollection collectionBuilder)
         where T : class
        {
            collectionBuilder.TryAddScoped<CreyService<T, T>>();
            collectionBuilder.TryAddScoped<ICreyService<T>>(x => x.GetRequiredService<CreyService<T, T>>());
            return collectionBuilder;
        }

        public static IServiceCollection AddTransientCreyServiceInternal<T>(this IServiceCollection collectionBuilder)
         where T : class
        {
            collectionBuilder.TryAddTransient<CreyService<T, T>>();
            collectionBuilder.TryAddTransient<ICreyService<T>>(x => x.GetRequiredService<CreyService<T, T>>());
            return collectionBuilder;
        }
    }
}
