using Crey.Contracts.XportEnums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
// https://stackoverflow.com/questions/58138765/compositetoken-in-new-microsoft-azure-cosmos-sdk
using Microsoft.AspNetCore.WebUtilities;

namespace Crey.Contracts
{
    [DataContract]
    public enum TagFilter
    {
        [EnumMember] Ignore = 0,
        [EnumMember] MatchExact,
        [EnumMember] MatchAny,
    }


    [DataContract]
    public class NoData { }

    [DataContract]
    public class ErrorMessage
    {
        public string Error { get; set; }
    }

    [DataContract]
    public class ValueData<ValueType>
    {
        [DataMember]
        public ValueType Value { get; set; }
    }

    public class BinaryContent
    {
        public byte[] Data { get; set; }
        public string ContentHash { get; set; }
        public string MimeType { get; set; }
    }

    [DataContract]
    public class IntegerId
    {
        [DataMember]
        public int Id { get; set; }
    }

    [DataContract]
    public class StringId
    {
        [DataMember]
        public string Id { get; set; }
    }

    [DataContract]
    public class SourceExportParams
    {
        [DataMember]
        public SourceFormat Format { get; set; }
    }

    [DataContract]
    public class MessageString
    {
        [DataMember]
        public string Message { get; set; } = "";
    }

    [DataContract]
    public class ResourceUploadDevParams
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        public string Version { get; set; }

        [DataMember]
        public ResourceType Type { get; set; }

