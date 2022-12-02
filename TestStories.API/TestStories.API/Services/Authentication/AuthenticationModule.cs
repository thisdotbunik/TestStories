using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TestStories.Common;
using TestStories.Common.Auth;
using static TestStories.API.Infrastructure.Filters.JWTAuthenticationFilter;

namespace TestStories.API.Common.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticationModule
    {
        private readonly TokenManagement _token = new TokenManagement();

        /// <summary>
        /// 
        /// </summary>
        public AuthenticationModule()
        {
            _token.Secret = EnvironmentVariables.JwtSecret;
            _token.Audience = EnvironmentVariables.JwtAudience;
            _token.Issuer = EnvironmentVariables.JwtIssuer;
        }

        /// Using the same key used for signing token, user payload is generated back
        public JwtSecurityToken GenerateUserClaimFromJwt(string authToken)
        {
            SecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_token.Secret));
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudiences = new[] { _token.Audience, },
                ValidIssuers = new[] { _token.Issuer, },
                IssuerSigningKey = signingKey
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken validatedToken;

            try
            {
                tokenHandler.ValidateToken(authToken, tokenValidationParameters, out validatedToken);
            }
            catch (Exception)
            {
                return null;
            }

            return validatedToken as JwtSecurityToken;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPayloadToken"></param>
        /// <returns></returns>
        public JWTAuthenticationIdentity PopulateUserIdentity(JwtSecurityToken userPayloadToken)
        {
            var name = ((userPayloadToken)).Claims.FirstOrDefault(m => m.Type == "Name")?.Value;
            var userId = ((userPayloadToken)).Claims.FirstOrDefault(m => m.Type == "UserId")?.Value;
            return new JWTAuthenticationIdentity(name) { UserId = userId, UserName = name };
        }
    }
}