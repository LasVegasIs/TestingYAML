using Core.Extensions.BulkDBContext;
using Crey.Exceptions;
using IAM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication
{
    public class SingleAccessKeyRepository
    {
        internal static readonly TimeSpan KEY_EXPIRATION = TimeSpan.FromMinutes(15);

        private readonly IConfiguration configuration_;
        private readonly ILogger logger_;
        private readonly ApplicationDbContext appDBContext_;

        public SingleAccessKeyRepository(
            IConfiguration configuration,
            ILogger<SessionRepository> logger,
            ApplicationDbContext appDBContext)
        {
            configuration_ = configuration;
            logger_ = logger;
            appDBContext_ = appDBContext;
        }

        public async Task<string> CreateKey(int accountId)
        {
            if (accountId <= 0)
                throw new InvalidArgumentException($"Invalid user id");

            var key = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var token = GenerateToken();
                    await db.SingleAccessKeys.AddAsync(new DBSingleAccessKey
                    {
                        AccountId = accountId,
                        Key = token,
                        Issued = DateTime.UtcNow,
                    });

                    await db.SaveChangesAsync();
                    return TransactionResult.Done<string>(token);
                },
                (db, tr, c) => Task.FromResult(TransactionResult.Retry<string>()),
                (db) => Task.FromResult("")
                );

            if (string.IsNullOrEmpty(key))
            {
                throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Failed to create SA token");
            }

            return key;
        }
        public async Task<int> FindUserByKey(string token)
        {
            var bestBefore = DateTime.UtcNow - KEY_EXPIRATION;
            var account = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var session = await (from sessionToken in appDBContext_.SingleAccessKeys
                                         where sessionToken.Key == token && sessionToken.Used == null && sessionToken.Issued > bestBefore
                                         select sessionToken)
                          .FirstOrDefaultAsync();

                    if (session == null)
                        return TransactionResult.Done<object>(null);

                    session.Used = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                    return TransactionResult.Done<object>(session.AccountId);
                },
                (db, tr, c) => Task.FromResult(TransactionResult.Retry<object>()),
                (db) => Task.FromResult((object)null)
            );

            if (account == null)
                throw new AccountNotFoundException($"SA token not found or expired");

            return (int)account;
        }

        private string GenerateToken()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[32];
            provider.GetBytes(byteArray);
            return "3" + Convert.ToBase64String(byteArray); // prefix is used to differentiat from session key and catch client errors faster
        }
    }
}
