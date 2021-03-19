using System;

namespace Crey.MessageContracts.Content
{
    [MessageSerde("AccountPlay")]
    public class UserLevelPlayMessage : ContentUserMessage
    {
        public override int UserId { get; set; }
        public int LevelId { get; set; }
        public uint CountOnLevel { get; set; }
        public uint CountOnAllLevels { get; set; }

        [Obsolete("only for backward compatibility")]
        public int SenderUserId => UserId;
        [Obsolete("only for backward compatibility")]
        public uint PlayedOnLevel => CountOnLevel;
        [Obsolete("only for backward compatibility")]
        public uint TotalPlayCount => CountOnAllLevels;

        public UserLevelPlayMessage()
        {
        }

        public UserLevelPlayMessage(
            int userId,
            int levelId,
            uint countOnLevel,
            uint countOnAllLevels)
        {
            UserId = userId;
            LevelId = levelId;
            CountOnLevel = countOnLevel;
            CountOnAllLevels = countOnAllLevels;
        }
    }
}
