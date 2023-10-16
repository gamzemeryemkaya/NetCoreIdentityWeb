using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using NetCoreIdentityApp.Web.Extenisons;
using NetCoreIdentityApp.Repository.Models;
using NetCoreIdentityApp.Core.ViewModels;
using System.Security.Claims;
using NetCoreIdentityApp.Core.Models;
using NetCoreIdentityApp.Service.Services;

namespace NetCoreIdentityApp.Web.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileProvider _fileProvider;
        private readonly IMemberService _memberService;
        private string userName => User.Identity!.Name!;

        // Dependency Injection ile SignInManager'ı alır
        public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IFileProvider fileProvider, IMemberService memberService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _fileProvider = fileProvider;
            _memberService = memberService;
        }

        // Ana sayfa-kodlar katmanlı mimariye uyarlandı 
        public async Task<IActionResult> Index()
        {
            // Oluşturulan UserViewModel'i kullanarak bir görünüm döndürür
            return View(await _memberService.GetUserViewModelByUserNameAsync(userName));
        }


        // Çıkış işlemi için kullanılan action
        public async Task Logout()
        {
            // Kullanıcıyı oturumdan çıkarır
            await _memberService.LogoutAsync();
        }



        public IActionResult PasswordChange()
        {
            // Şifre değiştirme sayfasını görüntüler
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel request)
        {
            if (!ModelState.IsValid)
            {
                // Eğer model geçerli değilse (örneğin, şifre değiştirme isteği geçerli değilse), sayfayı tekrar gösterir
                return View();
            }

            // Kullanıcının eski şifresini kontrol etmek için IMemberService üzerinden servisi kullanır
            if (!await _memberService.CheckPasswordAsync(userName, request.PasswordOld))
            {
                // Eski şifre yanlışsa, hata ekler ve sayfayı tekrar gösterir
                ModelState.AddModelError(string.Empty, "Eski şifreniz yanlış");
                return View();
            }

            // Yeni şifre ile eski şifreyi değiştirme işlemini gerçekleştirir
            var (isSuccess, errors) = await _memberService.ChangePasswordAsync(userName, request.PasswordOld, request.PasswordNew);

            if (!isSuccess)
            {
                // Eğer şifre değiştirme işlemi başarısızsa, hata ekler ve sayfayı tekrar gösterir
                ModelState.AddModelErrorList(errors!);
                return View();
            }

            // Şifre değiştirme işlemi başarılıysa, başarı mesajını geçici verilere ekler
            TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir";

            return View();
        }

        public async Task<IActionResult> UserEdit()
        {
            // Cinsiyet seçeneklerini ViewBag üzerinden görünüme gönderir
            ViewBag.genderList = _memberService.GetGenderSelectList();
            return View(await _memberService.GetUserEditViewModelAsync(userName));
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserEditViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Kullanıcının bilgilerini güncellemek için servisi kullanır
            var (isSuccess, errors) = await _memberService.EditUserAsync(request, userName);

            if (!isSuccess)
            {
                ModelState.AddModelErrorList(errors!);
                return View();
            }

            TempData["SuccessMessage"] = "Üye bilgileri başarıyla değiştirilmiştir";

            return View(await _memberService.GetUserEditViewModelAsync(userName));
        }



        public IActionResult AccessDenied(string ReturnUrl)
        {
            string message = string.Empty;

            // Erişim reddedildiğinde kullanıcıya görüntülenen ileti (hata mesajı)
            message = "Bu sayfayı görmeye yetkiniz yoktur. Yetki almak için yöneticiniz ile görüşebilirsiniz.";

            // ViewBag, görünüm dosyasına veri iletmek için kullanılan bir mekanizmadır.
            // Burada "message" adında bir değişken, görünüm dosyasında kullanılmak üzere ViewBag'e eklenir.
            ViewBag.message = message;

            // "AccessDenied" görünümünü döndürerek erişim reddedildiğini bildiren bir sayfa görüntüler.
            return View();
        }

        [HttpGet]
        public IActionResult Claims()
        {

            return View(_memberService.GetClaims(User));

        }

        // Bu aksiyon sadece "AntalyaPolicy" adlı yetkilendirme politikasına sahip kullanıcılar için erişilebilirdir.
        [Authorize(Policy = "AntalyaPolicy")]
        [HttpGet]
        public IActionResult AntalyaPage()
        {
            return View();
        }
        //10 gün ücretsiz deneme sayfası -policy

        [Authorize(Policy = "ExchangePolicy")]
        [HttpGet]
        public IActionResult ExchangePage()
        {
            return View();

        }
        //18 yaşından küçüklere erişim sınırlama sayfa-policy
        [Authorize(Policy = "ViolencePolicy")]
        [HttpGet]
        public IActionResult ViolencePage()
        {
            return View();
        }


    }
}



//// katmanlı miamri için  // Kullanıcının eski şifresini kontrol etmek için servis kullanılır ve // Şifre değiştirme işlemi servis aracılığıyla gerçekleştirilir
//[HttpPost]
//public async Task<IActionResult> PasswordChange(PasswordChangeViewModel request)
//{
//    // Eğer model geçerli değilse (örneğin, şifre değiştirme isteği geçerli değilse) sayfayı tekrar gösterir
//    if (!ModelState.IsValid)
//    {
//        return View();
//    }

//    var currentUser = (await _userManager.FindByNameAsync(User.Identity!.Name!))!;

//    // Girilen eski şifrenin doğruluğunu kontrol eder

//    // _userManager.CheckPasswordAsync(currentUser, request.PasswordOld), mevcut kullanıcının eski şifresini doğrular
//    var checkOldPassword = await _userManager.CheckPasswordAsync(currentUser, request.PasswordOld);

//    if (!checkOldPassword)
//    {
//        ModelState.AddModelError(string.Empty, "Eski şifreniz yanlış");
//        return View();
//    }

//    // Şifre değiştirme işlemini gerçekleştirir

//    // _userManager.ChangePasswordAsync(currentUser, request.PasswordOld, request.PasswordNew), kullanıcının şifresini değiştirir
//    var resultChangePassword = await _userManager.ChangePasswordAsync(currentUser, request.PasswordOld, request.PasswordNew);

//    if (!resultChangePassword.Succeeded)
//    {
//        ModelState.AddModelErrorList(resultChangePassword.Errors.Select(x => x.Description).ToList());
//        return View();
//    }

//    // Şifre değiştirme işlemi başarılıysa, kullanıcının SecurityStamp günceller, oturumu kapatır ve yeni şifreyle oturum açar

//    // _userManager.UpdateSecurityStampAsync(currentUser), kullanıcının SecurityStamp günceller
//    await _userManager.UpdateSecurityStampAsync(currentUser);

//    // _signInManager.SignOutAsync(), kullanıcıyı oturumdan çıkarır
//    await _signInManager.SignOutAsync();

//    // _signInManager.PasswordSignInAsync(currentUser, request.PasswordNew, true, false), yeni şifre ile oturum açar
//    await _signInManager.PasswordSignInAsync(currentUser, request.PasswordNew, true, false);

//    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir";
//    return View();
//}
