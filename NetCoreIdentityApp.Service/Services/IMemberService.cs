using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using NetCoreIdentityApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NetCoreIdentityApp.Service.Services
{
    public interface IMemberService
    {
        Task<UserViewModel> GetUserViewModelByUserNameAsync(string userName);
        Task LogoutAsync();

        // Kullanıcının girdiği eski şifrenin doğruluğunu kontrol etmek için kullanılır.
        // Geri dönüş değeri, eski şifrenin doğru olup olmadığını belirtir.
        // - `userName`: Şifre değiştirilecek kullanıcının adı
        // - `password`: Kullanıcının girdiği eski şifre
        Task<bool> CheckPasswordAsync(string userName, string password);

        // Kullanıcının şifresini değiştirmek için kullanılır.
        // Şifre değiştirme işlemi başarılıysa, geri dönüş değeri (true, null) olur.
        // Şifre değiştirme işlemi başarısızsa, geri dönüş değeri (false, hata nedenleri) olur.
        // - `userName`: Şifresi değiştirilecek kullanıcının adı
        // - `oldPassword`: Kullanıcının eski şifresi
        // - `newPassword`: Kullanıcının yeni şifresi
        Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(string userName, string oldPassword, string newPassword);

        // Kullanıcının düzenleme sayfası görünüm modelini döndüren metod
        Task<UserEditViewModel> GetUserEditViewModelAsync(string userName);

        // Cinsiyet seçeneklerini içeren SelectList'i döndüren metod
        SelectList GetGenderSelectList();

        // Kullanıcının profil bilgilerini güncelleyen ve oturumu yöneten metod
        Task<(bool, IEnumerable<IdentityError>?)> EditUserAsync(UserEditViewModel request, string userName);

        List<ClaimViewModel> GetClaims(ClaimsPrincipal principal);
    }
}
