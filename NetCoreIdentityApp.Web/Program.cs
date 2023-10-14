using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NetCoreIdentityApp.Web.Extenisons;
using NetCoreIdentityApp.Web.Models;
using NetCoreIdentityApp.Web.OptionsModels;
using NetCoreIdentityApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//------------------------------
//// Ba?lant? dizesi, "SqlCon" adl? yap?land?rma ayar?ndan al?n?r.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"));
});


//---------------------------------------
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

//---------------------------------------
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));


//--------------------------------

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddIdentityWithExt();

builder.Services.AddScoped<IEmailService, EmailService>();


//------------------------------
//Servislerin yap?land?r?lmas? s?ras?nda Application Cookie için yap?land?rmalar? belirler
builder.Services.ConfigureApplicationCookie(opt =>
{
    // Yeni bir çerez yap?land?r?c?s? olu?turulur
    var cookieBuilder = new CookieBuilder();

    // Çerezin ad?n? belirler ("IdentityAppCookie" olarak ayarlanm??)
    cookieBuilder.Name = "IdentityAppCookie";

    // Giri? yapma sayfas?n? belirler (Kullan?c? giri? yapmad???nda yönlendirilece?i sayfa)
    opt.LoginPath = new PathString("/Home/Signin");
    // Logout i?lemi için belirtilen yolun yap?land?r?lmas?
    opt.LogoutPath = new PathString("/Member/logout");
    opt.AccessDeniedPath = new PathString("/Member/AccessDenied");

    // Çerez yap?land?rmas?n? belirler
    opt.Cookie = cookieBuilder;

    // Çerezin ne kadar süre boyunca geçerli olaca??n? belirler (60 gün)
    opt.ExpireTimeSpan = TimeSpan.FromDays(60);

    // "SlidingExpiration" özelli?i, kullan?c?n?n her istek yapt???nda çerezin süresinin s?f?rlan?p s?f?rlanmayaca??n? belirler
    opt.SlidingExpiration = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
