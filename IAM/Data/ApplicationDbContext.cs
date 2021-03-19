using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IAM.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public virtual DbSet<AuthToken> AuthToken { get; set; }
        public virtual DbSet<DBSessionToken> SessionTokens { get; set; }
        public virtual DbSet<DBSingleAccessKey> SingleAccessKeys { get; set; }
        public virtual DbSet<DBPersistentToken> PersistentTokens { get; set; }
        public virtual DbSet<DBUserData> UserDatas { get; set; }
        public virtual DbSet<DBSoftDeletedUserAccount> SoftDeletedUserAccounts { get; set; }
        public virtual DbSet<DBHardDeletedUserAccount> HardDeletedUserAccounts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AuthToken>(entity =>
            {
                entity.HasKey(e => new { e.AccountId, e.SiteId });
                entity.HasIndex(e => new { e.AccountId });
                entity.HasIndex(e => new { e.Token });
                entity.HasIndex(e => new { e.SiteId, e.Token });
            });

            builder.Entity<DBSessionToken>(entity =>
            {
                entity.HasIndex(e => new { e.AccountId });
                entity.HasIndex(e => new { e.Token }).IsUnique();
            });

            builder.Entity<DBSingleAccessKey>(entity =>
            {
                entity.HasIndex(e => new { e.AccountId });
                entity.HasIndex(e => new { e.Key }).IsUnique();
            });

            builder.Entity<DBUserSearch>(entity => {
                entity.HasNoKey();
                entity.ToView(nameof(DBUserSearch));
            });
        }
    }
}
