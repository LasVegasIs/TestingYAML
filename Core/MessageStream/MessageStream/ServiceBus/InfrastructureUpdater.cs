using Crey.MessageContracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Core.MessageStream.ServiceBus
{
    //todo: queue and topic might require a separate class
    public class InfrastructureUpdater
    {
        private const string DateTimeFormat = "yyyy.MM.dd";
        private IConfiguration _configuration;
        private ManagementClient _managementClient;
        public InfrastructureUpdater(IConfiguration configuration)
        {
            _configuration = configuration;
            _managementClient = new ManagementClient(configuration.GetServiceBusConnectionString());
        }

        public async Task CreateQueueAsync(string queuePath)
        {
            try
            {
                var channelName = _configuration.GetServiceBusChannelName(ChannelType.PointToPoint, queuePath);
                await _managementClient.CreateQueueAsync(channelName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // noop
            }
        }

        public async Task CreateTopicAsync(string topicPath)
        {
            try
            {
                var channelName = _configuration.GetServiceBusChannelName(ChannelType.PublishSubscribe, topicPath);
                await _managementClient.CreateTopicAsync(channelName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // noop
            }
        }

        public async Task CreateOrUpdateSubscriptionAsync<TMessageType>(
            string connectionString,
            string channelName,
            string subscriptionName,
            string topicPath,
            IMessageHandler<TMessageType> messageHandler)
            where TMessageType : class, IMessageContract
        {
            var ruleDescription = GetRuleDescriptionFromTypeMap(subscriptionName, topicPath, messageHandler);

            try
            {
                var subscriptionDescription = new SubscriptionDescription(channelName, subscriptionName);
                await _managementClient.CreateSubscriptionAsync(subscriptionDescription, ruleDescription);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                await UpdateSubscriptionRules(channelName, subscriptionName, ruleDescription, messageHandler);
            }
        }

        private async Task UpdateSubscriptionRules<TMessageType>(
            string channelName,
            string subscriptionName,
            RuleDescription ruleDescription,
            IMessageHandler<TMessageType> messageHandler)
            where TMessageType : class, IMessageContract
        {
            var currentRules = await _managementClient.GetRulesAsync(channelName, subscriptionName);
            if (currentRules.All(rule => ParseRuleName(rule) < messageHandler.CurrentVersion))
            {
                try
                {
                    // First we add a rule so new messages can qualify
                    await _managementClient.CreateRuleAsync(channelName, subscriptionName, ruleDescription);
                }
                catch (MessagingEntityAlreadyExistsException)
                {
                    // noop
                }

                // Remove each rule that are older than the one we just added
                currentRules = await _managementClient.GetRulesAsync(channelName, subscriptionName);
                foreach (var rule in currentRules)
                {
                    if (ParseRuleName(rule) < messageHandler.CurrentVersion)
                    {
                        try
                        {
                            await _managementClient.DeleteRuleAsync(channelName, subscriptionName, rule.Name);
                        }
                        catch (MessagingEntityNotFoundException)
                        {
                            // noop
                        }
                    }
                }
            }
        }

        private RuleDescription GetRuleDescriptionFromTypeMap<TMessageType>(
            string subscriptionName,
            string topicPath,
            IMessageHandler<TMessageType> messageHandler)
            where TMessageType : class, IMessageContract
        {
            var service = $"{subscriptionName}-{topicPath}";
            var typeFilters = string.Join(",", messageHandler.HandledTypes.Select(type => $"'{type}'"));
            var compiledFilterString = $"(To = '{service}' OR To IS NULL) AND (Type IN ({typeFilters}))";

            return new RuleDescription(
                messageHandler.CurrentVersion.ToString(DateTimeFormat),
                new SqlFilter(compiledFilterString));
        }

        private DateTime ParseRuleName(RuleDescription rule)
        {
            return DateTime.TryParseExact(rule.Name, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var result)
                ? result
                : DateTime.MinValue;
        }
    }
}
