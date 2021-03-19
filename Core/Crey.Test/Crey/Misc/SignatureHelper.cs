using Crey.Misc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Crey.Misc
{
    public class SignatureHelperTests
    {

        [Fact]
        public void SignUnsignVerify()
        {
            var secret = "15766391286959756134157663912869597561341576639128695975613415766391286959756134";
            var input = new[] { new Claim("schema", "https://playcrey.com/coupon"), new Claim("payload", "{\"a\" :[]}") };
            var token = SignatureHelper.Sign(42, input, secret);
            var (issuer, data) = SignatureHelper.Data(token, secret);
            Assert.Equal(42, issuer);
            Assert.Equal(data.First().Value, input[0].Value);
            Assert.Equal(data.Skip(1).First().Value, input[1].Value);
            Assert.Throws<SecurityTokenInvalidSignatureException>(() => SignatureHelper.Data(token, secret + "1"));
        }
    }
}
