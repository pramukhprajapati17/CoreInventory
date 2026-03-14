using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public sealed class AuthLoginModel
{
    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MaxLength(200)]
    public string? Password { get; set; }
}
