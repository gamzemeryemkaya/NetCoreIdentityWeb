using Microsoft.AspNetCore.Identity;
using NetCoreIdentityApp.Web.Models;

namespace NetCoreIdentityApp.Web.CustomValidations
{
    // Bu sınıf, kullanıcı adı doğrulamasını uygulayan bir IUserValidator'dır.
    public class UserValidator : IUserValidator<AppUser>
    {
        // Kullanıcı adı doğrulamasını gerçekleştiren metot
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user)
        {
            var errors = new List<IdentityError>();

            // Kullanıcı adının ilk karakterinin bir rakam (sayı) olup olmadığını kontrol eder
            var isDigit = int.TryParse(user.UserName![0].ToString(), out _);

            if (isDigit)
            {
                errors.Add(new() { Code = "UserNameContainFirstLetterDigit", Description = "Kullanıcı adının ilk karakteri sayısal bir karakter içeremez" });
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






