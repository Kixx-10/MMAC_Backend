namespace MMAC.DTOS
{
    public class SignInResponseDTO
    {
        public string Email { get; set; } = string.Empty;
        public bool Role { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}