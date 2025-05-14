using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Models.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string toEmail, string confirmationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendVerificationEmail(string toEmail, string confirmationLink)
        {
            try
            {
                _logger.LogInformation("Starting email sending process to {Email}", toEmail);

                var message = new MimeMessage();
                var fromEmail = _configuration["SmtpSettings:FromEmail"];
                var fromName = _configuration["SmtpSettings:FromName"];

                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName))
                {
                    throw new InvalidOperationException("SMTP FromEmail or FromName is not configured");
                } 

                message.From.Add(new MailboxAddress(fromName, fromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Xác thực email của bạn";
                message.Headers.Add("X-Priority", "1");
                message.Headers.Add("X-MSMail-Priority", "High");
                message.Headers.Add("Importance", "High");
                message.Headers.Add("X-Mailer", "MyShop Mailer");

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $@"Xin chào,

Cảm ơn bạn đã đăng ký tài khoản tại MyShop.

Vui lòng xác thực email của bạn bằng cách nhấp vào liên kết sau:
{confirmationLink}

Liên kết này sẽ hết hạn sau 24 giờ.

Nếu bạn không yêu cầu xác thực email này, vui lòng bỏ qua email này.

Trân trọng,
MyShop Team",
                    HtmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='utf-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Xác thực email của bạn</title>
                        </head>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px;'>
                                <h2 style='color: #2c3e50; margin-bottom: 20px;'>Xác thực email của bạn</h2>
                                <p>Xin chào,</p>
                                <p>Cảm ơn bạn đã đăng ký tài khoản tại MyShop. Vui lòng xác thực email của bạn bằng cách nhấp vào nút bên dưới:</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{confirmationLink}' 
                                       style='background-color: #4CAF50; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                        Xác thực email
                                    </a>
                                </div>
                                <p style='font-size: 14px; color: #666;'>Hoặc bạn có thể copy và paste đường link sau vào trình duyệt:</p>
                                <p style='font-size: 14px; color: #666; word-break: break-all;'>{confirmationLink}</p>
                                <p style='font-size: 14px; color: #666;'>Liên kết này sẽ hết hạn sau 24 giờ.</p>
                                <p style='font-size: 14px; color: #666;'>Nếu bạn không yêu cầu xác thực email này, vui lòng bỏ qua email này.</p>
                                <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                                <p style='font-size: 14px; color: #666;'>Trân trọng,<br>MyShop Team</p>
                            </div>
                        </body>
                        </html>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                var server = _configuration["SmtpSettings:Server"];
                var port = _configuration["SmtpSettings:Port"];
                var username = _configuration["SmtpSettings:Username"];
                var password = _configuration["SmtpSettings:Password"];

                if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(port) || 
                    string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("SMTP settings are not properly configured");
                }

                _logger.LogInformation("Connecting to SMTP server {Server}:{Port}", server, port);

                using var client = new SmtpClient();
                try
                {
                    await client.ConnectAsync(server, int.Parse(port), SecureSocketOptions.StartTls);
                    _logger.LogInformation("Connected to SMTP server successfully");

                    await client.AuthenticateAsync(username, password);
                    _logger.LogInformation("Authenticated with SMTP server successfully");

                    await client.SendAsync(message);
                    _logger.LogInformation("Email sent successfully to {Email}", toEmail);

                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Disconnected from SMTP server");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during SMTP operation: {Message}", ex.Message);
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}. Error: {ErrorMessage}", toEmail, ex.Message);
                throw new Exception($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}