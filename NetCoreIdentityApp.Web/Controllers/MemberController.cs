using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using NetCoreIdentityApp.Web.Models;

namespace NetCoreIdentityApp.Web.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;

        // Dependency Injection ile SignInManager'ı alır
        public MemberController(SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // Ana sayfa
        public IActionResult Index()
        {
            return View();
        }

        // Çıkış işlemi için kullanılan action
        public async Task Logout()
        {
            // Kullanıcıyı oturumdan çıkarır
            await _signInManager.SignOutAsync();
        }
    }
}

