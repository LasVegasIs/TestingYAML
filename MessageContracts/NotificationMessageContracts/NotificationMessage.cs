using MessagingCore;

namespace NotificationMessageContracts
{
    public enum NotificationGroups
    {
        user,
        group
    }

    public static class NotificationGroupIds
    {
        public const int MODERATORS = 1;
    }

    public interface INotificationMessage : IMessageContract
    {
    }
}
