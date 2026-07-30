namespace MMAC.Services.TokenService
{
    public interface ITokenService
    {
        Task<String> CreateToken(int id);
    }
}
