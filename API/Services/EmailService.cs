using MailKit.Net.Smtp;
using MimeKit;

namespace API.Services
{
    public interface IEmailService
    {
        Task<bool> SendResetPasswordEmailAsync(string toEmail, string resetCode);
        Task<bool> SendApplicationStatusEmailAsync(string toEmail, string studentName, string jobTitle, string status, string? providerName);
        Task<bool> SendNewApplicationEmailAsync(string toEmail, string providerName, string jobTitle, string studentName, string studentPhone, string studentYear, string workType);
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

        public async Task<bool> SendResetPasswordEmailAsync(string toEmail, string resetCode)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var password = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email configuration is not set properly");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Mã xác thực đặt lại mật khẩu - Project PRN232";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GetEmailTemplate(resetCode)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
				client.LocalDomain = "localhost";
				await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Reset password email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send reset password email to {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendApplicationStatusEmailAsync(string toEmail, string studentName, string jobTitle, string status, string? providerName)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var password = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email configuration is not set properly");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(studentName, toEmail));
                message.Subject = $"Thông báo về đơn ứng tuyển - {jobTitle}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GetApplicationStatusTemplate(studentName, jobTitle, status, providerName ?? "Nhà tuyển dụng")
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.LocalDomain = "localhost";
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Application status email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send application status email to {toEmail}");
                return false;
            }
        }

