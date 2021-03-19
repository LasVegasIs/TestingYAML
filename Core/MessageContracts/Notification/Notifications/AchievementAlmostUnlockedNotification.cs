using System.Collections.Generic;

namespace Crey.MessageContracts.Notification
{

    [MessageSerde("AchievementAlmostUnlocked")]
    public sealed class AchievementAlmostUnlockedNotification : NotificationPayload
    {
        public string AwardName { get; set; }
        public string DisplayName { get; set; }
        public string Image { get; set; }
        public Dictionary<string, string> Progress { get; set; }
        public string ButtonText { get; set; } = "Awesome!";
    }

    [MessageSerde("AchievementAlmostUnlocked")]
    public sealed class AchievementAlmostUnlockedNotificationMessage : NotificationMessage<AchievementAlmostUnlockedNotification>
    {
    }
}
