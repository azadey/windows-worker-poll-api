using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using NightFisionAutomatedPrintAndPickList.Services.Interfaces;

namespace NightFisionAutomatedPrintAndPickList.Services
{
    internal class SmtpEmailService : IEmailService
    {
        private readonly ConfigManager _configuration;

        private readonly string _smtpServer;

        private readonly int _smtpPort;

        private readonly string _smtpUsername;

        private readonly string _smtpPassword;

        private readonly string _fromEmailAddress;

        public SmtpEmailService(ConfigManager configuration)
        {
            _configuration = configuration;
            _smtpServer = configuration.GetEmailSettings("Default", "Host");
            _smtpPort = int.Parse(configuration.GetEmailSettings("Default", "Port"));
            _smtpUsername = configuration.GetEmailSettings("Default", "Username");
            _smtpPassword = configuration.GetEmailSettings("Default", "Password"); ;
            _fromEmailAddress = configuration.GetEmailSettings("Default", "FromEmail"); ;
        }

        public async Task<bool> SendEmailWithAttachmentAsync(string toEmailAddress, string subject, string messageBody, List<string> attachments = null)
        {
            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_fromEmailAddress);
                    mailMessage.To.Add(toEmailAddress);
                    mailMessage.Subject = subject;
                    mailMessage.Body = messageBody;

                    if (attachments != null && attachments.Count > 0)
                    {
                        foreach (var filePath in attachments)
                        {
                            if (!File.Exists(filePath))
                            {
                                throw new FileNotFoundException($"[EMAIL_SERVICE] Attachment file not found: {filePath}");
                            }

                            var attachment = new Attachment(filePath);
                            mailMessage.Attachments.Add(attachment);
                        }
                    }



                    await smtpClient.SendMailAsync(mailMessage);

                    return true;
                }
            }
        }
    }
}
