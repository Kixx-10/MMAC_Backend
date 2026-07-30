using Microsoft.IdentityModel.Tokens;
using MMAC.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MMAC.Services.TokenService
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public TokenService(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        public async Task<string> CreateToken(int id)
        {
            var user = await _context.Auths.FindAsync(id);
            if (user == null) throw new Exception("User not found");

            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);


            string roleName = user.Role ? "Admin" : "Officer";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
    }
}