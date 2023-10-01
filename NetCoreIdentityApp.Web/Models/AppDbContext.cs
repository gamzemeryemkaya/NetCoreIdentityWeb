using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace NetCoreIdentityApp.Web.Models
{
    //// Bu sınıf, ASP.NET Core Identity tarafından kullanılan veritabanı bağlantısını temsil eder.
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        //Bu metot, veritabanı bağlantısını yapılandırmak için kullanılır.
        // DbContextOptions, veritabanı bağlantısı için gerekli yapılandırma seçeneklerini içerir.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
