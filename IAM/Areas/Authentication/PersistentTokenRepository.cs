using Core.Extensions.BulkDBContext;
using Crey.Exceptions;
using IAM.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class PersistentTokenRepository
    {
        private readonly IConfiguration configuration_;
        private readonly ApplicationDbContext appDBContext_;
        private readonly ILogger logger_;

        public PersistentTokenRepository(
            ILogger<PersistentTokenRepository> logger,
            IConfiguration configuration,
            ApplicationDbContext appDBContext)
        {
            logger_ = logger;
            configuration_ = configuration;
            appDBContext_ = appDBContext;
        }

        public async Task<int> FindUserByPersistentToken(string token)
        {
            var developerToken = await appDBContext_.PersistentTokens
                .FirstOrDefaultAsync(dbPersistentToken => dbPersistentToken.Token == token && dbPersistentToken.Revoked == null)
                .ThrowIfNullAsync(() => throw new AccountNotFoundException($"Token {token} not found or expired"));

            return developerToken.AccountId;
        }

        public async Task<string> CreatePersistentToken(int accountId)
        {
            if (accountId <= 0)
                throw new InvalidArgumentException($"Invalid user id");

            var key = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var token = GenerateToken();
                    await db.PersistentTokens.AddAsync(new DBPersistentToken
                    {
                        AccountId = accountId,
                        Token = token,
                        Issued = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                    return TransactionResult.Done<string>(token);
                },
                (db, tr, c) => Task.FromResult(TransactionResult.Retry<string>()),
                (db) => Task.FromResult("")
                );

            if (string.IsNullOrEmpty(key))
            {
                throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Failed to generate developer token");
            }

            return key;
        }

        public async Task RevokePersistentToken(int accountId, string token)
        {
            // we ignore any row version, just update the revoke as it is a very strong request that shall not fail.

            var rows = await appDBContext_.Database.ExecuteSqlRawAsync(
                    "update PersistentTokens Set Revoked = @now where accountId = @accountId and token = @token and Revoked is null",
                    new SqlParameter("now", DateTime.UtcNow),
                    new SqlParameter("accountId", accountId),
                    new SqlParameter("token", token));

            if (rows == 0)
                throw new AccountNotFoundException($"Token not found or expired");
        }

        private string GenerateToken()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[32];
            provider.GetBytes(byteArray);
            return "4" + Convert.ToBase64String(byteArray);
        }
    }
}