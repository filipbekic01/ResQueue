using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using ResQueue.Models;

namespace ResQueue;

public class EmailSender(
    IOptions<Settings> settings
) : IEmailSender<User>
{
    public async Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ResQueue", "no-reply@resqueue.io"));
        message.To.Add(new MailboxAddress(user.UserName, email));
        message.Subject = "ResQueue — Account Confirmation";

        message.Body = new TextPart("html")
        {
            Text = $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Value.SmtpHost, settings.Value.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings.Value.SmtpUsername, settings.Value.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        return Task.CompletedTask;
    }

    public async Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("ResQueue", "no-reply@resqueue.io"));
        message.To.Add(new MailboxAddress(user.UserName, email));
        message.Subject = "ResQueue — Reset Password Code";

        message.Body = new TextPart("html")
        {
            Text = $"Your password reset code is: {resetCode}"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(settings.Value.SmtpHost, settings.Value.SmtpPort, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(settings.Value.SmtpUsername, settings.Value.SmtpPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}