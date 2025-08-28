using BELEPOS.DataModel;
using BELEPOS.Entity;
using BELEPOS.Entity.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BELEPOS.Service
{
    public class JwtService
    {
        private readonly JwtSettingsDto _settings;

        private readonly BeleposContext _context;

        public JwtService(IOptions<JwtSettingsDto> settings, BeleposContext context)
        {
            _settings = settings.Value;
            _context = context;
        }

        public string GenerateTokenWithPermissions(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Userid.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                //new Claim("role", user.Role.RoleName), // 🔥 single role claim
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var permissions = _context.RolePermissions
                             .Where(rp => rp.Roleid == user.Roleid)
                             .Select(rp => rp.Permission.Permissionname)
                             .ToList();

            // 🔥 Add permission claims
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
