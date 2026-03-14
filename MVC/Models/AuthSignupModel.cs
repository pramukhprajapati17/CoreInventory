using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public sealed class AuthSignupModel
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MaxLength(200)]
    public string Password { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [MaxLength(200)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; }
}
