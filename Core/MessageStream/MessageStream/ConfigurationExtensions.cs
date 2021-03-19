using Microsoft.Extensions.Configuration;
using System.Linq;
using Core.MessageStream.ServiceBus;

namespace Core.MessageStream
{
    public static class ConfigurationExtensions
    {
        #region Should be in some shared lib
        public static string GetDeploymentSlot(this IConfiguration configuration)
        {
            var slot = configuration.GetValue<string>("DeploymentSlot")?.ToLower()
                ?? throw new InternalErrorException($"Missing deploymentSlot");

            if (slot.Any(x => !char.IsLetter(x)))
            {
                throw new InternalErrorException($"Invalid slot name, slot can contain only alpha (letter) characters and should be lowercase, provided: {slot}");
            }
            return slot;
        }

        public static string GetChangeSetIdentifier(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("Changeset");
        }
        #endregion Should be in some shared lib

        public static string GetServiceBusChannelName(this IConfiguration configuration, ChannelType channelType, string channel)
        {
            var deploymentSlot = configuration.GetDeploymentSlot();
            switch (channelType)
            {
                case ChannelType.PublishSubscribe: return $"{deploymentSlot}-{channel}";
                case ChannelType.PointToPoint: return $"{deploymentSlot}-queue-{channel}";
                default: throw new InternalErrorException($"Unkown channel type: {channelType}");
            }
        }

        public static bool IsCodeFirstServiceBus(this IConfiguration configuration)
        {
            return configuration.GetValue<bool>("CodeFirstServiceBus", false);
        }

        public static string GetServiceBusConnectionString(this IConfiguration configuration)
        {
            return configuration.GetValue<string>($"{configuration.GetDeploymentSlot()}-creybus");
        }
    }
}
