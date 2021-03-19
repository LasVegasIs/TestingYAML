using Crey.MessageContracts.Avatar;
using Crey.MessageContracts.Bank;
using Crey.MessageContracts.GameEvents;
using Crey.MessageContracts.Prefabs;
using Crey.MessageContracts.UserProfile;
using MessageContracts.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crey.MessageContracts.Rewards
{
    [JsonConverter(typeof(JsonMessageSerdeConverter<Reward>))]
    public abstract class Reward : IValidatableObject
    {
        public string Type => MessageSerdeAttribute.GetSerdeToken(this);

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return Array.Empty<ValidationResult>();
        }

        internal Reward() { }
    }
}