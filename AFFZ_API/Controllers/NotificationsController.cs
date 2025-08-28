using AFFZ_API.Interfaces;
using AFFZ_API.Models;
using AFFZ_API.NotificationsHubs;
using AFFZ_API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AFFZ_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly ILogger<NotificationsController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private string BaseUrl = string.Empty;
        private string PublicDomain = string.Empty;
        private string ApiHttpsPort = string.Empty;
        private string MerchantHttpsPort = string.Empty;
        private string CustomerHttpsPort = string.Empty;
        private readonly IEmailService _emailService;

        public NotificationsController(MyDbContext context, ILogger<NotificationsController> logger, IHubContext<NotificationHub> hubContext, IEmailService emailService, IAppSettingsService service)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _emailService = emailService;
            BaseUrl = service.GetBaseIpAddress();
            PublicDomain = service.GetPublicDomain();
            ApiHttpsPort = service.GetApiHttpsPort();
            MerchantHttpsPort = service.GetMerchantHttpsPort();
            CustomerHttpsPort = service.GetCustomerHttpsPort();
        }

        [HttpPost("CreateNotification")]
        public async Task<IActionResult> CreateNotification(Notification notification, string StatusId)
        {
            try
            {
                notification.CreatedDate = DateTime.UtcNow.ToLocalTime();
                notification.ReadDate = new DateTime(1900, 1, 1);

                notification.IsRead = false;
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
                // Notify connected clients
                string userId = string.Empty;
                userId = notification.SenderType == "Customer" ? notification.MerchantId : notification.UserId;
                List<NotificationDTO> notify = await GetMerchantNotification(userId, notification.SenderType);
                // Send notifications based on sender type to only the targeted clients
                if (notification.SenderType == "Customer")
                {
                    await _hubContext.Clients.Group("AFFZ_Provider").SendAsync("ReceiveNotification", notify);
                    await SendNotificationEmailToMerchant(notification);
                }
                else if (notification.SenderType == "Merchant")
                {
                    await _hubContext.Clients.Group("AFFZ_Customer").SendAsync("ReceiveNotification", notify);
                    await SendNotificationEmailToCustomer(notification);
                }
                //await SendNotificationEmail(notification, StatusId);
                // await _hubContext.Clients.All.SendAsync("ReceiveNotification", notify);

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }

        private async Task<string> NotificationEmailTemplate(string Emailmsg, string status)
        {

            if (Emailmsg.Contains("Paid Amount="))
            {
                string[] mailing = Emailmsg.Split('=');
                Emailmsg = $"{mailing[0]} <b>{mailing[1]}</b>";
            }
            string EmailTemplate = "<!DOCTYPE html>\n<html>\n<head>\n<style>\nbody { font-family: Arial, sans-serif; background-color: #f8f9fa; margin: 0; padding: 0; }\n.email-container { max-width: 600px; margin: 20px auto; background: #ffffff; border-radius: 10px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); padding: 20px; }\n.header { text-align: center; color: #343a40; margin-bottom: 20px; }\n.header h1 { font-size: 24px; }\n.content { color: #555555; line-height: 1.6; }\n.footer { margin-top: 20px; text-align: center; font-size: 12px; color: #888888; }\n</style>\n</head>\n<body>\n<div class='email-container'>\n<div class='header'><h1>Service Status: InProgress</h1></div>\n<div class='content'>\n<p style='font-weight:bold;'>Hello <strong>{{Name}}</strong>,</p>\n<p>Status Description: <em>" + status + "</em></p>\n<p>" + Emailmsg + "</p>\n</div>\n<div class='footer'><p>&copy; {{CurrentYear}} SmartCenter. All Rights Reserved.</p></div>\n</div>\n</body>\n</html>";

            return EmailTemplate;
        }
        private async Task<bool> SendNotificationEmail(Notification notification, string StatusID)
        {
            try
            {
                string templatePath = string.Empty;
                string RedirectUrlLoc = string.Empty;
                if (notification.SenderType == "Customer")
                {
                    templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "CEmailTemplates.json");
                    RedirectUrlLoc = $"{Request.Scheme}://{PublicDomain}:{CustomerHttpsPort}/";
                }
                else
                {
                    templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "MEmailTemplates.json");
                    RedirectUrlLoc = $"{Request.Scheme}://{PublicDomain}:{MerchantHttpsPort}/";
                }
                var emailTemplateLoader = new EmailTemplateLoader(templatePath);

                EmailTemplate emailTemplate = new EmailTemplate();
                string userName = string.Empty;
                string EmailAddress = string.Empty;

                // Load Template Based on NotificationType
                if (!string.IsNullOrEmpty(notification.SenderType))
                {
                    //emailTemplate = emailTemplateLoader.GetStatusNotificationTemplate($"{StatusID}");
                    emailTemplate.Body = await NotificationEmailTemplate(notification.Message, emailTemplate.Subject);
                    int CID = Convert.ToInt32(notification.UserId);
                    int MID = Convert.ToInt32(notification.MerchantId);
                    userName = notification.SenderType != "Customer"
                        ? _context.Customers.FirstOrDefault(c => c.CustomerId == CID)?.CustomerName
                        : _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.ProviderName;
                    EmailAddress = notification.SenderType != "Customer"
                        ? _context.Customers.FirstOrDefault(c => c.CustomerId == CID)?.Email
                        : _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.Email;
                }
                else
                {

                    //add log no Template found
                }

                // Replace Placeholders
                string emailBody = emailTemplate.Body
                    .Replace("{{Name}}", userName ?? "Application User")
                    .Replace("{{ResetLink}}", RedirectUrlLoc)
                    .Replace("{{CurrentYear}}", DateTime.UtcNow.Year.ToString());

                // Simulate Email Sending
                _logger.LogInformation("Sending Email to: {Email}, Subject: {Subject}", notification.MessageFromId, emailTemplate.Subject);
                _logger.LogInformation("Email Body: {Body}", emailBody);

                // Use your IEmailService here to send the email
                // Example:
                bool emailSent = await _emailService.SendEmail(EmailAddress, emailTemplate.Subject, emailBody, userName, isHtml: true);

                // Simulated Success
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email for notification.");
                return false;
            }
        }
        private async Task<bool> SendNotificationEmailToCustomer(Notification notification)
        {
            try
            {
                string templatePath = string.Empty;
                string RedirectUrlLoc = string.Empty;
                EmailTemplate emailTemplate = new EmailTemplate();
                string userName = string.Empty;
                string EmailAddress = string.Empty;
                string serviceName = string.Empty;
                // Load Template Based on NotificationType
                if (!string.IsNullOrEmpty(notification.SenderType))
                {
                    emailTemplate.Subject = (!notification.Message.Contains("Success") ? notification.Message.Contains("Fail") ? "Service Status - Failed Providing Service." : "Service Status - In Progress." : "Service Status - Completed Successfully.");
                    //emailTemplate.Body = await NotificationEmailTemplate(notification.Message, emailTemplate.Subject);
                    int CID = Convert.ToInt32(notification.UserId);
                    int MID = Convert.ToInt32(notification.MerchantId);
                    string SenderName = _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.ProviderName;
                    userName = _context.Customers.FirstOrDefault(c => c.CustomerId == CID)?.CustomerName;
                    EmailAddress = _context.Customers.FirstOrDefault(c => c.CustomerId == CID)?.Email;
                    emailTemplate.Body = await NotificationEmailTemplate(ReplaceAllPlaceholders(notification.Message, SenderName, SenderName, null, ref serviceName), emailTemplate.Subject);
                    serviceName = !string.IsNullOrEmpty(serviceName) ? "For Service " + serviceName + "" : "";
                    emailTemplate.Subject = $"Service Status: {SenderName} {serviceName}";
                }
                else
                {

                    //add log no Template found
                }

                // Replace Placeholders
                string emailBody = emailTemplate.Body
                    .Replace("{{Name}}", userName ?? "Application User")
                    .Replace("{{ResetLink}}", RedirectUrlLoc)
                    .Replace("{{CurrentYear}}", DateTime.UtcNow.Year.ToString());

                // Simulate Email Sending
                _logger.LogInformation("Sending Email to: {Email}, Subject: {Subject}", EmailAddress, emailTemplate.Subject);
                _logger.LogInformation("Email Body: {Body}", emailBody);

                // Use your IEmailService here to send the email
                // Example:
                bool emailSent = await _emailService.SendEmail(EmailAddress, emailTemplate.Subject, emailBody, userName, isHtml: true);

                // Simulated Success
                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email for notification.");
                return false;
            }
        }
        private async Task<bool> SendNotificationEmailToMerchant(Notification notification)
        {
            try
            {
                string RedirectUrlLoc = string.Empty;


                EmailTemplate emailTemplate = new EmailTemplate();
                string userName = string.Empty;
                string EmailAddress = string.Empty;
                string serviceName = string.Empty;
                // Load Template Based on NotificationType
                if (!string.IsNullOrEmpty(notification.SenderType))
                {
                    //emailTemplate.Subject = (!notification.Message.Contains("Success") ? notification.Message.Contains("Fail") ? "Service Status - Failed Providing Service." : "Service Status - In Progress." : "Service Status - Completed Successfully.");

                    //emailTemplate = emailTemplateLoader.GetStatusNotificationTemplate($"{StatusID}");

                    int CID = Convert.ToInt32(notification.UserId);
                    int MID = Convert.ToInt32(notification.MerchantId);
                    userName = _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.ProviderName;
                    string SenderName = _context.Customers.FirstOrDefault(c => c.CustomerId == CID)?.CustomerName;
                    EmailAddress = _context.ProviderUsers.FirstOrDefault(p => p.ProviderId == MID)?.Email;
                    emailTemplate.Body = await NotificationEmailTemplate(ReplaceAllPlaceholders(notification.Message, SenderName, null, null, ref serviceName), emailTemplate.Subject);
                    serviceName = !string.IsNullOrEmpty(serviceName) ? "For Service " + serviceName + "" : "";
                    emailTemplate.Subject = $"Service Status: {SenderName} {serviceName}";
                }
                else
                {

                    //add log no Template found
                }

                // Replace Placeholders
                string emailBody = emailTemplate.Body
                    .Replace("{{Name}}", userName ?? "Application Merchant")
                    .Replace("{{ResetLink}}", RedirectUrlLoc)
                    .Replace("{{CurrentYear}}", DateTime.UtcNow.Year.ToString());

                // Simulate Email Sending
                _logger.LogInformation("Sending Email to: {Email}, Subject: {Subject}", EmailAddress, emailTemplate.Subject);
                _logger.LogInformation("Email Body: {Body}", emailBody);

                // Use your IEmailService here to send the email
                // Example:
                bool emailSent = await _emailService.SendEmail(EmailAddress, emailTemplate.Subject, emailBody, userName, isHtml: true);

                // Simulated Success
                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email for notification.");
                return false;
            }
        }
        [HttpGet("GetUserNotifications/{userId}")]
        public async Task<IActionResult> GetUserNotifications(string userId)
        {
            try
            {
                string servicename = string.Empty;
                int id = Convert.ToInt32(userId);
                string q = _context.Notifications
                                              .Where(n => n.MessageFromId != id && n.UserId == userId)
                                              .OrderByDescending(n => n.Id).ToQueryString();
                var notifications = await _context.Notifications
                        .Where(n => n.UserId == userId && n.SenderType == "Merchant")
                        .OrderByDescending(n => n.Id)
                        .Join(
                            _context.Merchants,
                            notification => notification.UserId,
                            merchant => merchant.MerchantId.ToString(),
                            (notification, merchant) => new NotificationDTO
                            {
                                Id = notification.Id,
                                UserId = notification.UserId,
                                SenderType = notification.SenderType,
                                MerchantId = notification.MerchantId,
                                MessageFromId = notification.MessageFromId,
                                Message = notification.Message,
                                CreatedDate = notification.CreatedDate,
                                ReadDate = notification.ReadDate,
                                SenderName = merchant.CompanyName,
                                RedirectToActionUrl = notification.RedirectToActionUrl,
                                NotificationType = notification.NotificationType,
                                IsRead = notification.IsRead
                            }
                        ).ToListAsync();
                // Apply placeholder replacements after data retrieval
                foreach (var notification in notifications)
                {
                    notification.Message = ReplaceAllPlaceholders(notification.Message, null, notification.SenderName, null, ref servicename);
                }

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the file list.");
                return StatusCode(500, "Internal server error.");
            }

        }

        [HttpGet("GetMerchantNotifications/{merchantId}")]
        public async Task<IActionResult> GetMerchantNotifications(string merchantId)
        {
            string servicename = string.Empty;
            int id = Convert.ToInt32(merchantId);
            //var notifications = await _context.Notifications
            //            .Where(n => n.MerchantId == merchantId && (n.SenderType == "Customer" || n.SenderType == "Admin"))
            //            .OrderByDescending(n => n.Id)
            //            .Join(
            //                _context.Customers,
            //                notification => notification.UserId,
            //                customer => customer.CustomerId.ToString(),
            //                (notification, customer) => new NotificationDTO
            //                {
            //                    Id = notification.Id,
            //                    UserId = notification.UserId,
            //                    SenderType = notification.SenderType,
            //                    MerchantId = notification.MerchantId,
            //                    MessageFromId = notification.MessageFromId,
            //                    Message = notification.Message,
            //                    CreatedDate = notification.CreatedDate,
            //                    ReadDate = notification.ReadDate,
            //                    SenderName = customer.CustomerName,
            //                    RedirectToActionUrl = notification.RedirectToActionUrl,
            //                    NotificationType = notification.NotificationType,
            //                    IsRead = notification.IsRead
            //                }
            //            ).ToListAsync();
            //Left Join (GroupJoin + SelectMany): This ensures that notifications with UserId = 0 are always included, even if there’s no corresponding entry in the Customers table.
            //SenderName: If UserId is 0, it sets SenderName to "Admin"; otherwise, it takes the name from the Customers table.
            //DefaultIfEmpty(): Ensures the query does not exclude notifications without a matching customer.
            // Fetch Customer-based notifications
            var customerNotifications = await _context.Notifications
                        .Where(n => n.MerchantId == merchantId && n.SenderType == "Customer")
                        .OrderByDescending(n => n.Id)
                        .Join(
                            _context.Customers,
                            notification => notification.UserId,
                            customer => customer.CustomerId.ToString(),
                            (notification, customer) => new NotificationDTO
                            {
                                Id = notification.Id,
                                UserId = notification.UserId,
                                SenderType = notification.SenderType,
                                MerchantId = notification.MerchantId,
                                MessageFromId = notification.MessageFromId,
                                Message = notification.Message,
                                CreatedDate = notification.CreatedDate,
                                ReadDate = notification.ReadDate,
                                SenderName = customer.CustomerName,
                                RedirectToActionUrl = notification.RedirectToActionUrl,
                                NotificationType = notification.NotificationType,
                                IsRead = notification.IsRead,
                                ImageUrl = customer.ProfilePicture
                            }
                        ).ToListAsync();

            // Fetch Admin-based notifications
            var adminNotifications = await _context.Notifications
                .Where(n => n.MerchantId == merchantId && n.SenderType == "Admin")
                .Join(
                    _context.AdminUsers,
                    notification => notification.UserId,
                    admin => admin.UserId.ToString(),
                    (notification, admin) => new NotificationDTO
                    {
                        Id = notification.Id,
                        UserId = notification.UserId,
                        SenderType = notification.SenderType,
                        MerchantId = notification.MerchantId,
                        MessageFromId = notification.MessageFromId,
                        Message = notification.Message,
                        CreatedDate = notification.CreatedDate,
                        ReadDate = notification.ReadDate,
                        SenderName = admin.UserName,
                        RedirectToActionUrl = notification.RedirectToActionUrl,
                        NotificationType = notification.NotificationType,
                        IsRead = notification.IsRead
                    }
                )
                .ToListAsync();

            // Combine both lists
            var notifications = customerNotifications.Union(adminNotifications)
                .OrderByDescending(n => n.Id)
                .ToList();
            // Apply placeholder replacements after data retrieval
            foreach (var notification in notifications)
            {
                notification.Message = ReplaceAllPlaceholders(notification.Message, notification.SenderName, null, null, ref servicename);
            }

            return Ok(notifications);
        }

        [HttpPost("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            var notification = await _context.Notifications.FindAsync(Convert.ToInt32(id));
            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead(string UserOrMerchantid, bool isUser)
        {
            List<Notification> notifications = new List<Notification>();
            if (isUser)
            {
                notifications = await _context.Notifications.Where(x => x.UserId == UserOrMerchantid && x.SenderType == "Merchant" && x.IsRead == false).ToListAsync();
            }
            else
            {
                notifications = await _context.Notifications.Where(x => x.MerchantId == UserOrMerchantid && x.SenderType == "Customer" && x.IsRead == false).ToListAsync();
            }
            if (notifications == null)
            {
                return NotFound();
            }
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        private async Task<List<NotificationDTO>> GetMerchantNotification(string userId, string Sendertype)
        {
            try
            {
                string servicename = string.Empty;
                int id = Convert.ToInt32(userId);
                List<NotificationDTO> notifications = new List<NotificationDTO>();

                if (Sendertype == "Merchant")
                {
                    notifications = await _context.Notifications
                       .Where(n => n.UserId == userId && n.SenderType == "Merchant")
                       .OrderByDescending(n => n.Id)
                       .Join(
                           _context.Customers,
                           notification => notification.UserId,
                           customer => customer.CustomerId.ToString(),
                           (notification, customer) => new NotificationDTO
                           {
                               Id = notification.Id,
                               UserId = notification.UserId,
                               SenderType = notification.SenderType,
                               MerchantId = notification.MerchantId,
                               MessageFromId = notification.MessageFromId,
                               Message = notification.Message,
                               CreatedDate = notification.CreatedDate,
                               ReadDate = notification.ReadDate,
                               SenderName = customer.CustomerName,
                               RedirectToActionUrl = notification.RedirectToActionUrl,
                               NotificationType = notification.NotificationType,
                               IsRead = notification.IsRead
                           }
                       ).ToListAsync();
                    // Apply placeholder replacements after data retrieval
                    foreach (var notification in notifications)
                    {
                        notification.Message = ReplaceAllPlaceholders(notification.Message, null, notification.SenderName, null, ref servicename);
                    }
                }
                else if (Sendertype == "Customer")
                {
                    notifications = await _context.Notifications
                        .Where(n => n.MerchantId == userId && n.SenderType == Sendertype)
                        .OrderByDescending(n => n.Id)
                        .Join(
                            _context.Customers,
                            notification => notification.UserId,
                            customer => customer.CustomerId.ToString(),
                            (notification, customer) => new NotificationDTO
                            {
                                Id = notification.Id,
                                UserId = notification.UserId,
                                SenderType = notification.SenderType,
                                MerchantId = notification.MerchantId,
                                MessageFromId = notification.MessageFromId,
                                Message = notification.Message,
                                CreatedDate = notification.CreatedDate,
                                ReadDate = notification.ReadDate,
                                SenderName = customer.CustomerName,
                                RedirectToActionUrl = notification.RedirectToActionUrl,
                                NotificationType = notification.NotificationType,
                                IsRead = notification.IsRead
                            }
                        ).ToListAsync();
                    // Apply placeholder replacements after data retrieval
                    foreach (var notification in notifications)
                    {
                        notification.Message = ReplaceAllPlaceholders(notification.Message, notification.SenderName, null, null, ref servicename);
                    }
                }

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting the file list.");
                return new List<NotificationDTO>();
            }
        }

        // Helper method to replace placeholders based on multiple possible prefixes
        private string ReplaceAllPlaceholders(string message, string userName, string merchantName, string serviceName, ref string sname)
        {
            // Replace any occurrences of "User[<ID>]" with the userName
            if (!string.IsNullOrEmpty(userName))
            {
                message = Regex.Replace(message, @"User\[\d+\]", $"User[{userName}]");
            }

            // Replace any occurrences of "Merchant[<ID>]" with the merchantName
            if (!string.IsNullOrEmpty(merchantName))
            {
                message = Regex.Replace(message, @"Merchant\[\d+\]", $"Merchant[{merchantName}]");
            }

            // Extract and replace "Service[<ID>]" with the corresponding ServiceName from the database
            var serviceMatch = Regex.Match(message, @"Service\[(\d+)\]");
            string _sName = string.Empty;
            if (serviceMatch.Success)
            {
                int serviceId = int.Parse(serviceMatch.Groups[1].Value);

                // Query the database to get the ServiceName for the extracted ServiceId
                //_sName = _context.Services.Where(s => s.ServiceId == serviceId).Select(s => s.SID).FirstOrDefault();
                _sName = _context.Services
    .Where(x => x.ServiceId == serviceId)
    .Join(_context.ServicesLists,
          service => service.SID,
          serviceList => serviceList.ServiceListID,
          (service, serviceList) => serviceList.ServiceName)
    .FirstOrDefault();
                if (!string.IsNullOrEmpty(_sName))
                {
                    message = Regex.Replace(message, @"Service\[\d+\]", $"Service[{_sName}]");
                    sname = _sName;
                }
            }

            // Extract and replace "Service[<ID>]" with the corresponding ServiceName from the database
            var documentMatch = Regex.Match(message, @"Document\[(\d+)\]");
            string _dName = string.Empty;
            if (documentMatch.Success)
            {
                int docId = int.Parse(documentMatch.Groups[1].Value);

                // Query the database to get the ServiceName for the extracted ServiceId
                _dName = _context.UploadedFiles.Where(s => s.Ufid == docId).Select(s => s.FileName).FirstOrDefault();

                if (!string.IsNullOrEmpty(_dName))
                {
                    message = Regex.Replace(message, @"Document\[\d+\]", $"Document[{_dName}]");
                }
            }

            return message;
        }
    }
}
