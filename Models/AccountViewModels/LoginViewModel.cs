namespace StajYonetim.Identity.Models.AccountViewModels;

using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [Display(Name = "TC Kimlik No veya E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;    

    [Display(Name = "Beni hatırla")]
    public bool RememberMe { get; set; }    
}
