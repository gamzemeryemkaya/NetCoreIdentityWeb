using Microsoft.AspNetCore.Identity;
using NetCoreIdentityApp.Repository.Models;
using System.Security.Claims;

namespace NetCoreIdentityApp.Web.Seeds
{
    public class PermissionSeed
    {
        // Bu sınıf, rolleri ve izinleri başlatmak için kullanılır.

        public static async Task Seed(RoleManager<AppRole> roleManager)
        {
            // Temel roller (BasicRole, AdvancedRole, AdminRole) oluşturulur.

            // BasicRole kontrol edilir ve yoksa oluşturulur.
            var hasBasicRole = await roleManager.RoleExistsAsync("BasicRole");
            if (!hasBasicRole)
            {
                await roleManager.CreateAsync(new AppRole() { Name = "BasicRole" });
                var basicRole = (await roleManager.FindByNameAsync("BasicRole"))!;
                // BasicRole'a sadece okuma izni eklenir.
                await AddReadPermission(basicRole, roleManager);
            }

            // AdvancedRole kontrol edilir ve yoksa oluşturulur.
            var hasAdvancedRole = await roleManager.RoleExistsAsync("AdvancedRole");
            if (!hasAdvancedRole)
            {
                await roleManager.CreateAsync(new AppRole() { Name = "AdvancedRole" });
                var advancedRole = (await roleManager.FindByNameAsync("AdvancedRole"))!;
                // AdvancedRole'a okuma, oluşturma ve güncelleme izinleri eklenir.
                await AddReadPermission(advancedRole, roleManager);
                await AddUpdateAndCreatePermission(advancedRole, roleManager);
            }

            // AdminRole kontrol edilir ve yoksa oluşturulur.
            var hasAdminRole = await roleManager.RoleExistsAsync("AdminRole");
            if (!hasAdminRole)
            {
                await roleManager.CreateAsync(new AppRole() { Name = "AdminRole" });
                var adminRole = (await roleManager.FindByNameAsync("AdminRole"))!;
                // AdminRole'a okuma, oluşturma, güncelleme ve silme izinleri eklenir.
                await AddReadPermission(adminRole, roleManager);
                await AddUpdateAndCreatePermission(adminRole, roleManager);
                await AddDeletePermission(adminRole, roleManager);
            }
        }

        public static async Task AddReadPermission(AppRole role, RoleManager<AppRole> roleManager)
        {
            // Bir role, sadece okuma izinleri eklenir.
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Stock.Read));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Order.Read));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Catalog.Read));
        }

        public static async Task AddUpdateAndCreatePermission(AppRole role, RoleManager<AppRole> roleManager)
        {
            // Bir role, oluşturma ve güncelleme izinleri eklenir.
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Stock.Create));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Order.Create));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Catalog.Create));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Stock.Update));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Order.Update));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Catalog.Update));
        }

        public static async Task AddDeletePermission(AppRole role, RoleManager<AppRole> roleManager)
        {
            // Bir role, silme izinleri eklenir.
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Stock.Delete));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Order.Delete));
            await roleManager.AddClaimAsync(role, new Claim("Permission", Core.PermissionsRoot.Permissions.Catalog.Delete));
        }
    }
}
