using Core.Extensions.BulkDBContext;
using Crey.Exceptions;
using IAM.Data;
using Microsoft.Data.SqlClient;
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
    public class SessionToken
    {
        public string Token { get; set; }
        public CredentialType CredentialType { get; set; }
    }

    public class SessionTokenRepository
    {
        internal static readonly TimeSpan TOKEN_EXPIRATION = TimeSpan.FromHours(1);

        private readonly IConfiguration configuration_;
        private readonly ILogger logger_;
        private readonly ApplicationDbContext appDBContext_;

        public SessionTokenRepository(
            IConfiguration configuration,
            ILogger<SessionTokenRepository> logger,
            ApplicationDbContext appDBContext)
        {
            configuration_ = configuration;
            logger_ = logger;
            appDBContext_ = appDBContext;
        }

        public async Task<string> CreateToken(int accountId, SiteInfo siteInfo, CredentialType credential)
        {
            if (accountId <= 0)
                throw new InvalidArgumentException($"Invalid user id");

            var key = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var token = GenerateToken();
                    await db.SessionTokens.AddAsync(new DBSessionToken
                    {
                        AccountId = accountId,
                        Credential = credential,
                        Token = token,
                        UserAgent = siteInfo.UserAgent ?? "",
                        Ip = siteInfo.Ip,
                        Country = siteInfo.Country,
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
                throw new HttpStatusErrorException(HttpStatusCode.Conflict, $"Failed to generate login key");
            }

            return key;
        }

        public async Task<int> RefreshToken(string token, SiteInfo siteInfo)
        {
            var account = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var session = await (from sessionToken in appDBContext_.SessionTokens
                                         where sessionToken.Token == token && sessionToken.Revoked == null
                                         select sessionToken)
                          .FirstOrDefaultAsync();

                    if (session == null)
                        return TransactionResult.Done<object>(null);

                    if (!session.Check(siteInfo))
                    {
                        session.Revoked = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return TransactionResult.Done<object>(null);
                    }

                    session.LastRefreshed = DateTime.UtcNow;
                    session.RefreshCount += 1;
                    await db.SaveChangesAsync();
                    return TransactionResult.Done<object>(session.AccountId);
                },
                (db, tr, c) => Task.FromResult(TransactionResult.Retry<object>()),
                (db) => Task.FromResult((object)null)
            );

            if (account == null)
                throw new AccountNotFoundException($"Token not found or expired");

            return (int)account;
        }

        public async Task<int> FindUserByToken(string token, SiteInfo siteInfo)
        {
            var account = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var session = await (from sessionToken in appDBContext_.SessionTokens
                                         where sessionToken.Token == token && sessionToken.Revoked == null
                                         select sessionToken)
                          .FirstOrDefaultAsync();

                    if (session == null)
                        return TransactionResult.Done<object>(null);

                    if (!session.Check(siteInfo))
                    {
                        session.Revoked = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return TransactionResult.Done<object>(null);
                    }

                    return TransactionResult.Done<object>(session.AccountId);
                },
                (db, tr, c) => Task.FromResult(TransactionResult.Retry<object>()),
                (db) => Task.FromResult((object)null)
            );

            if (account == null)
                throw new AccountNotFoundException($"Token not found or expired");

            return (int)account;
        }

        public async Task<DBSessionToken> FindUserSessionByToken(string token, SiteInfo siteInfo)
        {
            var session = await
                   (from sessionToken
                    in appDBContext_.SessionTokens
                    where sessionToken.Token == token && sessionToken.Revoked == null
                    select sessionToken)
                    .FirstOrDefaultAsync()
                    .ThrowIfNullAsync(() => throw new AccountNotFoundException($"Token {token} not found or expired"));
            session.Check(siteInfo);
            return session;
        }

        public async Task<SessionToken> FindSessionTokenByUser(int account, SiteInfo siteInfo)
        {
            var token = await appDBContext_.RetryTransactionFuncAsync(5,
                async (db, tr) =>
                {
                    var session = await (from sessionToken in appDBContext_.SessionTokens
                                         where sessionToken.AccountId == account && sessionToken.Revoked == null
                                         select sessionToken)
                          .FirstOrDefaultAsync();

                    if (session == null)
                        return TransactionResult.Done<object>(null);

                    if (!session.Check(siteInfo))
                    {
                        session.Revoked = DateTime.UtcNow;
                        await db.SaveChangesAsync();
                        return TransactionResult.Done<object>(null);
                    }

                    return TransactionResult.Done<object>(new SessionToken
                    {
                        Token = session.Token,
                        CredentialType = session.Credential
                    });
                },
                (db, tr, c) => Task.FromResult(TransactionResult.Retry<object>()),
                (db) => Task.FromResult((object)null)
            );

            if (token == null)
                throw new AccountNotFoundException($"Account not found or its token is expired");

            return (SessionToken)token;
        }

        public async Task RevokeToken(string token)
        {
            // we ignore any row version, just update the revoke as it is a very strong request that shall not fail.

            var rows = await appDBContext_.Database.ExecuteSqlRawAsync(
                    "update SessionTokens Set Revoked = @now where token = @token and Revoked is null",
                    new SqlParameter("now", DateTime.UtcNow),
                    new SqlParameter("token", token));

            if (rows == 0)
                throw new AccountNotFoundException($"Token not found or expired");
        }

        public async Task RevokeAllTokens(int accountId, string activeToken)
        {
            //find active tokoens
            var tokens = (from sessionToken in appDBContext_.SessionTokens
                          where sessionToken.AccountId == accountId && sessionToken.Revoked == null
                          select sessionToken)
                        .Select(x => new { x.AccountId, x.Token })
                        .ToList();

            //check consistency
            if (tokens.Find(x => x.Token == activeToken) == null)
                throw new AccountNotFoundException($"Token not found or expired");

            //invalidate tokens one-by-one. 
            // If new token is generated meanwhile, keep them
            // Don't invalidate the "original" token, make it the last to handle error in client and have a chance to retry using the original credentials
            foreach (var t in tokens)
            {
                if (t.Token == activeToken)
                    continue;
                try
                {
                    await RevokeToken(t.Token);
                }
                catch (AccountNotFoundException)
                {
                    //ignore it, already revoked
                }
            }

            //and revoke the active one as well
            try
            {
                await RevokeToken(activeToken);
            }
            catch (AccountNotFoundException)
            {
                //ignore it, already revoked
            }
        }

        public async Task RevokeAllTokensAsync(int accountId)
        {
            //find active tokoens
            var tokens = (from sessionToken in appDBContext_.SessionTokens
                          where sessionToken.AccountId == accountId && sessionToken.Revoked == null
                          select sessionToken)
                        .Select(x => new { x.AccountId, x.Token })
                        .ToList();

            foreach (var t in tokens)
            {
                try
                {
                    await RevokeToken(t.Token);
                }
                catch (AccountNotFoundException)
                {
                    //ignore it, already revoked
                }
            }

            var deprecatedTokens = (from authToken in appDBContext_.AuthToken
                                    where authToken.AccountId == accountId
                                    select authToken).ToList();

            appDBContext_.RemoveRange(deprecatedTokens);
            await appDBContext_.SaveChangesAsync();
        }

        public Task RemovePersonallyIdentifiableInformationFromTokensAsync(int accountId)
        {
            // we ignore any row version, just update the revoke as it is a very strong request that shall not fail.
            return appDBContext_.Database.ExecuteSqlRawAsync(
                "update SessionTokens set UserAgent = '', Ip = '', Country = '' where AccountId = @accountId",
                new SqlParameter("accountId", accountId));
        }

        private string GenerateToken()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[32];
            provider.GetBytes(byteArray);
            return "2" + Convert.ToBase64String(byteArray);
        }
    }

    public static class SessionCheckExtension
    {
        public static bool CheckSiteInfo(this DBSessionToken session, SiteInfo siteInfo)
        {
            return session.UserAgent == siteInfo.UserAgent;
        }

        public static bool CheckExpirationDate(this DBSessionToken session)
        {
            var date = session.LastRefreshed ?? session.Issued;
            var validUntil = date + SessionTokenRepository.TOKEN_EXPIRATION;
            var now = DateTime.UtcNow;
            return validUntil >= now;
        }

        public static bool Check(this DBSessionToken session, SiteInfo siteInfo)
        {
            return true;
            // TODO: add checks when clients use the sign in and refresh flows defined by IAM and not the old auth APIs
            // return session.CheckSiteInfo(siteInfo)
            //     && session.CheckExpirationDate();
        }
    }
}
