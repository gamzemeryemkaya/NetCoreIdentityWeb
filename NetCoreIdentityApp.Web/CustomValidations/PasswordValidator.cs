using Microsoft.AspNetCore.Identity;
using NetCoreIdentityApp.Web.Models;

namespace NetCoreIdentityApp.Web.CustomValidations
{
    // Bu sınıf, özel şifre doğrulama kurallarını uygulayan bir IPasswordValidator'dır.
    public class PasswordValidator : IPasswordValidator<AppUser>
    {
        // Şifre doğrulamasını gerçekleştiren metot
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
        {
            var errors = new List<IdentityError>();

            // Şifrenin kullanıcı adını içerip içermediğini kontrol eder
            if (password!.ToLower().Contains(user.UserName!.ToLower()))
            {
                errors.Add(new() { Code = "PasswordContainUserName", Description = "Şifre alanı kullanıcı adı içeremez" });
            }

            // Şifrenin "1234" ile başlayıp başlamadığını kontrol eder
            if (password!.ToLower().StartsWith("1234"))
            {
                errors.Add(new() { Code = "PasswordContain1234", Description = "Şifre alanı ardışık sayı içeremez" });
            }

            // Eğer hatalar varsa, IdentityResult ile hata sonuçlarını döndürür
            if (errors.Any())
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }

            // Hata yoksa başarılı sonuç döndürür
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
