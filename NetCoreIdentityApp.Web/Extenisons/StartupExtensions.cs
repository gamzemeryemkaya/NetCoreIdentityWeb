using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NetCoreIdentityApp.Web.CustomValidations;
using NetCoreIdentityApp.Web.Localizations;
using NetCoreIdentityApp.Repository.Models;

namespace NetCoreIdentityApp.Web.Extenisons
{
    public static class StartupExtensions
    {
        public static void AddIdentityWithExt(this IServiceCollection services)
        {
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(2);
            });

            services.AddIdentity<AppUser, AppRole>(options =>
            {

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnoprstuvwxyz1234567890_";

                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;


                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
                options.Lockout.MaxFailedAccessAttempts = 3;




            }).AddPasswordValidator<PasswordValidator>()
                .AddUserValidator<UserValidator>()
                .AddErrorDescriber<LocalizationIdentityErrorDescriber>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<AppDbContext>();


        }
    }
}




//  public static class StartupExtensions
//{
//    // Identity servisini yapılandırmak için kullanılan bir extension metodu ekler.
//    public static void AddIdentityWithExt(this IServiceCollection services)
//    {
//        services.Configure<DataProtectionTokenProviderOptions>(opt =>
//        {
//            // Token ömrü 2 saat olarak ayarlanıyor-şifremi unuttum.
//            opt.TokenLifespan = TimeSpan.FromHours(2);
//        });
//        services.AddIdentity<AppUser, AppRole>(options =>


//            // Kullanıcı ayarlarını yapılandırma
//            options.User.RequireUniqueEmail = true; // E-posta adreslerinin benzersiz olması gerektiğini belirtir.
//        options.User.AllowedUserNameCharacters = "abcdefghijklmnoprstuvwxyz1234567890_"; // Kullanıcı adı için izin verilen karakterleri belirtir.

//        // Şifre ayarlarını yapılandırma
//        options.Password.RequiredLength = 6; // Şifrenin en az 6 karakter uzunluğunda olması gerektiğini belirtir.
//        options.Password.RequireNonAlphanumeric = false; // Şifre içermeyen özel karakterlerin gerekip gerekmediğini belirtir.
//        options.Password.RequireLowercase = true; // Küçük harf içermesinin gerekip gerekmediğini belirtir.
//        options.Password.RequireUppercase = false; // Büyük harf içermesinin gerekip gerekmediğini belirtir.
//        options.Password.RequireDigit = false; // Rakam içermesinin gerekip gerekmediğini belirtir.


//        // Oturum kilitlenmesi ayarlarını yapılandırır: Bir kullanıcı başarısız oturum açma girişimlerinden sonra ne kadar süre boyunca kilitlenmeli
//        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);

//        // Bir kullanıcının izin verilen maksimum başarısız oturum açma girişim sayısı
//        options.Lockout.MaxFailedAccessAttempts = 3;


//    }).AddPasswordValidator<PasswordValidator>()
//            .AddUserValidator<UserValidator>()
//             .AddDefaultTokenProviders()
//            .AddEntityFrameworkStores<AppDbContext>();


//            // Identity için kullanılacak veri bağlamını belirtir.
//        }
//    }
//}
