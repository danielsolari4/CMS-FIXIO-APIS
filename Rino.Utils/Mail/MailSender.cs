using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Rino.Dtos.Configuration;
using Rino.Utils.Configuration;

namespace Rino.Utils.Mail
{
    public interface IMailSender
    {
        Task<bool> Send(EmailTemplateType templateType, string to, string subject, Dictionary<string, string> parameters, MailPriority priority);
        Task<bool> Send(EmailTemplateType templateType, string[] to, string subject, Dictionary<string, string> parameters, MailPriority priority);
    }


    public class MailSender : IMailSender
    {
        private readonly SMTP _smtpSettings;
        private SmtpClient _smtpClient;
        public MailSender(AppSettings appSettings)
        {
            _smtpSettings = appSettings.Smtp;

            var smtp = new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = _smtpSettings.EnableSsl,
                UseDefaultCredentials = _smtpSettings.UseDefaultCredentials
            };

            if (!smtp.UseDefaultCredentials)
            {
                var username = _smtpSettings.UserName;
                var password = _smtpSettings.Password;
                smtp.Credentials = new NetworkCredential(username, password);
            }

            _smtpClient = smtp;
        }


        public async Task<bool> Send(EmailTemplateType templateType, string to, string subject, Dictionary<string, string> parameters, MailPriority priority)
        {
            if (!string.IsNullOrEmpty(to))
            {
                var result = await Send(templateType, to.Split(','), subject, parameters, priority);
                return result;
            }

            return false;
        }

        public async Task<bool> Send(EmailTemplateType templateType, string[] to, string subject, Dictionary<string, string> parameters, MailPriority priority)
        {
            try
            {
                if (_smtpSettings.EnableEmailNotifications)
                {
                    if (string.IsNullOrWhiteSpace(_smtpSettings.EnableEmailAddresses))
                        to = to.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
                    else
                        to = to.Where(t => !string.IsNullOrWhiteSpace(t) && _smtpSettings.EnableEmailAddresses.Contains(t)).ToArray();

                    if (to.Length > 0)
                    {
                        var email = GetMailMessage(templateType, to, subject, parameters, priority);
                        await _smtpClient.SendMailAsync(email);
                        return true;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private MailMessage GetMailMessage(EmailTemplateType templateType, string[] to, string subject, Dictionary<string, string> parameters, MailPriority priority)
        {
            var email = new MailMessage();
            var fromAddress = _smtpSettings.FromAddress;
            var fromDisplayName = _smtpSettings.FromDisplayName;

            foreach (var item in to)
            {
                if (string.IsNullOrWhiteSpace(item.Trim())) continue;
                email.Bcc.Add(new MailAddress(item));
            }

            email.From = new MailAddress(fromAddress, fromDisplayName);
            email.Subject = subject;
            var templateBody = GetTemplatedBody(templateType, parameters);
            email.AlternateViews.Add(templateBody);
            email.IsBodyHtml = true;
            email.Priority = priority;

            return email;
        }

        private AlternateView GetTemplatedBody(EmailTemplateType templateType, Dictionary<string, string> parameters)
        {
            //"C:\\Repos\\Rino\\Apis\\Trunk\\CMS.Utils\\Mail\\Templates"
            var templateLocation = _smtpSettings.EmailTemplateLocation;

            if (!templateLocation.Trim().EndsWith(System.IO.Path.DirectorySeparatorChar) && !templateLocation.Trim().EndsWith('/'))
            {
                templateLocation = templateLocation.Trim() + System.IO.Path.DirectorySeparatorChar;
            }

            var templateBuilder = new TemplateBuilder(templateLocation);
            var templatedBody = templateBuilder.GetTemplate(templateType);
            var mappedBody = MailValueMapper.Map(templatedBody, parameters);
            var alternateView = AlternateView.CreateAlternateViewFromString(mappedBody, null, MediaTypeNames.Text.Html);

            return alternateView;
        }
    }
}
