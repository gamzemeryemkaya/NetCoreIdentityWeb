using Microsoft.AspNetCore.Identity;

namespace NetCoreIdentityApp.Web.Localizations
{
    // Bu sınıf, IdentityErrorDescriber sınıfını genişleterek özel hata mesajları tanımlar.Türkçeleştirir.
    public class LocalizationIdentityErrorDescriber : IdentityErrorDescriber
    {
        // Kullanıcı adının benzersiz olmaması durumunda döndürülecek hata mesajını oluşturan metot
        public override IdentityError DuplicateUserName(string userName)
        {
            return new() { Code = "DuplicateUserName", Description = $"{userName} daha önce başka bir kullanıcı tarafından alınmıştır" };
        }

        // E-posta adresinin benzersiz olmaması durumunda döndürülecek hata mesajını oluşturan metot
        public override IdentityError DuplicateEmail(string email)
        {
            return new() { Code = "DuplicateEmail", Description = $"{email} daha önce başka bir kullanıcı tarafından alınmıştır" };
        }

        // Şifrenin yetersiz uzunlukta olması durumunda döndürülecek hata mesajını oluşturan metot
        public override IdentityError PasswordTooShort(int length)
        {
            return new() { Code = "PasswordTooShort", Description = $"Şifre en az 6 karakterli olmalıdır" };
        }
    }
}
