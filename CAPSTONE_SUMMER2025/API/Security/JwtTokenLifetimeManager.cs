using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Security
{
    public class JwtTokenLifetimeManager
    {
        private static readonly ConcurrentDictionary<string, DateTime> DisavowedSignatures = new();

        public static bool ValidateTokenLifetime(DateTime? notBefore,
                                           DateTime? expires,
                                           SecurityToken securityToken,
                                           TokenValidationParameters validationParameters) =>
           securityToken is JwtSecurityToken token &&
           token.ValidFrom <= DateTime.UtcNow &&
           token.ValidTo >= DateTime.UtcNow &&
           DisavowedSignatures.ContainsKey(token.RawSignature) is false;

        public static void SignOut(SecurityToken securityToken)
        {
            if (securityToken is JwtSecurityToken token)
                DisavowedSignatures.TryAdd(token.RawSignature, token.ValidTo);

            foreach ((string? key, DateTime _) in DisavowedSignatures.Where(x => x.Value < DateTime.UtcNow))
                DisavowedSignatures.TryRemove(key, out DateTime _);
        }
    }
}
