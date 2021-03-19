using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Core.Extensions.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        // Create an instance of T by getting constructor parameters from the service provider even if T is not registered
        // https://stackoverflow.com/questions/38182929/how-to-resolve-un-registered-type-using-standard-mvc-core-dependency-injection
        public static TService AsSelf<TService>(this IServiceProvider serviceProvider)
        {
            return (TService)AsSelf(serviceProvider, typeof(TService));
        }

        public static object AsSelf(this IServiceProvider serviceProvider, Type serviceType)
        {
            var constructors = serviceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(o => o.GetParameters())
                .ToArray()
                .OrderByDescending(o => o.Length)
                .ToArray();

            if (!constructors.Any())
            {
                throw new Exception($"No public construct found for {serviceType}");
            }

            object[] arguments = ResolveParameters(serviceProvider, constructors);

            if (arguments == null)
            {
                throw new Exception($"No usable construct found for {serviceType}");
            }

            return Activator.CreateInstance(serviceType, arguments);
        }

        private static object[] ResolveParameters(IServiceProvider resolver, ParameterInfo[][] constructors)
        {
            foreach (ParameterInfo[] constructor in constructors)
            {
                bool hasNull = false;
                object[] values = new object[constructor.Length];
                for (int i = 0; i < constructor.Length; i++)
                {
                    var value = resolver.GetService(constructor[i].ParameterType);
                    values[i] = value;
                    if (value == null)
                    {
                        Console.WriteLine($"Could not get value for {constructor[i].ParameterType}"); // todo make it part of the exception info.
                        hasNull = true;
                        break;
                    }
                }
                if (!hasNull)
                {
                    // found a constructor we can create.
                    return values;
                }
            }

            return null;
        }
    }
}