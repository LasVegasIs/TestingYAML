using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Crey.Misc
{
    public static class SignatureHelper
    {
        private static JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
        public static string Sign(int issuer, IEnumerable<Claim> claims, string secretKey) =>
            CreateToken(issuer, claims, secretKey).To(jwt.WriteToken);

        private static JwtSecurityToken CreateToken(int issuer, IEnumerable<Claim> data, string secretKey)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var crypto = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            return new JwtSecurityToken(
                claims: data,
                issuer: issuer.ToString(),
                signingCredentials: crypto
                );
        }

        public static (int issuer, IEnumerable<Claim> data) Data(string token, string secretKey)
        {
            var data = jwt.ReadJwtToken(token);
            var validations = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false, // infinite
                ValidateAudience = false,
                ValidateIssuer = false
            };
            jwt.ValidateToken(token, validations, out var valid);
            valid.NotNull();
            return (Int32.Parse(data.Issuer), data.Claims);
        }

    }
}
