using MMAC.DTOS;

namespace MMAC.Services.AuthService
{
    public interface ISignInService
    {
        Task<SignInResponseDTO> SignInAsync(SignInRequestDTO requestDTO);
    }
}