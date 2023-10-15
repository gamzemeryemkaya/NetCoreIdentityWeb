using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using NetCoreIdentityApp.Web.Extenisons;
using NetCoreIdentityApp.Web.Models;
using NetCoreIdentityApp.Web.ViewModels;
using System.Security.Claims;

namespace NetCoreIdentityApp.Web.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileProvider _fileProvider;

        // Dependency Injection ile SignInManager'ı alır
        public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IFileProvider fileProvider)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _fileProvider = fileProvider;
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
                PictureUrl = currentUser.Picture
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



        public IActionResult PasswordChange()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel request)
        {
            // Eğer model geçerli değilse (örneğin, şifre değiştirme isteği geçerli değilse) sayfayı tekrar gösterir
            if (!ModelState.IsValid)
            {
                return View();
            }

            var currentUser = (await _userManager.FindByNameAsync(User.Identity!.Name!))!;

            // Girilen eski şifrenin doğruluğunu kontrol eder

            // _userManager.CheckPasswordAsync(currentUser, request.PasswordOld), mevcut kullanıcının eski şifresini doğrular
            var checkOldPassword = await _userManager.CheckPasswordAsync(currentUser, request.PasswordOld);

            if (!checkOldPassword)
            {
                ModelState.AddModelError(string.Empty, "Eski şifreniz yanlış");
                return View();
            }

            // Şifre değiştirme işlemini gerçekleştirir

            // _userManager.ChangePasswordAsync(currentUser, request.PasswordOld, request.PasswordNew), kullanıcının şifresini değiştirir
            var resultChangePassword = await _userManager.ChangePasswordAsync(currentUser, request.PasswordOld, request.PasswordNew);

            if (!resultChangePassword.Succeeded)
            {
                ModelState.AddModelErrorList(resultChangePassword.Errors.Select(x => x.Description).ToList());
                return View();
            }

            // Şifre değiştirme işlemi başarılıysa, kullanıcının SecurityStamp günceller, oturumu kapatır ve yeni şifreyle oturum açar

            // _userManager.UpdateSecurityStampAsync(currentUser), kullanıcının SecurityStamp günceller
            await _userManager.UpdateSecurityStampAsync(currentUser);

            // _signInManager.SignOutAsync(), kullanıcıyı oturumdan çıkarır
            await _signInManager.SignOutAsync();

            // _signInManager.PasswordSignInAsync(currentUser, request.PasswordNew, true, false), yeni şifre ile oturum açar
            await _signInManager.PasswordSignInAsync(currentUser, request.PasswordNew, true, false);

            TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir";
            return View();
        }

        public async Task<IActionResult> UserEdit()
        {
            ViewBag.genderList = new SelectList(Enum.GetNames(typeof(Gender)));
            var currentUser = (await _userManager.FindByNameAsync(User.Identity!.Name!))!;

            var userEditViewModel = new UserEditViewModel()
            {
                UserName = currentUser.UserName!,
                Email = currentUser.Email!,
                Phone = currentUser.PhoneNumber!,
                BirthDate = currentUser.BirthDate,
                City = currentUser.City,
                Gender = currentUser.Gender,
            };

            return View(userEditViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> UserEdit(UserEditViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);

            currentUser.UserName = request.UserName;
            currentUser.Email = request.Email;
            currentUser.PhoneNumber = request.Phone;
            currentUser.BirthDate = request.BirthDate;
            currentUser.City = request.City;
            currentUser.Gender = request.Gender;

            if (request.Picture != null && request.Picture.Length > 0)
            {
                var wwwrootFolder = _fileProvider.GetDirectoryContents("wwwroot");

                string randomFileName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(request.Picture.FileName)}";

                var newPicturePath = Path.Combine(wwwrootFolder!.First(x => x.Name == "userpictures").PhysicalPath!, randomFileName);

                using var stream = new FileStream(newPicturePath, FileMode.Create);

                await request.Picture.CopyToAsync(stream);

                currentUser.Picture = randomFileName;
            }

            var updateToUserResult = await _userManager.UpdateAsync(currentUser);

            if (!updateToUserResult.Succeeded)
            {
                ModelState.AddModelErrorList(updateToUserResult.Errors);
                return View();
            }

            await _userManager.UpdateSecurityStampAsync(currentUser);
            await _signInManager.SignOutAsync();

            if (request.BirthDate.HasValue)
            {
                await _signInManager.SignInWithClaimsAsync(currentUser, true, new[] { new Claim("birthdate", currentUser.BirthDate!.Value.ToString()) });
            }

            else
            {
                await _signInManager.SignInAsync(currentUser, true);
            }

            TempData["SuccessMessage"] = "Üye bilgileri başarıyla değiştirilmiştir";

            var userEditViewModel = new UserEditViewModel()
            {
                UserName = currentUser.UserName!,
                Email = currentUser.Email!,
                Phone = currentUser.PhoneNumber!,
                BirthDate = currentUser.BirthDate,
                City = currentUser.City,
                Gender = currentUser.Gender,
            };

            return View(userEditViewModel);
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
            var userClaimList = User.Claims.Select(x => new ClaimViewModel()
            {
                Issuer = x.Issuer,
                Type = x.Type,
                Value = x.Value
            }).ToList();

            return View(userClaimList);

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

