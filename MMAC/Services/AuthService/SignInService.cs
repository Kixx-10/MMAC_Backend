using AutoMapper;
using MMAC.DTOS;
using MMAC.Repositories.AuthRepository;
using MMAC.Services.TokenService;

namespace MMAC.Services.AuthService
{
    public class SignInService : ISignInService
    {
        private readonly ITokenService _tokenService;
        private readonly ISignInRepository _signInRepository;
        private readonly IMapper _mapper;

        public SignInService(ITokenService tokenService, ISignInRepository signInRepository, IMapper mapper)
        {
            _tokenService = tokenService;
            _signInRepository = signInRepository;
            _mapper = mapper;
        }
        public async Task<SignInResponseDTO> SignInAsync(SignInRequestDTO requestDTO)
        {
            var dbUser = await _signInRepository.SingInByEmailAsync(requestDTO.Email);
            if (dbUser == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            bool isPasswordValid = dbUser.Password == requestDTO.Password;
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }
            var token = await _tokenService.CreateToken(dbUser.Id);

            var response = new SignInResponseDTO
            {
                Email = dbUser.Email,
                Role = dbUser.Role,
                Token = token
            };

            return response;
        }
    }
}