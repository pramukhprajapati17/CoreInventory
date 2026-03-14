using System.ComponentModel.DataAnnotations;

namespace MVC.Models;

public sealed class ProfileUpdateModel
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Password { get; set; }
}
