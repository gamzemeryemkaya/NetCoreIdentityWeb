using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NetCoreIdentityApp.Web.Controllers
{
    // OrderController, "Permissions.Order.Read" politikasına sahip kullanıcıları kabul eden bir eylemi içerir.
    public class OrderController : Controller
    {
        [Authorize(Policy = "Permissions.Order.Read")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
