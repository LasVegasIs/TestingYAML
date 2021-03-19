using System;

namespace IAM.Data
{
    public class DBUserSearch
    {
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public string Roles { get; set; }
    }
}
