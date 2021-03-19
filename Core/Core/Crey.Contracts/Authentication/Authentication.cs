using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace Crey.Contracts
{
    // role constant for convenience, to convert typo into compile errors
    public static class UserRoles
    {
        public const string FreeUser = "FreeUser";
        public const string Anonymous = "Anonymous";
        public const string InternalUser = "InternalUser";
        public const string ExternalDeveloper = "ExternalDeveloper"; // outsourcing roles

        public const string UserAdmin = "UserAdmin";
        public const string UserStat = "UserStat";

        public const string Subscriber = "Subscriber";
        public const string ContentDev = "ContentDev";
        public const string BetaTester = "BetaTester";
        public const string Moderator = "Moderator";
        public const string DevStage = "DevStage";
        public const string PrefabSupport = "PrefabSupport";

        public const string AvatarManager = "AvatarManager";

        public const string BankManager = "BankManager";
        public const string BankAdmin = "BankAdmin";
        public const string BankStat = "BankStat";

        public const string Muted = "Muted";

        // roles those are not really roles and not present in the DB
        public static List<string> PhantomRoles = new List<string>
        {
            FreeUser, Anonymous, InternalUser
        };
    }

    /// <summary>
    /// Most usable part in service logic.
    /// </summary>
    public class UserInfo
    {
        public int AccountId { get; set; }

        public HashSet<string> Roles { get; set; } = new HashSet<string>();

        public bool IsDeleted { get; set; } = false;
    }

    public class SessionInfo : UserInfo
    {
        public string UserId { get; set; }

        public string Key { get; set; } = "";

        public string AuthenticationMethod { get; set; } = "";

        [Obsolete("Use Roles instead")]
        public AccountRoles RoleMask => Roles.ToAccountRolesMask();

        public bool IsValid => AccountId != 0;
        public bool IsSignedIn => !string.IsNullOrEmpty(Key);


        [Obsolete("Don't use it, only present while not remove from client")]
        public readonly List<string> DownVersions = new List<string> { "main" };
        [Obsolete("Don't use it, only present while not remove from client")]
        public readonly string UpVersion = "main";


        [Obsolete("Will be removed soon")]
        public bool IsGuest => AccountId < 0;
        public bool IsUser => AccountId > 0;

        public List<int> GroupIds => Roles.Contains(UserRoles.Moderator) ? new List<int> { (int)SystemGroups.Moderators } : new List<int>();

        public SessionInfo Clone()
        {
            return new SessionInfo
            {
                AccountId = AccountId,
                UserId = UserId,
                Key = Key.Clone() as string,
                Roles = new HashSet<string>(Roles),
                AuthenticationMethod = AuthenticationMethod
            };
        }

        // Checks if toTest is valid with respect to this
        public bool CheckValidity(SessionInfo toTest)
        {
            if (AccountId != toTest.AccountId)
                return false;
            if (Key != toTest.Key)
                return false;

            return true;
        }
    }

    public class GeoLocation
    {
        public GeoLocation(string yourip, string continentCode, string countryCode, double latitude, double longtitude)
        {
            YourIp = yourip;
            ContinentCode = continentCode;
            CountryCode = countryCode;
            Latitude = latitude;
            Longitude = longtitude;
        }

        public string CountryCode { get; }
        public string ContinentCode { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        /// <summary>
        /// IP used for geolocation.
        /// </summary>
        public string YourIp { get; }
    };

    public class SignInWithEmailParams
    {
        [Required]
        public string SiteId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "EmailOrUsername length should be between 3 and 100.")]
        public string EmailOrUsername { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Password length should be between 3 and 100.")]
        public string Password { get; set; }
    }

    [DataContract]
    public class SignInAsGuestParams
    {
        [DataMember]
        [Required]
        public string SiteId { get; set; }
    }

    [DataContract]
    public class SignInWithKeyParams
    {
        [DataMember]
        [Required]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Key length should be between 10 and 100.")]
        public string Key { get; set; }
    }

    [DataContract]
    public class CheckKeyParams
    {
        [DataMember]
        [Required]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Key length should be between 10 and 100.")]
        public string Key { get; set; }
    }

    [Obsolete("Will be removed soon")]
    public class SignInResult
    {
        [DataMember]
        public SessionInfo SessionInfo { get; set; }

        [DataMember]
        public string IdToken { get; set; }
    }
}
