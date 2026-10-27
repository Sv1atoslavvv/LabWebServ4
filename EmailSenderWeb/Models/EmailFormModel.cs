using System.ComponentModel.DataAnnotations;

namespace EmailSenderWeb.Models;

public sealed class EmailFormModel
{
    [Required(ErrorMessage = "Вкажіть email одержувача.")]
    [EmailAddress(ErrorMessage = "Введіть коректну email-адресу.")]
    [Display(Name = "Email одержувача")]
    public string Recipient { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть текст повідомлення.")]
    [StringLength(5000, MinimumLength = 2, ErrorMessage = "Повідомлення має містити від 2 до 5000 символів.")]
    [Display(Name = "Повідомлення")]
    public string Message { get; set; } = string.Empty;
}
