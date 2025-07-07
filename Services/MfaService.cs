using System.Net.Mail;
using System.Net;

public class MfaService
{
    private readonly Dictionary<string, (string Code, DateTime Expiration)> _codes = new();
    private readonly IConfiguration _config;

    public MfaService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateCode(string email)
    {
        var code = new Random().Next(100000, 999999).ToString();
        var expiration = DateTime.UtcNow.AddMinutes(10); // Kód 10 percig érvényes
        _codes[email] = (code, expiration);

        SendEmail(email, code);

        return code;
    }
    public void SendResetCodeEmail(string toEmail, string code)
    {
        var smtpHost = _config["Smtp:Host"];
        var smtpPort = int.Parse(_config["Smtp:Port"]!);
        var smtpUser = _config["Smtp:Username"];
        var smtpPass = _config["Smtp:Password"];
        var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]!);
        if (string.IsNullOrEmpty(smtpUser))
            throw new InvalidOperationException("SMTP username is not configured properly.");

        var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = enableSsl
        };

        var mail = new MailMessage
        {
            From = new MailAddress(smtpUser),
            Subject = "Password Reset Code",
            Body = $"Your password reset code is: {code}"
        };

        mail.To.Add(toEmail);
        client.Send(mail);
    }

    private void SendEmail(string toEmail, string code)
    {
        var smtpHost = _config["Smtp:Host"];
        var smtpPort = int.Parse(_config["Smtp:Port"]!);
        var smtpUser = _config["Smtp:Username"];
        var smtpPass = _config["Smtp:Password"];
        var enableSsl = bool.Parse(_config["Smtp:EnableSsl"]!);

        if (string.IsNullOrEmpty(smtpUser))
            throw new InvalidOperationException("SMTP username is not configured properly.");

        var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = enableSsl
        };

        var mail = new MailMessage
        {
            From = new MailAddress(smtpUser),
            Subject = "MFA kód",
            Body = $"A belépéshez szükséges MFA kódod: {code}"
        };

        mail.To.Add(toEmail);
        client.Send(mail);
    }

    public bool ValidateCode(string email, string code)
    {
        if (!_codes.ContainsKey(email))
            return false;

        var (storedCode, expiration) = _codes[email];

        if (DateTime.UtcNow > expiration)
        {
            _codes.Remove(email);
            return false;
        }

        return storedCode == code;
    }

    public void ClearCode(string email) =>
        _codes.Remove(email);
}