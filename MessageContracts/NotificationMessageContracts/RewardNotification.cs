using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;

namespace NotificationMessageContracts
{
    [Flags]
    public enum NotificationType
    {
        Silent = 0,
        Offline = 1,
        Online  = 2,
        //Email = 4,
        //SMS = 8, :)
    }

    public class RewardNotification : INotificationMessage
    {
        public string Type => "Reward";

        [Required]
        public string TrackingId { get; set; } // unique id, correlating to the CollectedRewards

        [Required]
        public JObject Reward { get; set; }

        public JObject Trigger { get; set; }
    }
}
