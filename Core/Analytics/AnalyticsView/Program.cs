using Analytics;
using Analytics.Events;
using Azure.Messaging.EventHubs.Consumer;
using Crey.Instrumentation.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyticsView
{
    class Program
    {
        static void Main(string[] args)
        {
            using ServiceProvider services = RegisterServices(args);

            Task task = Task.Run(async () =>
            {
                IConfiguration configuration = services.GetService<IConfiguration>();

                var connectionString = configuration.GetAnalyticsEventHubConnectionString();
                string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                Console.WriteLine("Start listening to event hub messages");
                await using (var consumer = new EventHubConsumerClient(consumerGroup, connectionString))
                {
                    await foreach (var receivedEvent in consumer.ReadEventsAsync())
                    {
                        var data = Encoding.UTF8.GetString(receivedEvent.Data.Body.ToArray());
                        Console.WriteLine(data);
                    }
                }
            });

            Console.WriteLine("Type [send|exit].");
            bool done = false;
            while (!done)
            {
                var cmd = Console.ReadLine().Trim().ToLower();
                switch (cmd)
                {
                    case "send":
                        {
                            var analitics = services.GetRequiredService<IAnalytics>();
                            Task.Run(async () => await analitics.SendEventAsync(new[] {
                                new GameEventObjectiveProgress(17, 1, Guid.NewGuid(), 123)
                            })).Wait();
                            break;
                        }
                    case "exit":
                        {
                            done = true;
                            break;
                        }
                }
            }
        }


        private static ServiceProvider RegisterServices(string[] args)
        {
            var SERVCE_NAME = "gameevents"; // hack to act in the name of gameevents and use that keyvault.

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(cfg => cfg.AddConsole());

            IConfiguration configuration = new ConfigurationBuilder()
                .AddCreyConfigurations(args, SERVCE_NAME, "Developement", KeyVaultPolicy.All)
                .Build();
            serviceCollection.AddSingleton(configuration);

            serviceCollection.AddEventHubAnalytics();
            return serviceCollection.BuildServiceProvider();
        }
    }
}
