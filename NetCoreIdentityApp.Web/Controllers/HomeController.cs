using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCoreIdentityApp.Web.Extenisons;
using NetCoreIdentityApp.Repository.Models;
using NetCoreIdentityApp.Core.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Security.Policy;
using NetCoreIdentityApp.Service.Services;

namespace NetCoreIdentityApp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> _UserManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;

        public HomeController(ILogger<HomeController> logger, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService)
        {
            _logger = logger;
            _UserManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
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


            if (!identityResult.Succeeded)
            {
                ModelState.AddModelErrorList(identityResult.Errors.Select(x => x.Description).ToList());
                return View();
            }

            var exchangeExpireClaim = new Claim("ExchangeExpireDate", DateTime.Now.AddDays(10).ToString());

            var user = await _UserManager.FindByNameAsync(request.UserName);

            var claimResult = await _UserManager.AddClaimAsync(user!, exchangeExpireClaim);


            if (!claimResult.Succeeded)
            {
                ModelState.AddModelErrorList(claimResult.Errors.Select(x => x.Description).ToList());
                return View();
            }


            TempData["SuccessMessage"] = "Üyelik kayıt işlemi başarıla gerçekleşmiştir.";

            return RedirectToAction(nameof(HomeController.SignUp));

        }

        public IActionResult SignIn()

        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model, string? returnUrl = null)
        {
            // Eğer model geçerli değilse, kullanıcıya aynı sayfayı tekrar göster
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Eğer 'returnUrl' boşsa, varsayılan olarak ana sayfaya yönlendir
            returnUrl ??= Url.Action("Index", "Home");

            // Kullanıcının e-posta adresi ile kaydını kontrol et
            var hasUser = await _UserManager.FindByEmailAsync(model.Email);

            // Eğer kullanıcı yoksa, hata ekleyip aynı sayfayı tekrar göster
            if (hasUser == null)
            {
                ModelState.AddModelError(string.Empty, "Email veya şifre yanlış");
                return View();
            }

            // Kullanıcının şifresini ve diğer giriş bilgilerini kontrol et
            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, model.Password, model.RememberMe, true);

            // Eğer kullanıcı hesabı kilitlenmişse, hata ekleyip aynı sayfayı tekrar göster
            if (signInResult.IsLockedOut)
            {
                ModelState.AddModelErrorList(new List<string>() { "3 dakika boyunca giriş yapamazsınız." });
                return View();
            }

            // Eğer giriş başarısız olduysa, hataları ekleyip aynı sayfayı tekrar göster
            if (!signInResult.Succeeded)
            {
                var accessFailedCount = await _UserManager.GetAccessFailedCountAsync(hasUser);
                ModelState.AddModelErrorList(new List<string>() { $"Email veya şifre yanlış", $"Başarısız giriş sayısı = {accessFailedCount}" });
                return View();
            }

            // Eğer kullanıcının doğum tarihi varsa, doğum günü claim'ini ekler
            if (hasUser.BirthDate.HasValue)
            {
                await _signInManager.SignInWithClaimsAsync(hasUser, model.RememberMe, new[] { new Claim("birthdate", hasUser.BirthDate.Value.ToString()) });
            }

            // Başarılı giriş durumunda belirtilen 'returnUrl' sayfasına yönlendir
            return Redirect(returnUrl!);
        }




        // Şifreyi unutan kullanıcılar için sayfayı görüntülemek için kullanılan kontrolör eylemi
        public IActionResult ForgetPassword()
        {
            return View();
        }

        // Şifreyi unutan kullanıcıların şifre sıfırlama isteği gönderdiği HTTP POST isteğine yanıt olarak çalışan kontrolör eylemi
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            // Kullanıcı e-posta adresi ile aranır
            var hasUser = await _UserManager.FindByEmailAsync(request.Email);

            // Kullanıcı bulunamazsa hata eklenir ve aynı sayfaya geri dönülür
            if (hasUser == null)
            {
                ModelState.AddModelError(String.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
                return View();
            }

            // Şifre sıfırlama tokeni oluşturulur
            string passwordResetToken = await _UserManager.GeneratePasswordResetTokenAsync(hasUser);

            // Şifre sıfırlama bağlantısı oluşturulur
            var passwordResetLink = Url.Action("ResetPassword", "Home", new { userId = hasUser.Id, Token = passwordResetToken }, HttpContext.Request.Scheme);

            await _emailService.SendResetPasswordEmail(passwordResetLink!, hasUser.Email!);

            // Başarı mesajı geçici veriye eklenir
            TempData["SuccessMessage"] = "Şifre yenileme linki, eposta adresinize gönderilmiştir";

            // Şifreyi unutan sayfasına yeniden yönlendirilir
            return RedirectToAction(nameof(ForgetPassword));
        }



        public IActionResult ResetPassword(string userId, string token)
        {
            // Kullanıcıya ait userId ve token bilgilerini TempData üzerinde sakla.
            TempData["userId"] = userId;
            TempData["token"] = token;

            // Şifre sıfırlama sayfasını görüntüle.
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            // TempData üzerinden userId ve token bilgilerini al.
            var userId = TempData["userId"];
            var token = TempData["token"];

            if (userId == null || token == null)
            {
                throw new Exception("Bir hata meydana geldi");
            }

            // Kullanıcıyı userId ile bul.
            var hasUser = await _UserManager.FindByIdAsync(userId.ToString()!);

            if (hasUser == null)
            {
                ModelState.AddModelError(String.Empty, "Kullanıcı bulunamamıştır.");
                return View();
            }

            // Yeni şifreyi kullanıcı için ayarla.
            IdentityResult result = await _UserManager.ResetPasswordAsync(hasUser, token.ToString()!, request.Password);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir";
            }
            else
            {
                ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
            }

            // Şifre sıfırlama sonucunu gösteren sayfayı görüntüle.
            return View();
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Core.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}