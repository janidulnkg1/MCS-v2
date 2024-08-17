using System.ComponentModel.DataAnnotations;

namespace MCS_WEB_API.Models
{
    public class UserRegister
    {
        public int userId { get; set; }

        [Required]
        public string? Username { get; set; }

        [EmailAddress]
        [Required]

        public string? Email { get; set; }

        [DataType(DataType.Password)]
        [Required]

        public string? Password { get; set; }

        [Required]
        public string? Designation { get; set; }
    }
}
