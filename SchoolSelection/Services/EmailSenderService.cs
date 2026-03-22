using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using SchoolSelection.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Project_Articles.Services;

public class EmailSenderService:IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;


    public EmailSenderService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }
   
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        try
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            string smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            string smtpPassword = _configuration["EmailSettings:SmtpPassword"];

            using (SmtpClient smtpClient = new SmtpClient(smtpServer))
            {
                smtpClient.Port = smtpPort;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true; // Enable SSL for secure email sending

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpUsername);
                mailMessage.To.Add(email);
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;

                //smtpClient.Send(mailMessage);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
        
    }

    public async Task Execute(string apiKey, string subject, string message, string email)
    {
        throw new NotImplementedException();
    }
    
    public async Task SendGmailEmailAsync(string email, string subject, string message)
    {
        try
        {
            string smtpServer = _configuration["GmailEmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["GmailEmailSettings:SmtpPort"]);
            string smtpUsername = _configuration["GmailEmailSettings:SmtpUsername"];
            string smtpPassword = _configuration["GmailEmailSettings:SmtpPassword"];
            string appPassword = _configuration["GmailEmailSettings:AppPassword"]; // Use the generated App Password


            using (SmtpClient smtpClient = new SmtpClient(smtpServer))
            {
                smtpClient.Port = smtpPort;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, appPassword);
                smtpClient.EnableSsl = true; // Enable SSL for secure email sending

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(smtpUsername);
                mailMessage.To.Add(email);
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
        
    }
    
    public async Task ContactSendEmailAsync(string fromEmail, string subject, string message)
    {
        try
        {
            string smtpServer = _configuration["EmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
            string smtpUsername = _configuration["EmailSettings:SmtpUsername"];
            string smtpPassword = _configuration["EmailSettings:SmtpPassword"];

            using (SmtpClient smtpClient = new SmtpClient(smtpServer))
            {
                smtpClient.Port = smtpPort;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true; // Enable SSL for secure email sending

                MailMessage mailMessage = new MailMessage();
                mailMessage.From=new MailAddress(fromEmail);;
                mailMessage.To.Add(new MailAddress("info@scholarwriteai.com"));
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                // Add a "Reply-To" address
                mailMessage.ReplyToList.Add(new MailAddress(fromEmail));

                //smtpClient.Send(mailMessage);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
        
    }
    
    public async Task ContactSendGmailEmailAsync(string fromEmail, string subject, string message)
    {
        try
        {
            string smtpServer = _configuration["GmailEmailSettings:SmtpServer"];
            int smtpPort = int.Parse(_configuration["GmailEmailSettings:SmtpPort"]);
            string smtpUsername = _configuration["GmailEmailSettings:SmtpUsername"];
            string smtpPassword = _configuration["GmailEmailSettings:SmtpPassword"];
            string appPassword = _configuration["GmailEmailSettings:AppPassword"]; // Use the generated App Password


            using (SmtpClient smtpClient = new SmtpClient(smtpServer))
            {
                smtpClient.Port = smtpPort;
                smtpClient.Credentials = new NetworkCredential(smtpUsername, appPassword);
                smtpClient.EnableSsl = true; // Enable SSL for secure email sending

                MailMessage mailMessage = new MailMessage();
                mailMessage.From=new MailAddress(fromEmail);;
                mailMessage.To.Add(new MailAddress("info@scholarwriteai.com"));
                mailMessage.Subject = subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                // Add a "Reply-To" address
                mailMessage.ReplyToList.Add(new MailAddress(fromEmail));

                //smtpClient.Send(mailMessage);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            
        }
        
    }


}