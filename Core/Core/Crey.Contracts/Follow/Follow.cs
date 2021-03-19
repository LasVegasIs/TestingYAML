using System.ComponentModel.DataAnnotations;

namespace Crey.Contracts.Follow
{
    public class AccountIdParams
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Account id must be positive.")]
        public int? AccountId { get; set; }
    }

    public struct Follower
    {
        public int AccountId { get; set; }
        public bool IsFollowing { get; set; }
    }

    public struct FollowingResult
    {
        public bool IsFollowing { get; set; }
        public bool IsFollowedBy { get; set; }
    }
}
