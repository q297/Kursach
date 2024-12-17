using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend;
using Microsoft.IdentityModel.Tokens;

namespace Test
{
    public static class JwtHelper
    {
        public static string CreateJwt1(string value)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, value) };
            var jwt = new JwtSecurityToken(
                AuthOptions.Issuer,
                AuthOptions.Audience,
                claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
    }
}