using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Crey.Contracts.XportContracts
{
    public interface IResponse
    {
        Guid Id { get; }
        DateTime Creation { get; }
        Guid Parent { get; } // link to a parent comment entry
        string Context { get; } // "level" "screenshot"
        string Target { get; } // "13" "324F2Q-AAUX7" - levelId or screenShotId - the id of the target entity
        int Owner { get; } // issuer accountid 
        string Body { get; set; } // the raw content
        DateTime Modified { get; set; }
    }

    public interface ICloudStored
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
    }

    [DataContract]
    public abstract class CloudStored : ICloudStored
    {
        // It's NOT a datamember, The rowKey gets created from it
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; protected set; }

        [DataMember]
        public string PartitionKey { get; set; }

        [DataMember]
        public string RowKey { get; set; }

        public abstract void SetupKeys();
    }

    [DataContract]
    public abstract class TargetInContext : CloudStored
    {
        public TargetInContext(string context, string target)
        {
            Id = CombGuidGenerator.Instance.NewCombGuid(Guid.NewGuid(), DateTime.UtcNow);
            Context = context;
            Target = target;
            Init();
        }



        [DataMember]
        public string Context { get; set; } // "level" "screenshot"

        [DataMember]
        public string Target { get; set; } // "13" "324F2Q-AAUX7" - levelId or screenShotId - the id of the target entity

        private void Init()
        {
            SetupKeys();
        }

        public override void SetupKeys()
        {
            PartitionKey = $"{Target}{Context}";
            RowKey = Id.ToString();
        }
    }

    [DataContract]
    public class Response : TargetInContext, IResponse
    {
        protected Response(string context, string target, int accountId, string body, Guid parent) : base(context, target)
        {
            var now = DateTime.UtcNow;
            Creation = now;
            Modified = now;
            Owner = accountId;
            Body = body;
            Parent = parent;
        }

        [DataMember]
        public DateTime Creation { get; set; }

        [DataMember]
        public DateTime Modified { get; set; }

        [DataMember]
        public Guid Parent { get; set; }

        [DataMember]
        public int Owner { get; set; } // issuer accountid 

        [DataMember]
        public string Body { get; set; } // the raw content

        public override string ToString()
        {
            return $"{Creation.ToShortDateString()} {Context}:{Target} {(Parent == Guid.Empty ? "" : $"{Parent.ToString().Substring(0, 4)}")} - accountId:{Owner} [{Body}] {Id.ToString().Substring(0, 4)}";
        }
    }

    [DataContract]
    public class LevelResponse : Response
    {

        public delegate LevelResponse Factory(string target, int accountId, string body, Guid parent);

        public LevelResponse(string target, int accountId, string body, Guid parent) : base("level", target, accountId, body, parent) { }
    }
}