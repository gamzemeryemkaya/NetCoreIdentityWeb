using Microsoft.AspNetCore.Identity;
using NetCoreIdentityApp.Web.CustomValidations;
using NetCoreIdentityApp.Web.Models;

namespace NetCoreIdentityApp.Web.Extenisons
{
    // Bu sınıf, Startup sınıfına ek işlevler eklemek için kullanılır.
    public static class StartupExtensions
    {
        // Identity servisini yapılandırmak için kullanılan bir extension metodu ekler.
        public static void AddIdentityWithExt(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, AppRole>(options =>
            {
                // Kullanıcı ayarlarını yapılandırma
                options.User.RequireUniqueEmail = true; // E-posta adreslerinin benzersiz olması gerektiğini belirtir.
                options.User.AllowedUserNameCharacters = "abcdefghijklmnoprstuvwxyz1234567890_"; // Kullanıcı adı için izin verilen karakterleri belirtir.

                // Şifre ayarlarını yapılandırma
                options.Password.RequiredLength = 6; // Şifrenin en az 6 karakter uzunluğunda olması gerektiğini belirtir.
                options.Password.RequireNonAlphanumeric = false; // Şifre içermeyen özel karakterlerin gerekip gerekmediğini belirtir.
                options.Password.RequireLowercase = true; // Küçük harf içermesinin gerekip gerekmediğini belirtir.
                options.Password.RequireUppercase = false; // Büyük harf içermesinin gerekip gerekmediğini belirtir.
                options.Password.RequireDigit = false; // Rakam içermesinin gerekip gerekmediğini belirtir.

            }).AddPasswordValidator<PasswordValidator>()
            .AddUserValidator<UserValidator>()
            .AddEntityFrameworkStores<AppDbContext>(); // Identity için kullanılacak veri bağlamını belirtir.
        }
    }
}
