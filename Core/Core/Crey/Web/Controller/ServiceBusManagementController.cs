using Crey.Configuration.ConfigurationExtensions;
using Crey.Contracts;
using Crey.Kernel.Authentication;
using Crey.MessageStream;
using Crey.MessageStream.ServiceBus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Crey.Web.Controllers
{
    [EnableCors]
    [AuthenticateStrict]
    [Authorize(Policy = CreyAuthorizationDefaults.CreyUser, Roles = UserRoles.InternalUser)]
    public class ServiceBusManagementController : Controller
    {
        public ServiceBusManagementController()
        {
        }

        public class Topic
        {
            public string Path { get; set; }
            public bool IsUsedAsSender { get; set; }
            public bool IsUsedAsReceiver { get; set; }
        }

        public class ServiceBusResourceResult
        {
            public Topic Topic { get; set; }
            public IEnumerable<string> Subscriptions { get; set; }
        }

        [HttpGet("/api/v1/servicebus/resources/topics")]
        [HttpGet("/api/v1/servicebus/resources/subscriptions")]
        public async Task<IEnumerable<ServiceBusResourceResult>> GetServiceBusTopicsAsync(
            [FromServices] IConfiguration configuration,
            [FromQuery] bool usedOnly)
        {
            var connectionString = configuration.GetServiceBusConnectionString();
            var managementClient = new ManagementClient(connectionString);
            var topics = await managementClient.GetTopicsAsync();
            var topicsUsedAsSender = GetChannelsUsed<ISentServiceBusMessage, ServiceBusTopicAttribute>(configuration);
            var topicsUsedAsReceiver = GetChannelsUsed<IReceivedServiceBusMessage, ServiceBusTopicAttribute>(configuration);

            var result = new List<ServiceBusResourceResult>();

            if (usedOnly)
            {
                topics = topics.Where(x => topicsUsedAsSender.Contains(x.Path) || topicsUsedAsReceiver.Contains(x.Path)).ToList();
            }

            foreach (var topic in topics)
            {
                var subscriptions = await managementClient.GetSubscriptionsAsync(topic.Path);
                result.Add(new ServiceBusResourceResult
                {
                    Topic = new Topic
                    {
                        Path = topic.Path,
                        IsUsedAsSender = topicsUsedAsSender.Contains(topic.Path),
                        IsUsedAsReceiver = topicsUsedAsReceiver.Contains(topic.Path)
                    },
                    Subscriptions = subscriptions.Select(x => x.SubscriptionName),
                });
            }

            return result;
        }

        public class Queue
        {
            public string Path { get; set; }
            public bool IsUsedAsSender { get; set; }
            public bool IsUsedAsReceiver { get; set; }
        }

        [HttpGet("/api/v1/servicebus/resources/queues")]
        public async Task<IEnumerable<Queue>> GetServiceBusQueuesAsync(
            [FromServices] IConfiguration configuration,
            [FromQuery] bool usedOnly)
        {
            var connectionString = configuration.GetServiceBusConnectionString();
            var managementClient = new ManagementClient(connectionString);
            var queues = await managementClient.GetQueuesAsync();
            var queuesUsedAsSender = GetChannelsUsed<ISentServiceBusMessage, ServiceBusQueueAttribute>(configuration);
            var queuesUsedAsReceiver = GetChannelsUsed<IReceivedServiceBusMessage, ServiceBusQueueAttribute>(configuration);

            var result = new List<Queue>();

            if (usedOnly)
            {
                queues = queues.Where(x => queuesUsedAsSender.Contains(x.Path) || queuesUsedAsReceiver.Contains(x.Path)).ToList();
            }

            foreach (var queue in queues)
            {
                result.Add(new Queue
                {
                    Path = queue.Path,
                    IsUsedAsSender = queuesUsedAsSender.Contains(queue.Path),
                    IsUsedAsReceiver = queuesUsedAsReceiver.Contains(queue.Path)
                });
            }

            return result;
        }

        private IEnumerable<string> GetChannelsUsed<TMessage, TChannelAttribute>(IConfiguration configuration)
            where TMessage : IStreamedMessage
            where TChannelAttribute : Attribute
        {
            var assembly = Assembly.GetEntryAssembly();
            return assembly
                .GetTypes()
                .Where(x => typeof(TMessage).IsAssignableFrom(x) && x.IsInterface && x.GetCustomAttributes<TChannelAttribute>().Any())
                .Select(x =>
                {
                    var attributes = x.GetCustomAttributes<ServiceBusTopicAttribute>();
                    return $"{configuration.GetDeploymentSlot()}-{attributes.First().Topic}";
                })
                .ToList();
        }
    }

}