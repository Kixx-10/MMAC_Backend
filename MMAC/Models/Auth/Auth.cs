using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MMAC.Models.Auth
{
    public class Auth
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Email { get; set; } = String.Empty;

        public string Password { get; set; } = String.Empty;

        public bool Role { get; set; }
    }
}
