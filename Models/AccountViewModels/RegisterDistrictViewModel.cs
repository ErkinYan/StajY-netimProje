namespace StajYonetim.Identity.Models.AccountViewModels;

using System.ComponentModel.DataAnnotations;

public class RegisterDistrictViewModel
{
    [Required]
    [Display(Name = "İlçe Adı")]
    public string IlceAdi { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Yetkili Ad")]
    public string YetkiliAd { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Yetkili Soyad")]
    public string YetkiliSoyad { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece 11 rakam içermelidir.")]
    [Display(Name = "TC Kimlik No")]
    public string TcKimlikNo { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Şifre en az 8 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "KVKK ve Kullanım koşulları")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "Kullanım koşullarını kabul etmelisiniz.")]
    public bool Kvkk { get; set; }
}
