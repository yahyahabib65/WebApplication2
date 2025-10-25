using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models;

public class RegisterVendorViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    // Add other vendor-specific properties here
    [Required]
    [Display(Name = "Store Name")]
    public string StoreName { get; set; }

    [Required]
    [Display(Name = "Business Address")]
    public string BusinessAddress { get; set; }

    [Required]
    [Display(Name = "Business Registration Number")]
    public string BusinessRegistrationNumber { get; set; }
}
