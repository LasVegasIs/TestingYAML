namespace Crey.MessageContracts.Notification
{
        [MessageSerde("AchievementUnlocked")]
    public sealed class AchievementUnlockedNotification : NotificationPayload
    {
        public string AwardName { get; set; }
        public string DisplayName { get; set; }
        public string Image { get; set; }
        public string ButtonText { get; set; } = "Awesome!";
    }


    [MessageSerde("AchievementUnlocked")]
    public sealed class AchievementUnlockedNotificationMessage : NotificationMessage<AchievementUnlockedNotification>
    {
    }
}
