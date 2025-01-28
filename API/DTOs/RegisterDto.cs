using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [StringLength(8,MinimumLength = 5)]
    public string Password { get; set; } = string.Empty;
}
