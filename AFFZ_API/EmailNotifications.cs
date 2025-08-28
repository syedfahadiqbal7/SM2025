using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AFFZ_API
{
    public class EmailNotifications : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailNotifications(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }


        //public async Task<bool> SendEmail(string to, string subject, string body, string toname, bool isHtml = true)
        //{
        //    try
        //    {
        //        //MimeMessage - a class from Mimekit
        //        MimeMessage email_Message = new MimeMessage();
        //        MailboxAddress email_From = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);
        //        email_Message.From.Add(email_From);
        //        MailboxAddress email_To = new MailboxAddress(toname, to);
        //        email_Message.To.Add(email_To);
        //        email_Message.Subject = subject;
        //        BodyBuilder emailBodyBuilder = new BodyBuilder();
        //        emailBodyBuilder.TextBody = body;
        //        email_Message.Body = emailBodyBuilder.ToMessageBody();
        //        //this is the SmtpClient class from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
        //        SmtpClient MailClient = new SmtpClient();
        //        MailClient.Connect(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
        //        MailClient.Authenticate(_smtpSettings.SenderEmail, _smtpSettings.Password);
        //        MailClient.Send(email_Message);
        //        MailClient.Disconnect(true);
        //        MailClient.Dispose();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Exception Details
        //        return false;
        //    }
        //}

        public async Task<bool> SendEmail(string to, string subject, string body, string toName, bool isHtml = true)
        {
            try
            {
                // Create a new MimeMessage
                MimeMessage emailMessage = new MimeMessage();

                // Set the sender and recipient
                emailMessage.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                emailMessage.To.Add(new MailboxAddress(toName, to));
                emailMessage.Subject = subject;

                // Set the body
                BodyBuilder emailBodyBuilder = new BodyBuilder();
                if (isHtml)
                {
                    emailBodyBuilder.HtmlBody = body; // Use HtmlBody for HTML content
                }
                else
                {
                    emailBodyBuilder.TextBody = body; // Use TextBody for plain text content
                }
                emailMessage.Body = emailBodyBuilder.ToMessageBody();

                // Send the email
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(_smtpSettings.Server, _smtpSettings.Port, SecureSocketOptions.StartTls);
                    smtpClient.Authenticate(_smtpSettings.SenderEmail, _smtpSettings.Password);
                    await smtpClient.SendAsync(emailMessage);
                    smtpClient.Disconnect(true);
                }

                return true; // Email sent successfully
            }
            catch (Exception ex)
            {
                // Log the exception (optional)
                Console.WriteLine($"Email sending failed: {ex.Message}");
                return false; // Indicate failure
            }
        }


    }

}
