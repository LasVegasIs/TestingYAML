using MessageContracts;
using MessageContracts.Rewards;
using MessageContracts.RewardTriggers;
using Newtonsoft.Json;
using NUnit.Framework;
using NotificationMessageContracts;

namespace RewardMessageContracts.Test
{
    public class SerdeRewardMessage
    {
        [Test]
        public void SerdeEventPointsReward()
        {
            var msg = new EventPointsRewardMessage()
            {
                RewardedAccountId = 123,
                NotificationOptions = new RewardNotificationOptions()
                {
                    TrackingId = "track",
                    NotificationType = NotificationType.Offline | NotificationType.Online,
                    Trigger = new DescriptionTrigger("Hello")
                },
                Reward = new EventPointsReward
                {
                    Amount = 5,
                    EventId = 1
                }
            };

            var msgStr = JsonConvert.SerializeObject(msg);
            var msg2 = JsonConvert.DeserializeObject<EventPointsRewardMessage>(msgStr);

            Assert.True(msg.NotificationOptions.NotificationType.HasFlag(NotificationType.Online));
            Assert.True(msg.NotificationOptions.NotificationType.HasFlag(NotificationType.Offline));
            Assert.True((msg.Reward).Amount == (msg2.Reward).Amount);
            Assert.True(((DescriptionTrigger)msg.NotificationOptions.Trigger).Description == ((DescriptionTrigger)msg2.NotificationOptions.Trigger).Description);
        }
    }
}