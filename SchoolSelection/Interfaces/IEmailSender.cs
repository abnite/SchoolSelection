namespace SchoolSelection.Interfaces;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string message);
    Task SendGmailEmailAsync(string email, string subject, string message);
    Task ContactSendEmailAsync(string from, string subject, string message);
    Task ContactSendGmailEmailAsync(string email, string subject, string message);
    public Task Execute(string apiKey, string subject, string message, string email);
}