using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace CoreAngJwt
{
    public class JwtTokenOptions
    {
        public string Issuer { get; set; } // Emissor do token
        public string Subject { get; set; }  // Assunto
        public string Audience { get; set; }  // Para qual site este token é valido, https://site.domain.com por exemplo
        public DateTime NotBefore { get; set; } = DateTime.UtcNow; // Não utilizável antes desta data
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow; // Quando ele foi emitido
        public TimeSpan ValidFor { get; set; } = TimeSpan.FromHours(5); // Por quando tempo ele é válido. No caso 5 horas de validade para o Token
        public DateTime Expiration => IssuedAt.Add(ValidFor); // Expiração dele que é de quando ele foi emitido até o tempo de duração do token.
        public Func<Task<string>> JtiGenerator => () => Task.FromResult(Guid.NewGuid().ToString()); // Task Generator que gera a string de um novo Guid
        public SigningCredentials signingCredentials { get; set; }
    }
}