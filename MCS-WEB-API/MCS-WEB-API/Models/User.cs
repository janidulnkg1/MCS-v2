using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MCS_WEB_API.Models
{
    public class User
    {
        [Key]
        [Column("userID")]
        public int UserId { get; set; }

        [Column("username")]
        [Required]
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        [Required]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Required]
        [Column("passwordHash")]
        public byte[] PasswordHash { get; set; }

        [DataType(DataType.Password)]
        [Required]
        [Column("passwordSalt")]
        public byte[] PasswordSalt { get; set; }

        [Required]
        [Column("designation")]
        public string Designation { get; set; } = string.Empty;
    }
}