        [DataMember]
        public ResourceKind Kind { get; set; }
    }

    [DataContract]
    public class ResourceUploadMyByNameParams
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public ResourceKind Kind { get; set; }
    }

    [DataContract]
    public class ResourceDeployVersionParams
    {
        [DataMember]
        public string SourceVersion { get; set; }

        [DataMember]
        public string TargetVersion { get; set; }

        [DataMember(IsRequired = false)]
        public List<string> ResourceFilter { get; set; }
    }

    [DataContract]
    public class ResourceSetVersionParams
    {
        [DataMember]
        public List<string> DownloadVersions { get; set; } = new List<string>();

        [DataMember]
        public string UploadVersion { get; set; }
    }

    [DataContract]
    public class IntIdList
    {
        [DataMember]
        public List<int> Ids { get; set; }
    }

    [DataContract]
    public class StringIdList
    {
        [DataMember]
        public List<string> Ids { get; set; }
    }

    [DataContract]
    public class ResourceByNameWithPrefsParam
    {
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        public string Hash { get; set; }

        [DataMember]
        public bool ReturnChunk { get; set; }
    }

    [DataContract]
    public class CommentAddParam
    {
        private const int MaxCommentLength = 2048;


        [DataMember]
        public int LevelId { get; set; }

        [DataMember]
        public string Body { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (Body.Length > MaxCommentLength) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class CommentDeleteParam
    {
        [DataMember]
        public int LevelId { get; set; }

        [DataMember]
        public Guid CommentId { get; set; }
    }

    [DataContract]
    public class CommentUpdateParam
    {
        private const int MaxCommentLength = 2048;

        [DataMember]
        public int LevelId { get; set; }

        [DataMember]
        public Guid CommentId { get; set; }

        [DataMember]
        public string Body { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (Body.Length > MaxCommentLength) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    #region Leaderboard
    [DataContract]
    public class LeaderboardAddParam
    {
        [DataMember(IsRequired = true)]
        public int LevelId { get; set; }

        [DataMember(IsRequired = true)]
        public int Score { get; set; }

        [DataMember(IsRequired = true)]
        public int TimeMsec { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (LevelId <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class LeaderboardListByLevelParam
    {
        [DataMember(IsRequired = true)]
        public int LevelId { get; set; }

        [DataMember(IsRequired = true)]
        public int OffsetResults { get; set; }

        [DataMember(IsRequired = true)]
        public int MaxResults { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (LevelId <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (OffsetResults < 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (MaxResults <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class LeaderboardGetSingleScoreParam
    {
        [DataMember(IsRequired = true)]
        public int LevelId { get; set; }

        [DataMember(IsRequired = true)]
        public int AccountId { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (LevelId <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (AccountId == 0) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class LeaderboardGetRankParam
    {
        [DataMember(IsRequired = true)]
        public int LevelId { get; set; }

        [DataMember(IsRequired = true)]
        public int AccountId { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (LevelId <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (AccountId == 0) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }
    #endregion

    #region Badge
    [DataContract]
    public class BadgeAddParam
    {
        [DataMember]
        public int LevelId { get; set; }

        [DataMember]
        public string BadgeGuid { get; set; }

        [DataMember]
        public string Icon { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int CountRequired { get; set; }

        [DataMember]
        public int Count { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (LevelId <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (BadgeGuid.Length == 0) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class BadgeListByLevelAndAccountParam
    {
        [DataMember(IsRequired = true)]
        public int LevelId { get; set; }

        [DataMember(IsRequired = true)]
        public int AccountId { get; set; }

        [OnDeserialized]
        void OnDeserialized(StreamingContext c)
        {
            if (LevelId <= 0) throw new CreyException(ErrorCodes.InvalidArgument);
            if (AccountId == 0) throw new CreyException(ErrorCodes.InvalidArgument);
        }
    }

    [DataContract]
    public class LevelBadgeListParam
    {
        [DataMember(IsRequired = true)]
        [Range(1, int.MaxValue)]
        public int LevelId { get; set; }

        [DataMember(IsRequired = true)]
        public bool IsPublic { get; set; }
    }
    #endregion

    #region Awards
    [DataContract]
    public class AwardCheckByNameParam
    {
        [DataMember]
        public string AwardName { get; set; }
    }
    #endregion


    // note: moved to net50
    public class PagedListResult<T>
    {
        public List<T> Items { get; set; }
        public string ContinuationToken { get; set; }
    }

    // note: moved to net50
    public class OffsetBasedContinuationToken
    {
        public OffsetBasedContinuationToken(string token)
        {
            int offset;
            int.TryParse(token, out offset);
            Offset = offset;
        }

        public OffsetBasedContinuationToken()
        {
            // Default constructor
        }

        public int Offset { get; set; }

        public bool IsContinuation()
        {
            return Offset > 0;
        }

        public override string ToString()
        {
            return Offset.ToString();
        }

        public string IntoToken()
        {
            return ToString();
        }
    }

    // note: moved to net50
    public class CosmosTableBasedContinuationToken
    {

        public Microsoft.Azure.Cosmos.Table.TableContinuationToken TableCursor { get; set; }

        public CosmosTableBasedContinuationToken(string token)
        {
            TableCursor = token?
                            .To(WebEncoders.Base64UrlDecode)
                            .To(Encoding.UTF8.GetString)
                            .To(JsonConvert.DeserializeObject<Microsoft.Azure.Cosmos.Table.TableContinuationToken>);
        }

        public CosmosTableBasedContinuationToken()
        {
        }

        public override string ToString()
        {
            return TableCursor?
                        .To(JsonConvert.SerializeObject)
                        .To(Encoding.UTF8.GetBytes)
                        .To(WebEncoders.Base64UrlEncode);
        }

        public bool IsContinuation => TableCursor != null && (!string.IsNullOrEmpty(TableCursor.NextPartitionKey) || !string.IsNullOrEmpty(TableCursor.NextRowKey));
    }

    [Obsolete("Use CosmosTableBasedContinuationToken")]
    public class CloudTableBasedContinuationToken
    {
        public Microsoft.WindowsAzure.Storage.Table.TableContinuationToken TableCursor { get; set; }

        public CloudTableBasedContinuationToken(string token)
        {
            if (token != null)
                TableCursor = JsonConvert.DeserializeObject<Microsoft.WindowsAzure.Storage.Table.TableContinuationToken>(token.Replace("'", "\""));
        }

        public CloudTableBasedContinuationToken()
        {
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(TableCursor);
        }

        public string IntoToken()
        {
            if (TableCursor == null)
            {
                return null;
            }

            // to avoid double escaping of the \"
            return ToString().Replace("\"", "'");
        }

        public bool IsContinuation()
        {
            return TableCursor != null && !string.IsNullOrEmpty(TableCursor.NextPartitionKey) && !string.IsNullOrEmpty(TableCursor.NextRowKey);
        }
    }


    [Obsolete("use CloudTableBasedContinuationToken instead")]
    public class TableBasedContinuationToken
    {
        public string NextPartitionKey { get; set; }
        public string NextRowKey { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(NextPartitionKey) && !string.IsNullOrEmpty(NextRowKey);
        }
    }
}