        private string GetEmailTemplate(string resetCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .code-box {{
            background-color: #fff3cd;
            border: 2px dashed #ffc107;
            border-radius: 8px;
            padding: 20px;
            text-align: center;
            margin: 30px 0;
        }}
        .code {{
            font-size: 36px;
            font-weight: bold;
            letter-spacing: 8px;
            color: #333;
            font-family: 'Courier New', monospace;
        }}
        .info {{
            background-color: #e7f3ff;
            border-left: 4px solid #2196F3;
            padding: 15px;
            margin: 20px 0;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
        .warning {{
            color: #dc3545;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Đặt Lại Mật Khẩu</h1>
        </div>
        <div class='content'>
            <p>Xin chào,</p>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình. Sử dụng mã xác thực bên dưới để tiếp tục:</p>
            
            <div class='code-box'>
                <p style='margin: 0 0 10px 0; color: #856404;'>Mã xác thực của bạn:</p>
                <div class='code'>{resetCode}</div>
            </div>

            <div class='info'>
                <p style='margin: 0;'><strong>⏰ Lưu ý:</strong> Mã này chỉ có hiệu lực trong <strong>15 phút</strong>.</p>
            </div>

            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            
            <p class='warning'>⚠️ Không chia sẻ mã này với bất kỳ ai!</p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống Project PRN232</p>
            <p>© 2025 Project PRN232. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetApplicationStatusTemplate(string studentName, string jobTitle, string status, string providerName)
        {
            var statusText = status switch
            {
                "Accepted" => "CHẤP NHẬN",
                "Rejected" => "TỪ CHỐI",
                _ => status.ToUpper()
            };

            var statusColor = status switch
            {
                "Accepted" => "#28a745",
                "Rejected" => "#dc3545",
                _ => "#6c757d"
            };

            var statusIcon = status switch
            {
                "Accepted" => "✅",
                "Rejected" => "❌",
                _ => "📋"
            };

            var message = status switch
            {
                "Accepted" => $"Chúc mừng! Đơn ứng tuyển của bạn cho vị trí <strong>{jobTitle}</strong> đã được chấp nhận bởi {providerName}.",
                "Rejected" => $"Rất tiếc! Đơn ứng tuyển của bạn cho vị trí <strong>{jobTitle}</strong> đã bị từ chối bởi {providerName}.",
                _ => $"Đơn ứng tuyển của bạn cho vị trí <strong>{jobTitle}</strong> đã được cập nhật trạng thái."
            };

            var nextStep = status switch
            {
                "Accepted" => "Vui lòng đăng nhập vào hệ thống để xem chi tiết và thực hiện check-in khi làm việc.",
                "Rejected" => "Đừng nản lòng! Hãy tiếp tục ứng tuyển các công việc khác phù hợp với bạn.",
                _ => "Vui lòng đăng nhập vào hệ thống để xem chi tiết."
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .status-box {{
            background-color: #f8f9fa;
            border-left: 5px solid {statusColor};
            border-radius: 4px;
            padding: 20px;
            margin: 25px 0;
            text-align: center;
        }}
        .status-title {{
            font-size: 28px;
            font-weight: bold;
            color: {statusColor};
            margin: 10px 0;
        }}
        .job-info {{
            background-color: #e7f3ff;
            border-radius: 8px;
            padding: 20px;
            margin: 20px 0;
        }}
        .job-info p {{
            margin: 8px 0;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
            font-weight: bold;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📢 Thông Báo Từ PRN232 Jobs</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{studentName}</strong>,</p>
            
           

            <p>{message}</p>

            <div class='job-info'>
                <p><strong>📋 Vị trí công việc:</strong> {jobTitle}</p>
                <p><strong>🏢 Nhà tuyển dụng:</strong> {providerName}</p>
                <p><strong>📊 Trạng thái:</strong> <span style='color: {statusColor}; font-weight: bold;'>{statusText}</span></p>
            </div>

            <p>{nextStep}</p>

           

            <p style='margin-top: 30px; color: #6c757d; font-size: 14px;'>
                💡 Mẹo: Thường xuyên kiểm tra email và hệ thống để không bỏ lỡ các cơ hội việc làm mới!
            </p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống PRN232 Jobs</p>
            <p>© 2025 Project PRN232. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public async Task<bool> SendNewApplicationEmailAsync(string toEmail, string providerName, string jobTitle, string studentName, string studentPhone, string studentYear, string workType)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderName = _configuration["EmailSettings:SenderName"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var password = _configuration["EmailSettings:Password"];

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(password))
                {
                    _logger.LogWarning("Email configuration is not set properly");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(senderName, senderEmail));
                message.To.Add(new MailboxAddress(providerName, toEmail));
                message.Subject = $"Đơn ứng tuyển mới cho vị trí {jobTitle}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GetNewApplicationTemplate(providerName, jobTitle, studentName, studentPhone, studentYear, workType)
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                client.LocalDomain = "localhost";
                await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(senderEmail, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"New application email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send new application email to {toEmail}");
                return false;
            }
        }

        private string GetNewApplicationTemplate(string providerName, string jobTitle, string studentName, string studentPhone, string studentYear, string workType)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 40px 30px;
        }}
        .highlight-box {{
            background-color: #fff3cd;
            border-left: 5px solid #ffc107;
            border-radius: 4px;
            padding: 20px;
            margin: 25px 0;
        }}
        .info-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        .info-table td {{
            padding: 12px;
            border-bottom: 1px solid #e9ecef;
        }}
        .info-table td:first-child {{
            font-weight: bold;
            color: #495057;
            width: 40%;
        }}
        .button {{
            display: inline-block;
            padding: 12px 30px;
            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
            color: white;
            text-decoration: none;
            border-radius: 5px;
            margin: 20px 0;
            font-weight: bold;
            box-shadow: 0 4px 6px rgba(40, 167, 69, 0.3);
            transition: all 0.3s ease;
        }}
        .button:hover {{
            background: linear-gradient(135deg, #20c997 0%, #28a745 100%);
            box-shadow: 0 6px 8px rgba(40, 167, 69, 0.4);
            transform: translateY(-2px);
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            color: #6c757d;
            font-size: 12px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Đơn Ứng Tuyển Mới</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{providerName}</strong>,</p>
            
            <div class='highlight-box'>
                <p style='margin: 0; font-size: 16px;'>
                    <strong>📢 Bạn có một đơn ứng tuyển mới cho vị trí:</strong>
                    <div style='font-size: 20px; color: #667eea; margin-top: 10px;'>{jobTitle}</div>
                </p>
            </div>

            <h3 style='color: #333; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>
                👤 Thông tin ứng viên
            </h3>
            
            <table class='info-table'>
                <tr>
                    <td>📝 Họ và tên:</td>
                    <td><strong>{studentName}</strong></td>
                </tr>
                <tr>
                    <td>📞 Số điện thoại:</td>
                    <td><strong>{studentPhone}</strong></td>
                </tr>
                <tr>
                    <td>🎓 Năm học:</td>
                    <td>{studentYear}</td>
                </tr>
                <tr>
                    <td>💼 Loại công việc:</td>
                    <td>{workType}</td>
                </tr>
            </table>

            <p style='background-color: #e7f3ff; padding: 15px; border-radius: 5px; border-left: 4px solid #2196F3;'>
                <strong>💡 Lưu ý:</strong> Vui lòng đăng nhập vào hệ thống để xem chi tiết đơn ứng tuyển và phê duyệt hoặc từ chối.
            </p>

           

            <p style='margin-top: 30px; color: #6c757d; font-size: 14px;'>
                ⏰ Thời gian nhận đơn: {DateTime.Now:dd/MM/yyyy HH:mm}
            </p>
        </div>
        <div class='footer'>
            <p>Email này được gửi tự động từ hệ thống PRN232 Jobs</p>
            <p>© 2025 Project PRN232. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
