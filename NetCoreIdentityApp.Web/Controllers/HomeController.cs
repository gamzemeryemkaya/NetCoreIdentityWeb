using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCoreIdentityApp.Web.Extenisons;
using NetCoreIdentityApp.Web.Models;
using NetCoreIdentityApp.Web.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace NetCoreIdentityApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _UserManager;
        private readonly SignInManager<AppUser> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _logger = logger;
            _UserManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var identityResult = await _UserManager.CreateAsync(new() { UserName = request.UserName, PhoneNumber = request.Phone, Email = request.Email }, request.PasswordConfirm);


            if (identityResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Üyelik kayıt işlemi başarıla gerçekleşmiştir.";
                return RedirectToAction(nameof(HomeController.SignUp));
            }

            ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
            //foreach (IdentityError item in identityResult.Errors)
            //{
            //    ModelState.AddModelError(string.Empty, item.Description);
            //}
            return View();
        }

        public IActionResult SignIn()

        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            // Eğer returnUrl belirtilmemişse, varsayılan olarak "Index" action'ına yönlendirme yapar
            returnUrl ??= Url.Action("Index", "Home");

            // Kullanıcının e-posta adresine göre veritabanında arama yapar
            var hasUser = await _UserManager.FindByEmailAsync(model.Email);

            // Eğer kullanıcı bulunamazsa, hata ekler ve giriş sayfasını tekrar gösterir
            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                return View();
            }

            // Kullanıcının şifresini ve "RememberMe" seçeneğini kullanarak giriş yapmayı dener
            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

            // Giriş işlemi başarılıysa, belirtilen returnUrl'e yönlendirme yapar
            if (signInResult.Succeeded)
            {
                return Redirect(returnUrl);
            }

            // Eğer kullanıcı hesabı kilitlenmişse, hata ekler ve giriş sayfasını tekrar gösterir
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelErrorList(new List<string>() { "3 dakika boyunca giriş yapamazsınız." });
                return View();
            }

            // Giriş işlemi başarısızsa ve hatalı giriş sayısı izin verilen sınırı aşmışsa, hata ekler ve giriş sayfasını tekrar gösterir
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelErrorList(new List<string>() { $"Email veya şifre yanlış", $"Başarısız giriş sayısı = {await _UserManager.GetAccessFailedCountAsync(hasUser)}" });
                return View();
            }

            // Giriş işlemi başarısızsa, hataları ModelState'a ekler ve giriş sayfasını tekrar gösterir
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış." });
            return View();
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}