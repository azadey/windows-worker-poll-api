using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal interface IEmailService
    {
        Task<bool> SendEmailWithAttachmentAsync(
            string toEmailAddress,
            string subject,
            string messageBody,
            List<string> attachments = null
        );
    }
}
