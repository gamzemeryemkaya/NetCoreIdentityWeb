using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NetCoreIdentityApp.Web.Requirements
{
    // ExchangeExpireRequirement, AuthorizationHandler'a gereksinimler eklemek için kullanılan bir arayüzdür.
    public class ExchangeExpireRequirement : IAuthorizationRequirement
    {

    }

    // ExchangeExpireRequirementHandler, ExchangeExpireRequirement gereksinimini işlemek için kullanılan bir yetkilendirme işleyicisidir.
    public class ExchangeExpireRequirementHandler : AuthorizationHandler<ExchangeExpireRequirement>
    {
        // HandleRequirementAsync, gereksinimin işlenmesi için asenkron bir metottur.
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ExchangeExpireRequirement requirement)
        {
            // Kullanıcının "ExchangeExpireDate" adlı bir talebi olup olmadığını kontrol eder.
            if (!context.User.HasClaim(x => x.Type == "ExchangeExpireDate"))
            {
                // Eğer kullanıcının bu talebi yoksa, gereksinimi karşılanamaz olarak işaretle.
                context.Fail();
                return Task.CompletedTask;
            }

            // Kullanıcının "ExchangeExpireDate" adlı talebini alır.
            Claim exchangeExpireDate = context.User.FindFirst("ExchangeExpireDate")!;

            // Geçerli tarihi alır ve belirtilen süre ile karşılaştırır.
            if (DateTime.Now > Convert.ToDateTime(exchangeExpireDate.Value))
            {
                // Eğer belirtilen süre dolmuşsa, gereksinimi karşılanamaz olarak işaretle.
                context.Fail();
                return Task.CompletedTask;
            }

            // Tüm kontroller başarılıysa gereksinimi başarıyla karşılandı olarak işaretle.
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
