using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class SmtpEmailService : IEmailService
    {
        private readonly string _smtpServer;

        private readonly int _smtpPort;

        private readonly string _smtpUsername;

        private readonly string _smtpPassword;

        private readonly string _fromEmailAddress;

        public SmtpEmailService(IConfiguration configuration)
        {
            _smtpServer = configuration.GetValue<string>("SmtpServer");
            _smtpPort = configuration.GetValue<int>("SmtpPort");
            _smtpUsername = configuration.GetValue<string>("SmtpUsername");
            _smtpPassword = configuration.GetValue<string>("SmtpPassword");
            _fromEmailAddress = configuration.GetValue<string>("FromEmailAddress");
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
