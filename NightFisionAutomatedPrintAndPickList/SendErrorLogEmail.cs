using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class SendErrorLogEmail
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly string _recipient;

        private TimeOnly _timer;
        private bool _disposed = false;

        public SendErrorLogEmail(ILogger<Worker> logger, IConfiguration configuration, IEmailService emailService)
        {
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;

            _recipient = _configuration.GetValue<string>("ErrorLogRecipient");
        }

        public async Task SendLogEmailAsync(CancellationToken stoppingToken, int emailServiceInterval)
        {
            try
            {
                var labelTask = "label";
                var pickNoteTask = "picknote";
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");


                var labelLogPath = Path.Combine(logPath, $"{labelTask}_exceptions_{DateTime.Now.AddSeconds(-1 * emailServiceInterval):yyyyMMdd}.log");
                var labelPathProcessing = Path.Combine(logPath, $"{labelTask}_exceptions_processing_{DateTime.Now:yyyyMMdd}.log");
                if (File.Exists(labelPathProcessing))
                {
                    _logger.LogWarning("[EMAIL_SERVICE] not able to mv label log file to new path", labelPathProcessing);
                }
                else
                {
                    File.Move(labelLogPath, labelPathProcessing);
                }

                
                var pickNoteLogPath = Path.Combine(logPath, $"{pickNoteTask}_exceptions_{DateTime.Now.AddSeconds(-1 * emailServiceInterval):yyyyMMdd}.log");
                var pickNotePathProcessing = Path.Combine(logPath, $"{pickNoteTask}_exceptions_processing_{DateTime.Now.AddSeconds(-1 * emailServiceInterval):yyyyMMdd}.log");
                if (File.Exists(pickNotePathProcessing))
                {
                    _logger.LogWarning("[EMAIL_SERVICE] not able to mv picknote log file to new path", pickNotePathProcessing);
                }
                else
                {
                    File.Move(pickNoteLogPath, pickNotePathProcessing);
                }

                // Check if log file exists
                if (!File.Exists(labelPathProcessing) && !File.Exists(pickNotePathProcessing))
                {
                    _logger.LogWarning("[EMAIL_SERVICE] Log file label {LogFilePath} not found", labelPathProcessing);
                    _logger.LogWarning("[EMAIL_SERVICE] Log file picknote {LogFilePath} not found", pickNotePathProcessing);
                    return;
                }

                // Read log file contents
                var labelContent = await File.ReadAllTextAsync(labelPathProcessing, stoppingToken);
                var pickNoteContent = await File.ReadAllTextAsync(pickNotePathProcessing, stoppingToken);

                if (string.IsNullOrEmpty(labelContent) && string.IsNullOrEmpty(pickNoteContent))
                {
                    // No need to send email if logs are empty
                    _logger.LogWarning("[EMAIL_SERVICE] Log file content not found ending worker");
                    return;
                }

                // Generate email subject and body
                var subject = $"Log file for {DateTime.Now.AddDays(-1 * emailServiceInterval):yyyy-MM-dd}";
                var body = $"Attached is the log file for {DateTime.Now.AddDays(-1 * emailServiceInterval):yyyy-MM-dd}.";
                List<string> attachments = new List<string>();
                if (!string.IsNullOrEmpty(labelContent))
                {
                    attachments.Add(labelPathProcessing);
                }
                if (!string.IsNullOrEmpty(pickNoteContent))
                {
                    attachments.Add(pickNotePathProcessing);
                }

                // Send email with log file as attachment
                var emailSent = await _emailService.SendEmailWithAttachmentAsync(
                    _recipient,
                    subject,
                    body,
                    attachments);

                if (emailSent)
                {
                    _logger.LogInformation("[EMAIL_SERVICE] Log email sent successfully to {LogEmailRecipient}.", _recipient);

                    // Delete log file
                    File.Delete(labelPathProcessing);
                    _logger.LogInformation("[EMAIL_SERVICE] Log file {LogFilePath} deleted successfully.", labelPathProcessing);

                    // Delete log file
                    File.Delete(pickNotePathProcessing);
                    _logger.LogInformation("[EMAIL_SERVICE] Log file {LogFilePath} deleted successfully.", pickNotePathProcessing);
                }
                else
                {
                    _logger.LogError("[EMAIL_SERVICE] Failed to send log email to {LogEmailRecipient}.", _recipient);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("[EMAIL_SERVICE] Exception received ::", ex);
            }
        }
    }
}
