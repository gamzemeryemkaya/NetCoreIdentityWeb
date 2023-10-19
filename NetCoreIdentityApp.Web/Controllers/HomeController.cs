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
        public IActionResult FacebookLogin(string ReturnUrl)
        {
            // Dönüş URL'sini oluşturuyoruz. Kullanıcı Facebook'ta oturum açtıktan sonra bu URL'ye yönlendirilecektir.
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            // Dış kaynak (Facebook) ile oturum açma işlemi için gereken özellikleri yapılandırıyoruz.
            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Facebook", RedirectUrl);

            // Kullanıcıyı Facebook giriş sayfasına yönlendiren ChallengeResult dönüşünü gerçekleştiriyoruz.
            return new ChallengeResult("Facebook", properties);
        }


        public IActionResult GoogleLogin(string ReturnUrl)

        {
            string RedirectUrl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", RedirectUrl);

            return new ChallengeResult("Google", properties);
        }



        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            // Dış kaynak (Facebook, Google vb.) ile giriş bilgilerini al
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

            // Eğer dış kaynak ile giriş bilgisi yoksa, giriş sayfasına yönlendir
            if (info == null)
            {
                return RedirectToAction("SignIn");
            }
            else
            {
                // Dış kaynak ile giriş denemesi yap
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);

                // Eğer başarılı ise, belirtilen URL'ye yönlendir
                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    // Yeni bir kullanıcı oluştur
                    AppUser user = new AppUser();

                    // Kullanıcı bilgilerini dış kaynaktan al
                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;

                    // Eğer dış kaynak kullanıcı adı (name) bilgisini içeriyorsa, kullanıcı adını ayarla
                    if (info.Principal.HasClaim(x => x.Type == ClaimTypes.Name))
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;

                        // Kullanıcı adında boşlukları kaldır, küçük harfe dönüştür ve benzersiz yap
                        userName = userName.Replace(' ', '-').ToLower() + ExternalUserId.Substring(0, 5).ToString();

                        user.UserName = userName;
                    }
                    else
                    {
                        // Eğer dış kaynak kullanıcı adı bilgisi yoksa, kullanıcı adını e-posta olarak ayarla
                        user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }

                    // Eğer bu e-postaya sahip kullanıcı henüz mevcut değilse, yeni bir kullanıcı oluştur
                    AppUser user2 = await _UserManager.FindByEmailAsync(user.Email);

                    if (user2 == null)
                    {
                        // Yeni kullanıcı oluştur
                        IdentityResult createResult = await _UserManager.CreateAsync(user);

                        // Kullanıcı oluşturma işlemi başarılı ise, kullanıcıyı dış kaynak ile ilişkilendir
                        if (createResult.Succeeded)
                        {
                            IdentityResult loginResult = await _UserManager.AddLoginAsync(user, info);

                            // Dış kaynak ile ilişkilendirme başarılı ise, kullanıcıyı oturum açtır ve belirtilen URL'ye yönlendir
                            if (loginResult.Succeeded)
                            {
                                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                // Dış kaynak ile ilişkilendirme başarısız ise, hata mesajı ekle
                                ModelState.AddModelError("",loginResult.Errors.ToString());
                            }
                        }
                        else
                        {
                            // Kullanıcı oluşturma işlemi başarısız ise, oluşan hataları ModelState'e ekle
                            ModelState.AddModelError("", createResult.Errors.ToString());
                        }
                    }
                    else
                    {
                        // Eğer aynı e-postaya sahip kullanıcı zaten mevcutsa, kullanıcıyı dış kaynak ile ilişkilendir
                        IdentityResult loginResult = await _UserManager.AddLoginAsync(user2, info);

                        // Dış kaynak ile ilişkilendirme işlemi başarılı ise, kullanıcıyı oturum açtır ve belirtilen URL'ye yönlendir
                        await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                        return Redirect(ReturnUrl);
                    }
                }
            }

            List<string> errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();

            return View("Error", errors);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Core.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}