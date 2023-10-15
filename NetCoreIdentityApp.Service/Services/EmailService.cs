using Microsoft.Extensions.Options;
using NetCoreIdentityApp.Core.OptionsModels;
using System.Net.Mail;
using System.Net;

namespace NetCoreIdentityApp.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        // EmailService sınıfının yapıcı metodu, yapılandırma ayarlarını enjekte eder
        public EmailService(IOptions<EmailSettings> options)
        {
            // IOptions ile EmailSettings ayarlarını alır
            _emailSettings = options.Value;
        }

        // Şifre sıfırlama e-postası gönderen metot
        public async Task SendResetPasswordEmail(string resetPasswordEmailLink, string ToEmail)
        {
            var smptClient = new SmtpClient();

            // SMTP sunucusunun adresini ayarlar
            smptClient.Host = _emailSettings.Host;

            smptClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smptClient.UseDefaultCredentials = false;
            smptClient.Port = 587;

            // Kimlik bilgilerini ayarlar (E-posta ve şifre)
            smptClient.Credentials = new NetworkCredential(_emailSettings.Email, _emailSettings.Password);
            smptClient.EnableSsl = true;

            // E-posta mesajını oluşturur
            var mailMessage = new MailMessage();

            // Gönderen e-posta adresini ayarlar
            mailMessage.From = new MailAddress(_emailSettings.Email);

            // Alıcı e-posta adresini ayarlar
            mailMessage.To.Add(ToEmail);

            // E-posta konusunu ayarlar
            mailMessage.Subject = "Localhost | Şifre sıfırlama linki";

            // E-posta gövdesini oluşturur (HTML formatında)
            mailMessage.Body = $@"
                  <h4>Şifrenizi yenilemek için aşağıdaki linkte tıklayınız.</h4>
                  <p><a href='{resetPasswordEmailLink}'>şifre yenileme link</a></p>";

            // E-posta gövdesinin HTML formatında olduğunu belirtir
            mailMessage.IsBodyHtml = true;

            // E-postayı SMTP sunucusu aracılığıyla gönderir
            await smptClient.SendMailAsync(mailMessage);
        }
    }
}
