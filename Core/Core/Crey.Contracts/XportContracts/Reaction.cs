using System;
using System.Runtime.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Crey.Contracts.XportContracts
{
    public interface IComment
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

    public interface IPartitioned
    {
        string PartitionKey { get; set; }
        string RowKey { get; set; }
    }

    [DataContract]
    public abstract class Partitioned : IPartitioned
    {
        [DataMember]
        [BsonId(IdGenerator = typeof(CombGuidGenerator))]
        public Guid Id { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }

        public abstract void SetupKeys();
    }

    [DataContract]
    public abstract class ContextAndTarget : Partitioned
    {
        public ContextAndTarget(string context, string target)
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
    public class Reaction : ContextAndTarget, IComment
    {
        protected Reaction(string context, string target, int accountId, string body, Guid parent, long commentTags) : base(context, target)
        {
            var now = DateTime.UtcNow;
            Creation = now;
            Modified = now;
            Owner = accountId;
            Body = body;
            Parent = parent;
            CommentTags = commentTags;
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

        [DataMember]
        public long CommentTags { get; set; }

        public override string ToString()
        {
            return $"{Creation.ToShortDateString()} {Context}:{Target} {(Parent == Guid.Empty ? "" : $"{Parent.ToString().Substring(0, 4)}")} - accountId:{Owner} [{Body}] {Id.ToString().Substring(0, 4)}";
        }
    }

    [DataContract]
    public class LevelComment : Reaction
    {
        public LevelComment(string target, int accountId, string body, Guid parent, long commentTags) : base("level", target, accountId, body, parent, commentTags) { }
    }
}