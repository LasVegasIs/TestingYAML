using System.Runtime.Serialization;

namespace Crey.Contracts
{

    ///             Important: Add new stuff strictly to the end !

    [DataContract]
    public enum ChunkType
    {
        [EnumMember] RawBinary,
        [EnumMember] Rc4StreamKey,
        [EnumMember] CryptoControl,
        [EnumMember] JsonError,         // request error
        [EnumMember] Json,
        [EnumMember] JsonParameters,    // request parameters
        [EnumMember] JsonResult,        // request result
        [EnumMember] Png8bit,           // just to avoid rawbinary todo: remove
        [EnumMember] GameResource,      // just to avoid rawbinary todo: remove
    }
}