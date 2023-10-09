using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using NetCoreIdentityApp.Web.Models;
using NetCoreIdentityApp.Web.ViewModels;

namespace NetCoreIdentityApp.Web.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        // Dependency Injection ile SignInManager'ı alır
        public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Ana sayfa
        public async Task<IActionResult> Index()
        {
            // Kullanıcı bilgilerini görüntülemek için mevcut kullanıcıyı alır

            // User.Identity!.Name!, mevcut kimlik altında kullanıcı adını alır ve null olmadığını doğrular
            var currentUser = (await _userManager.FindByNameAsync(User.Identity!.Name!))!;

            // UserViewModel sınıfını kullanarak kullanıcı bilgilerini görüntülemek için bir görünüm modeli oluşturur

            var userViewModel = new UserViewModel
            {
                // Kullanıcının e-posta adresini alır ve UserViewModel'e ekler
                Email = currentUser.Email,

                // Kullanıcının kullanıcı adını alır ve UserViewModel'e ekler
                UserName = currentUser.UserName,

                // Kullanıcının telefon numarasını alır ve UserViewModel'e ekler
                PhoneNumber = currentUser.PhoneNumber,
            };

            // Oluşturulan UserViewModel'i kullanarak bir görünüm döndürür
            return View(userViewModel);
        }


        // Çıkış işlemi için kullanılan action
        public async Task Logout()
        {
            // Kullanıcıyı oturumdan çıkarır
            await _signInManager.SignOutAsync();
        }
    }
}

