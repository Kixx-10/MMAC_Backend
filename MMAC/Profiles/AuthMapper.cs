using MMAC.DTOS;
using MMAC.Models.Auth;

namespace MMAC.Profiles
{
    public class AuthMapper : AutoMapper.Profile
    {
        public AuthMapper()
        {
            CreateMap<SignInRequestDTO, Auth>();
        }
    }
}
