using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.Notification
{
    [MessageSerde("Reward")]
    public sealed class DeprecatedRewardNotification : NotificationPayload
    {
        [Required]
        public string TrackingId { get; set; } // unique id, correlating to the CollectedRewards

        [Required]
        public JObject Reward { get; set; }

        public JObject Trigger { get; set; }
    }

    [MessageSerde("Reward")]
    public sealed class DeprecatedRewardNotificationMessage : NotificationMessage<DeprecatedRewardNotification>
    {
    }
}
