using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using NetCoreIdentityApp.Repository.Models;
using System.Security.Claims;

namespace NetCoreIdentityApp.Web.ClaimProvider
{
    public class UserClaimProvider : IClaimsTransformation
    {
        private readonly UserManager<AppUser> _userManager;

        public UserClaimProvider(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Öncelikle, gelen principal (kimlik) örneğini ClaimsIdentity tipine dönüştürüyoruz.
            var identityUser = principal.Identity as ClaimsIdentity;

            // Kimlikteki kullanıcı adını kullanarak veritabanından kullanıcıyı buluyoruz.
            var currentUser = await _userManager.FindByNameAsync(identityUser!.Name!);

            // Kullanıcının şehir bilgisi boşsa, herhangi bir işlem yapmadan aynı kimliği geri döndürüyoruz.
            if (String.IsNullOrEmpty(currentUser!.City))
            {
                return principal;
            }

            // Eğer kimlikte "city" adında bir claim (kimlik bilgisi) yoksa, şehir bilgisini ekliyoruz.
            if (!principal.HasClaim(x => x.Type == "city"))
            {
                Claim cityClaim = new Claim("city", currentUser.City);

                // ClaimsIdentity'ye şehir claim'ini ekliyoruz.
                identityUser.AddClaim(cityClaim);
            }

            // Değiştirilmiş kimliği (claim'leri) döndürüyoruz.
            return principal;
        }
    }
}

