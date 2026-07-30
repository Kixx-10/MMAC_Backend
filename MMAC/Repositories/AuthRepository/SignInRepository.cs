using Microsoft.EntityFrameworkCore;
using MMAC.Data;
using MMAC.Models.Auth;

namespace MMAC.Repositories.AuthRepository
{
    public class SignInRepository : ISignInRepository
    {
        private readonly AppDbContext _context;

        public SignInRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Auth?> SingInByEmailAsync(string email)
        {
            return await _context.Auths
                .SingleOrDefaultAsync(x => x.Email == email);
        }
    }
}