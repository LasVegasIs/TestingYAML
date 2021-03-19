using Crey.Kernel.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Text;

namespace Core.Crypto
{
    public interface ICipherService
    {
        string Encrypt(string input);
        string Decrypt(string cipherText);
        string Base64Encode(string text);
        string Base64Decode(string text);
    }

    public class CipherServiceOptions
    {
        public string Key { get; set; }
    }

    public class CipherServiceOptionsBuilder
    {
        public Action<CipherServiceOptions> Build { get; set; }
    }


    public class CipherService : ICipherService
    {
        private readonly IDataProtectionProvider dataProtectionProvider_;
        private readonly CipherServiceOptions options_;

        public static byte[] EncryptionKey { get; private set; }

        public CipherService(IDataProtectionProvider dataProtectionProvider, CipherServiceOptionsBuilder builder)
        {
            dataProtectionProvider_ = dataProtectionProvider;
            options_ = new CipherServiceOptions();
            builder.Build(options_);
            Trace.Assert(!string.IsNullOrEmpty(options_.Key));
        }

        public string Encrypt(string input)
        {
            var protector = dataProtectionProvider_.CreateProtector(options_.Key);
            return protector.Protect(input);
        }

        public string Decrypt(string cipherText)
        {
            var protector = dataProtectionProvider_.CreateProtector(options_.Key);
            return protector.Unprotect(cipherText);
        }

        [Obsolete]
        public string Base64Encode(string text)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(plainTextBytes);
        }

        [Obsolete]
        public string Base64Decode(string text)
        {
            var base64EncodedBytes = Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public static class CipherServiceExtension
    {
        public static IServiceCollection AddCipherService(this IServiceCollection collectionBuilder, Action<CipherServiceOptions> builder)
        {
            Debug.Assert(collectionBuilder.HasIDInfoAccessor());

            collectionBuilder.AddDataProtection();
            collectionBuilder
                .AddSingleton(service => new CipherServiceOptionsBuilder { Build = builder })
                .AddSingleton<ICipherService, CipherService>();

            return collectionBuilder;
        }
    }
}