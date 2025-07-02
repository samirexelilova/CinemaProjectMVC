using Microsoft.Extensions.Options;
using StreamitMVC.Models;
using StreamitMVC.Services.Interfaces;
using System.Net.Mail;
using System.Net;

namespace StreamitMVC.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var smtpClient = new SmtpClient(_emailSettings.SmtpServer)
                {
                    Port = _emailSettings.Port,
                    Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                    EnableSsl = _emailSettings.EnableSSL,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Email göndərmə xətası: {ex.Message}", ex);
            }
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            string subject = "Email Təsdiqlənməsi - Streamit";
            string message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .button {{ 
                            background-color: #007bff; 
                            color: white; 
                            padding: 12px 24px; 
                            text-decoration: none; 
                            border-radius: 5px; 
                            display: inline-block;
                            margin: 10px 0;
                        }}
                        .header {{ color: #333; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2 class='header'>Email Təsdiqlənməsi</h2>
                        <p>Salam,</p>
                        <p>Streamit platformasına qeydiyyatınızı tamamlamaq üçün aşağıdakı linkə klik edin:</p>
                        <p><a href='{confirmationLink}' class='button'>Email-i Təsdiqlə</a></p>
                        <p>Əgər yuxarıdakı düymə işləmirsə, aşağıdakı linki brauzerinizə kopyalayın:</p>
                        <p style='word-break: break-all;'>{confirmationLink}</p>
                        <p>Bu link 24 saat ərzində keçərlidir.</p>
                        <p>Təşəkkürlər,<br><strong>Streamit Komandası</strong></p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            string subject = "Şifrə Sıfırlama - Streamit";
            string message = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .button {{ 
                            background-color: #dc3545; 
                            color: white; 
                            padding: 12px 24px; 
                            text-decoration: none; 
                            border-radius: 5px; 
                            display: inline-block;
                            margin: 10px 0;
                        }}
                        .header {{ color: #333; }}
                        .warning {{ color: #856404; background-color: #fff3cd; padding: 10px; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h2 class='header'>Şifrə Sıfırlama</h2>
                        <p>Salam,</p>
                        <p>Şifrənizi sıfırlamaq üçün sorğu daxil olub. Şifrənizi dəyişmək üçün aşağıdakı linkə klik edin:</p>
                        <p><a href='{resetLink}' class='button'>Şifrəni Sıfırla</a></p>
                        <p>Əgər yuxarıdakı düymə işləmirsə, aşağıdakı linki brauzerinizə kopyalayın:</p>
                        <p style='word-break: break-all;'>{resetLink}</p>
                        <div class='warning'>
                            <strong>Təhlükəsizlik:</strong> Bu link 1 saat ərzində keçərlidir. Əgər bu sorğu sizin tərəfinizdən edilməyibsə, bu email-i nəzərə almayın.
                        </div>
                        <p>Təşəkkürlər,<br><strong>Streamit Komandası</strong></p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, message);
        }
    }
}
