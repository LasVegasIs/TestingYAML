using System;

namespace Crey.Utils
{
#nullable enable
    public class SignedCDNUrlHelper
    {
        private readonly string _urlPattern;
        private readonly string _cdnKey1;
        private readonly string _cdnKey2;

        /// urlPattern is a pattern where {imageId} and {version} and {signature} are replaced automatically
        public SignedCDNUrlHelper(string urlPattern, string cdnKey1, string cdnKey2)
        {
            _urlPattern = urlPattern;
            _cdnKey1 = cdnKey1;
            _cdnKey2 = cdnKey2;
        }

        public string GetCDNUrl(string imageId, int version)
        {
            var md5Key = CalculateSignature(imageId, version);
            var uri = _urlPattern
                .Replace("{imageId}", imageId)
                .Replace("{version}", $"{version}")
                .Replace("{signature}", md5Key);
            return new Uri(uri).AbsoluteUri;
        }

        public string CalculateSignature(string imageId, int versionHint)
        {
            return CryptoHelper.CalculateMd5Key(
                $"{imageId},{versionHint}",
                _cdnKey1,
                _cdnKey2);
        }

        public bool ValidateSignature(string imageId, string? signature, int? versionHint)
        {
            if (string.IsNullOrEmpty(signature) && !versionHint.HasValue)
                return true;    // no version info, nothing to validate
            if (string.IsNullOrEmpty(signature) || !versionHint.HasValue)
                return false;   // both version and signature have to be provided
            return signature == CalculateSignature(imageId, versionHint.Value);
        }
    }
}
