using Crey.Contracts;
using Crey.Exceptions;
using Crey.Kernel;
using Crey.Kernel.ServiceDiscovery;
using Crey.Misc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IAM.Areas.Authentication 
{
    public interface IHoldJson
    {
        JObject payload { get; }
    }

    public interface ICallbackData : IHoldJson
    {
        Uri callback { get; }
    }

    public class Callback : ICallbackData
    {
        /// <summary>
        /// After unsigned, payload is send to this url. Must be POST for now.
        /// </summary>
        /// <remarks>
        ///  Must be safe to call any number of times by malicious user.
        /// </remarks>
        [Required]
        public Uri callback { get; set; }

        /// <summary>
        /// Can add here key/values if will need any other data
        /// </summary>
        public JObject payload { get; set; }
    }

    public class SafeUriConverter : JsonConverter<Uri>
    {
        public override void WriteJson(JsonWriter writer, Uri value, JsonSerializer serializer)
        {
            writer.WriteValue(WebUtility.UrlEncode(value.ToString()));
        }

        public override Uri ReadJson(JsonReader reader, Type objectType, Uri existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new Uri(WebUtility.UrlDecode((string)reader.Value));
        }
    }

    /// <summary>
    /// Invited (referral) registration flow.
    /// </summary>
    public class SignedParams
    {
        [Required]
        public string creyticket { get; set; }
    }

    public class SignInfo
    {
        /// <summary>
        /// AccountId of user who created signed callback.
        /// </summary>
        public int issuer { get; set; }

        public DateTimeOffset timestamp { get; set; }

        /// <summary>
        /// In case we will have to update key.
        /// </summary>
        public byte version { get; set; }

    }

    public class SignedPayload : SignInfo
    {
        public string creyticket { get; set; }
    }

    public class UnsignedPayload : SignInfo, ICallbackData
    {
        /// <summary>
        /// After unsigned, <see cref="payload"/> is send to this url. Must be POST for now.
        /// </summary>
        /// <remarks>
        ///  Must be safe to call any number of times by malicious user.
        /// </remarks>
        [Required]
        public Uri callback { get; set; }

        public JObject payload { get; set; }
    }

    public class PostData : SignInfo, IHoldJson
    {
        public JObject payload { get; set; }
    }

    public class SignatureFlowService
    {
        private string secret_;
        private ICreyService<AccountRepository> users_;
        private CreyRestClient http_;

        public SignatureFlowService(IConfiguration configuration, ICreyService<AccountRepository> users, CreyRestClient http)
        {
            secret_ = configuration.GetValue<string>("COOKIE-SESSIONINFO-SECRET");
            users_ = users;
            http_ = http;
        }

        public SignedPayload Sign(Callback data, int issuerAccountId, DateTimeOffset timestamp)
        {
            if (issuerAccountId <= 0)
                throw new InvalidOperationException("Must be registered user.");
            byte version = 0;
            var forSign =
                new List<Claim>
                    {
                        new Claim(nameof(SignedPayload.timestamp), timestamp.ToString()),
                        new Claim(nameof(UnsignedPayload.callback), data.callback.ToString()),
                        new Claim(nameof(SignedPayload.version), version.ToString()),
                    };

            if (data.payload != null)
            {
                forSign.Add(new Claim(nameof(UnsignedPayload.payload), data.payload.ToString()));
            }

            var creyticket = SignatureHelper.Sign(issuerAccountId,
                    forSign,
                    secret_
                );
            return new SignedPayload { creyticket = creyticket, version = version, issuer = issuerAccountId, timestamp = timestamp };
        }

        public async Task<UnsignedPayload> VerifyUnsign(string creyticket)
        {
            var (issuer, data) = SignatureHelper.Data(creyticket.Trim(), secret_);
            var values = data.ToDictionary(x => x.Type, x => x.Value);
            var result = new UnsignedPayload
            {
                version = byte.Parse(values[nameof(SignedPayload.version)]),
                issuer = issuer,
                timestamp = DateTimeOffset.Parse(values[nameof(SignedPayload.timestamp)]),
                callback = new Uri(values[nameof(UnsignedPayload.callback)]),

            };
            // TODO: validate exception are mapped to proper HTTP codes
            if (result.timestamp >= DateTimeOffset.UtcNow.AddSeconds(30) || result.timestamp < new DateTimeOffset(new DateTime(2020, 01, 01)))
                throw new ValidationException($"Invalid {nameof(result.timestamp)}");

            var user = await users_.Value.FindUserByAccountIdAsync(result.issuer) ?? throw new AccountNotFoundException(result.issuer.ToString());
            return result;
        }

        public Task<HttpStatusCode> ExecuteCallback(Uri callback, PostData data)
        {
            var content = new StringContent(JsonConvert.SerializeObject(data));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return http_.PostNoDataForRequestUriAsync(callback.ToString(), null, content);
        }
    }
}
