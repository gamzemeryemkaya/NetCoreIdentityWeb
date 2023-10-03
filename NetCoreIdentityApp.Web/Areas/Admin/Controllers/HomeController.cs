using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCoreIdentityApp.Web.Areas.Admin.Models;
using NetCoreIdentityApp.Web.Models;

namespace NetCoreIdentityApp.Web.Areas.Admin.Controllers
{
    // "Admin" alanında çalışan HomeController sınıfı
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        // HomeController sınıfının yapıcı metodu
        // UserManager<AppUser> türünde bir bağımlılığı alır
        public HomeController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Kullanıcı listesini döndüren asenkron eylem (action)
        public async Task<IActionResult> UserList()
        {
            // Kullanıcı listesini asenkron olarak alır
            var userList = await _userManager.Users.ToListAsync();

            // Kullanıcıları kullanıcı görünüm modellerine dönüştürür
            var userViewModelList = userList.Select(x => new UserViewModel()
            {
                Id = x.Id,
                Email = x.Email,
                Name = x.UserName
            }).ToList();

            // Kullanıcı listesi görünümünü döndürür
            return View(userViewModelList);
        }
    }
}