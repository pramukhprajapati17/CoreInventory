using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public sealed class ChangePasswordModel
{
    [Required]
    [MaxLength(200)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
