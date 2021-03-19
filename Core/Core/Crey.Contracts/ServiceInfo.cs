using System.Runtime.Serialization;

namespace Crey.Contracts
{
    // moved to standard
    [DataContract]
    public class ServiceInfo
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Changeset { get; set; }

        [DataMember]
        public string Stage { get; set; }

        [DataMember]
        public string DeploymentSlot { get; set; }
    }

    public class ServiceURIResult
    {
        public string AuthServiceAddress { get; set; }
        public string CommentsServiceAddress { get; set; }
        public string LevelsServiceAddress { get; set; }
        public string NotificationServiceAddress { get; set; }
        public string PrefabsServiceAddress { get; set; }
        public string GameResourcesServiceAddress { get; set; }
        public string MatchmakingServiceAddress { get; set; }
        public string AvatarServiceAddress { get; set; }
        public string WebsiteAddress { get; set; }
        public string LauncherDataAddress { get; set; }
    }
}
