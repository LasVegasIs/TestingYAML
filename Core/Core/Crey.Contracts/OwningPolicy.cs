using System;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    [DataContract]
    [Flags]
    public enum OwningPolicy
    {
        [EnumMember] Unknown = 0,
        [EnumMember] Free = 1 << 0,          // Free to use
        [EnumMember] Subscriber = 1 << 1,    // Subscriber role required
        [EnumMember] Dev = 1 << 2,           // Devs only        
        [EnumMember] MarketPlace = 1 << 3,   // Requires explicit ownership        
    }
}
