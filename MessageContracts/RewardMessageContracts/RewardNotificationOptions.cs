using Newtonsoft.Json;
using MessageContracts.RewardTriggers;
using System.ComponentModel.DataAnnotations;
using NotificationMessageContracts;

namespace MessageContracts
{
    public class RewardNotificationOptions
    {
        [Required]
        public NotificationType NotificationType { get; set; }
        [Required]
        public string TrackingId { get; set; } // unique id, correlating to the CollectedRewards

        [JsonConverter(typeof(RewardTriggerConverter))]
        public IRewardTrigger Trigger { get; set; }        
    }
}