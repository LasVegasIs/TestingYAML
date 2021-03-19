using Crey.Contracts.XportContracts;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    /// <summary>
    ///     Note: null wont get into the json
    /// </summary>
    [Obsolete]
    [DataContract]
    public class DemoOptionalText
    {
        /// <summary>
        ///     * This is the point where we can inject data into the json optionally *
        ///     At contract export time extra info can be emitted based on local conditons
        ///     SerializeBegin
        ///     case ExportMode ... slim, fat, includetypes, etc
        ///     The trick Setting it to null won't export anything
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Info { get; set; }
    }

    [Obsolete]
    [DataContract]
    public class Error
    {
        [DataMember]
        public ErrorCodes ErrorCode { get; set; }
        [DataMember]
        public string Message { get; set; }

        public Error()
        {
            ErrorCode = ErrorCodes.NoError;
            Message = "";
        }

        public static Error NoError => new Error();

        public Error(ErrorCodes errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }

        public bool IsNoError => ErrorCode == ErrorCodes.NoError;

        void Throw()
        {
            ErrorCode.Throw(Message);
        }
    }

    [DataContract]
    public class Error<T> : Error
    {
        [DataMember]
        public T Detail { get; set; }

        public static new Error<T> NoError => new Error<T>();

        public Error()
            : base()
        {
        }

        public Error(ErrorCodes errorCode, string message, T detail)
            : base(errorCode, message)
        {
            Detail = detail;
        }
    }

    [DataContract]
    public class SourceExportResult
    {
        [DataMember]
        public string Enums { get; set; }
    }

    [DataContract]
    public class ValueResult<ValueType>
    {
        [DataMember]
        public ValueType Value { get; set; }
    }

    [DataContract]
    public class TimeResult
    {
        [DataMember]
        public ulong Seconds { get; set; }
    }

    [DataContract]
    public class ResourceSetVersionResult
    {
        [DataMember]
        public List<string> DownloadVersions { get; set; } = new List<string>();

        [DataMember]
        public string UploadVersion { get; set; }
    }

    [DataContract]
    public class SingleResourceResult
    {
        [DataMember]
        public ResourceInfo ResourceInfo { get; set; }
    }

    [DataContract]
    public class StringKeyListResult
    {
        [DataMember]
        public List<string> Keys { get; set; } = new List<string>();
    }

    [DataContract]
    public class ResourceListResult
    {
        [DataMember]
        public List<ResourceInfo> Resources { get; set; } = new List<ResourceInfo>();

        public static string NameOfList => nameof(Resources);
    }

    [DataContract]
    public class ResourceCheckIntegrityResult
    {
        [DataMember]
        public List<string> Errors { get; set; } = new List<string>();
    }


    [DataContract]
    public class PokeResult
    {
        [DataMember]
        public int NumberA { get; set; }

        [DataMember]
        public int NumberB { get; set; }

        [DataMember]
        public string Text { get; set; }

        [DataMember]
        public string Payload { get; set; }
    }


    [DataContract]
    public class SingleCommentResult
    {
        [DataMember]
        public LevelComment Comment { get; set; }
    }

    [DataContract]
    public class CommentListResult
    {
        [DataMember]
        public List<LevelComment> Comments { get; set; }
    }

    #region Leaderboard
    [DataContract]
    public class SingleLeaderboardResult
    {
        [DataMember]
        public LeaderboardScore LeaderboardScore { get; set; }

        [DataMember]
        public bool UseTime { get; set; }   // True if the time value of the leaderboardScore is relevant (ordering goes by time)
    }

    [DataContract]
    public class LeaderboardGetRankResult
    {
        [DataMember]
        public int Rank { get; set; }
    }
    #endregion

    #region Rewards

    [DataContract]
    public class AwardCheckByNameResult
    {
        [DataMember]
        public bool AwardGiven { get; set; }
    }
    #endregion

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Crey.Contracts.XportResults", Name = "WelcomeResult")]  // remove namespace after some live deploy
    public class WelcomeResult
    {
        public WelcomeResult()
        {
#if DEBUG
            DevMode = true;
#endif
            ServerTime = DateTime.UtcNow;
            Machine = Environment.MachineName;
        }

        public void Init(string stage, string version)
        {
            Stage = stage;
            Version = version;
        }


        public const uint WELCOMEREQID = 0; // critically important => the client explicitly waits for this

        [DataMember]
        public string Stage { get; set; }

        [DataMember]
        public bool DevMode { get; set; }

        [DataMember]
        public DateTime ServerTime { get; set; }

        [DataMember]
        public string Version { get; set; } // format: 1.0.0  - semantic versioning - get it from assembly Crey

        [DataMember]
        public string IncompatibleClientMsg { get; set; } = "The CREY launcher has been updated. Please download it from http://bit.ly/Crey_installer . ";

        [DataMember]
        public string Machine { get; set; }

        public string Info()
        {
            return $"machine:[{Machine}] relay version:[{Version}] stage:[{Stage}]";
        }
    }
}
