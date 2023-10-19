using AspNetCoreIdentityApp.Web.Requirements;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetCoreIdentityApp.Web.ClaimProvider;
using NetCoreIdentityApp.Web.Extenisons;
using NetCoreIdentityApp.Repository.Models;
using NetCoreIdentityApp.Core.OptionsModels;
using NetCoreIdentityApp.Core.PermissionsRoot;
using NetCoreIdentityApp.Web.Requirements;
using NetCoreIdentityApp.Web.Seeds;
using NetCoreIdentityApp.Service.Services;
using Microsoft.AspNetCore.Authentication.Facebook;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


//------------------------------
//// Ba?lant? dizesi, "SqlCon" adl? yap?land?rma ayar?ndan al?n?r.
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlCon"), options =>
    {
        options.MigrationsAssembly("NetCoreIdentityApp.Repository");
    }
     );
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
//--------------------------------
builder.Services.AddScoped<IClaimsTransformation, UserClaimProvider>();
//--
builder.Services.AddScoped<IAuthorizationHandler, ExchangeExpireRequirementHandler>();
//--
builder.Services.AddScoped<IAuthorizationHandler, ViolenceRequirementHandler>();
//-------
builder.Services.AddScoped<IMemberService,MemberService>();


builder.Services.AddAuthorization(options =>
{
    // claim bazl? - ?ehire göre yetkilendirme 
    options.AddPolicy("AntalyaPolicy", policy =>
    {
        policy.RequireClaim("city", "antalya");


    });
    //policy bazl? -- 10 gün süre ile sayfaya eri?me
    options.AddPolicy("ExchangePolicy", policy =>
    {
        policy.AddRequirements(new ExchangeExpireRequirement());


    });
    //18 ya??ndan küçük kabul etmemek için
    options.AddPolicy("ViolencePolicy", policy =>
    {
        policy.AddRequirements(new ViolenceRequirement() { ThresholdAge = 18 });


    });
        // "OrderPermissionReadAndDelete" ad?nda bir politika olu?turulur.
        // Bu politika, "Permissions.Order.Read", "Permissions.Order.Delete" ve "Permissions.Stock.Delete" izinlerini gerektirir.
        options.AddPolicy("OrderPermissionReadAndDelete", policy =>
        {
            policy.RequireClaim("permission", Permissions.Order.Read);
            policy.RequireClaim("permission", Permissions.Order.Delete);
            policy.RequireClaim("permission", Permissions.Stock.Delete);
        });

        // "Permissions.Order.Read" ad?nda bir politika olu?turulur.
        // Bu politika yaln?zca "Permissions.Order.Read" iznine sahip kullan?c?lar? kabul eder.
        options.AddPolicy("Permissions.Order.Read", policy =>
        {
            policy.RequireClaim("permission", Permissions.Order.Read);
        });

        // "Permissions.Order.Delete" ad?nda bir politika olu?turulur.
        // Bu politika yaln?zca "Permissions.Order.Delete" iznine sahip kullan?c?lar? kabul eder.
        options.AddPolicy("Permissions.Order.Delete", policy =>
        {
            policy.RequireClaim("permission", Permissions.Order.Delete);
        });

        // "Permissions.Stock.Delete" ad?nda bir politika olu?turulur.
        // Bu politika yaln?zca "Permissions.Stock.Delete" iznine sahip kullan?c?lar? kabul eder.
        options.AddPolicy("Permissions.Stock.Delete", policy =>
        {
            policy.RequireClaim("permission", Permissions.Stock.Delete);
        });



});
builder.Services.AddIdentityWithExt();

builder.Services.AddAuthentication()
    .AddFacebook(opts =>
    {
        opts.AppId = "1038624207484121";
        opts.AppSecret = "95559a9ec7537e49b2ab0bb96bbe70db";
    }).AddGoogle(opts =>
    {
        opts.ClientId = "256248559922-n5t4nd3sj4fbgarm1qfp69n5jg2u34he.apps.googleusercontent.com";

        opts.ClientSecret = "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";
    });





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
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    await PermissionSeed.Seed(roleManager);
}

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
