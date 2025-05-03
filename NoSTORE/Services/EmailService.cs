using Microsoft.Extensions.Options;
using MimeKit;
using NoSTORE.Settings;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace NoSTORE.Services
{
    public class EmailService(IOptions<SmtpSettings> smtp)
    {
        public async Task<bool> SendCodeAsync(string toEmail, string code, CancellationToken ct)
        {
            try
            {
                MimeMessage msg = new();
                msg.From.Add(new MailboxAddress("NoSTORE", smtp.Value.EmailAddress));
                msg.To.Add(MailboxAddress.Parse(toEmail));
                msg.Subject = "Код подтверждения";
                msg.Body = new TextPart("plain")
                {
                    Text = $"Ваш код: {code}"
                };

                MailKit.Net.Smtp.SmtpClient client = new();

                await client.ConnectAsync("smtp.mail.ru", cancellationToken: ct);
                await client.AuthenticateAsync(smtp.Value.EmailAddress, smtp.Value.EmailPassword, ct);
                await client.SendAsync(msg, ct);
                return true;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                Console.WriteLine("Запрос отменён");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отправке email: {ex.Message}");
                return false;
            }

        }
    }
}
