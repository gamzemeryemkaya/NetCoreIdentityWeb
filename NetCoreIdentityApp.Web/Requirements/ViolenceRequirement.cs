using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AspNetCoreIdentityApp.Web.Requirements
{
    // Yetkilendirme gereksinimini tanımlayan sınıf
    public class ViolenceRequirement : IAuthorizationRequirement
    {
        // Şiddet eşiği için yaş sınırlaması
        public int ThresholdAge { get; set; }
    }

    // Yetkilendirme gereksinimini işleyen sınıf
    public class ViolenceRequirementHandler : AuthorizationHandler<ViolenceRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ViolenceRequirement requirement)
        {
            // Kullanıcıya ait 'birthdate' tipinde bir claim var mı kontrol edilir
            if (!context.User.HasClaim(x => x.Type == "birthdate"))
            {
                // Eğer yoksa, yetkilendirme başarısız olur
                context.Fail();
                return Task.CompletedTask;
            }

            // Kullanıcının doğum tarihi claim'i alınır
            Claim birthDateClaim = context.User.FindFirst("birthdate")!;

            // Güncel tarih ve kullanıcının doğum tarihi kullanılarak yaş hesaplanır
            var today = DateTime.Now;
            var birthDate = Convert.ToDateTime(birthDateClaim.Value);
            var age = today.Year - birthDate.Year;

            
            if (birthDate > today.AddYears(-age)) age--;

            // Kullanıcının yaşını, gereksinimde belirtilen yaş sınırlaması ile karşılaştırır
            if (requirement.ThresholdAge > age)
            {
                // Eğer yaş sınırlamanın altındaysa, yetkilendirme başarısız olur
                context.Fail();
                return Task.CompletedTask;
            }

            // Eğer yaş sınırlamanın üstündeyse, yetkilendirme başarılıdır
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}

