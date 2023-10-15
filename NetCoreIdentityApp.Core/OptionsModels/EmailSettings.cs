namespace NetCoreIdentityApp.Core.OptionsModels
{
    //appsettings.Development.json dosyasındaki yapılandırma ayarlarını kod içinde kullanmak için kullanılır
    public class EmailSettings
    {
        //Host: E-posta göndermek için kullanılacak SMTP sunucusunun adresini (smtp.gmail.com gibi) temsil eder.
        public string Host { get; set; } = null!;
        //Password: SMTP sunucusuna erişim için kullanılacak şifreyi temsil eder. Bu şifre, e-posta hesabının güvenliğini sağlar.
        public string Password { get; set; } = null!;
        //Email: E-postaların gönderileceği ve bu ayarların ilişkilendirildiği e-posta adresini temsil eder. Bu, genellikle bir uygulamanın iletişim kurduğu e-posta hesabını belirtir.
        public string Email { get; set; } = null!;
    }
}
