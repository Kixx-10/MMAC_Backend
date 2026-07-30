using Microsoft.AspNetCore.Mvc;
using MMAC.DTOS;
using MMAC.Services.AuthService;
namespace MMAC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISignInService _signInService;
        public AuthController(ISignInService signInService)
        {
            _signInService = signInService;
        }

        [HttpPost("signIn")]
        public async Task<ActionResult> SignIn([FromBody] SignInRequestDTO requestDTO)
        {
            try
            {

                var result = await _signInService.SignInAsync(requestDTO);
                return Ok(new
                {
                    message = "Sign in successfully",
                    data = result
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during sign in.", error = ex.Message });
            }
        }

    }
}
