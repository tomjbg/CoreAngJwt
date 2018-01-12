using Microsoft.IdentityModel.Tokens;

namespace CoreAngJwt
{
    public class JwtTokenOptions
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }    
        public SigningCredentials signingCredentials { get; set; }
    }
}