using MMAC.Models.Auth;

namespace MMAC.Repositories.AuthRepository
{
    public interface ISignInRepository
    {
        Task<Auth?> SingInByEmailAsync(string email);
    }
}
