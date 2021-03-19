using System.Runtime.Serialization;

namespace Crey.Contracts
{
    [DataContract]
    public enum TutorialProgressValue
    {
        [EnumMember] NotStarted,
        [EnumMember] HelloWorldFinished,
        [EnumMember] FirstGameFinished,
        [EnumMember] NewGameStarted
    }

    [DataContract]
    public class TutorialProgressInfo
    {
        [DataMember]
        public TutorialProgressValue ProgressValue { get; set; }
        [DataMember]
        public int HelloWorldLevelId { get; set; }
        [DataMember]
        public int FirstGameLevelId { get; set; }
    }
}
