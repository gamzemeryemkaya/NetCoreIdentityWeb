using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using NetCoreIdentityApp.Core.Models;
using NetCoreIdentityApp.Core.ViewModels;
using NetCoreIdentityApp.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreIdentityApp.Service.Services
{
    public class MemberService:IMemberService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IFileProvider _fileProvider;

        public MemberService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IFileProvider fileProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileProvider = fileProvider;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<UserViewModel> GetUserViewModelByUserNameAsync(string userName)
        {

            // Kullanıcı bilgilerini görüntülemek için mevcut kullanıcıyı alır

            // User.Identity!.Name!, mevcut kimlik altında kullanıcı adını alır ve null olmadığını doğrular
            var currentUser = (await _userManager.FindByNameAsync(userName))!;

            // UserViewModel sınıfını kullanarak kullanıcı bilgilerini görüntülemek için bir görünüm modeli oluşturur

            return new UserViewModel
            {
                // Kullanıcının e-posta adresini alır ve UserViewModel'e ekler
                Email = currentUser.Email,

                // Kullanıcının kullanıcı adını alır ve UserViewModel'e ekler
                UserName = currentUser.UserName,

                // Kullanıcının telefon numarasını alır ve UserViewModel'e ekler
                PhoneNumber = currentUser.PhoneNumber,
                PictureUrl = currentUser.Picture
            };
        }

        //Yeni kod, şifre doğrulama ve değiştirme işlemlerini ayrı metodlarda ele alır

        public async Task<bool> CheckPasswordAsync(string userName, string password)
        {
            // Kullanıcı adına göre mevcut kullanıcıyı bulur
            var currentUser = (await _userManager.FindByNameAsync(userName))!;

            // Girilen şifreyi, mevcut kullanıcının şifresiyle karşılaştırır
            return await _userManager.CheckPasswordAsync(currentUser, password);
        }

        public async Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            // Kullanıcı adına göre mevcut kullanıcıyı bulur
            var currentUser = (await _userManager.FindByNameAsync(userName))!;

            // Şifre değiştirme işlemini gerçekleştirir
            var resultChangePassword = await _userManager.ChangePasswordAsync(currentUser, oldPassword, newPassword);

            // Eğer şifre değiştirme işlemi başarısızsa, hata mesajlarını döner
            if (!resultChangePassword.Succeeded)
            {
                return (false, resultChangePassword.Errors);
            }

            // Kullanıcının güvenlik damgasını günceller
            await _userManager.UpdateSecurityStampAsync(currentUser);

            // Kullanıcıyı oturumdan çıkarır
            await _signInManager.SignOutAsync();

            // Yeni şifre ile kullanıcıyı tekrar oturum açar
            await _signInManager.PasswordSignInAsync(currentUser, newPassword, true, false);

            // Şifre değiştirme işlemi başarılı ise (true, null) döner
            return (true, null);
        }


        public async Task<UserEditViewModel> GetUserEditViewModelAsync(string userName)
        {
            // Kullanıcının bilgilerini çekerek düzenleme sayfası görünüm modelini oluşturur
            var currentUser = (await _userManager.FindByNameAsync(userName))!;

            return new UserEditViewModel()
            {
                UserName = currentUser.UserName!,
                Email = currentUser.Email!,
                Phone = currentUser.PhoneNumber!,
                BirthDate = currentUser.BirthDate,
                City = currentUser.City,
                Gender = currentUser.Gender,
            };
        }

        public SelectList GetGenderSelectList()
        {
            // Cinsiyet seçeneklerini içeren SelectList'i oluşturur
            return new SelectList(Enum.GetNames(typeof(Gender)));
        }

        public async Task<(bool, IEnumerable<IdentityError>?)> EditUserAsync(UserEditViewModel request, string userName)
        {
            // Kullanıcının bilgilerini günceller ve oturumu yönetir
            var currentUser = (await _userManager.FindByNameAsync(userName))!;

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
                return (false, updateToUserResult.Errors);
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

            return (true, null);
        }

        public List<ClaimViewModel> GetClaims(ClaimsPrincipal principal)
        {
            return principal.Claims.Select(x => new ClaimViewModel()
            {
                Issuer = x.Issuer,
                Type = x.Type,
                Value = x.Value
            }).ToList();

        }
    }



}
