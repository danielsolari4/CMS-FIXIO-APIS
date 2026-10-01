using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rino.Dtos.Configuration;

namespace Rino.Utils.Mail
{
    public class ResendMailSender : IMailSender
    {
        private const string EmailsEndpoint = "https://api.resend.com/emails";

        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

        private readonly SMTP _smtpSettings;
        private readonly Resend _resendSettings;
        private readonly ILogger<ResendMailSender> _logger;

        public ResendMailSender(AppSettings appSettings, ILogger<ResendMailSender> logger)
        {
            _smtpSettings = appSettings.Smtp;
            _resendSettings = appSettings.Resend;
            _logger = logger;
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
                        var mappedBody = GetTemplatedBody(templateType, parameters);
                        var sent = await SendViaApi(templateType, to, subject, mappedBody);
                        return sent;
                    }
                }

                return false;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "[resend-mail] No se pudo enviar el template {TemplateType} con asunto {Subject}.", templateType, subject);
                return false;
            }
        }

        private async Task<bool> SendViaApi(EmailTemplateType templateType, string[] to, string subject, string mappedBody)
        {
            if (_resendSettings == null || string.IsNullOrWhiteSpace(_resendSettings.ApiKey))
            {
                _logger.LogError("[resend-mail] Falta appSettings:resend:apiKey. No se puede enviar el template {TemplateType}.", templateType);
                return false;
            }

            var fromAddress = string.IsNullOrWhiteSpace(_resendSettings.FromAddress)
                ? _smtpSettings.FromAddress
                : _resendSettings.FromAddress;
            var fromDisplayName = string.IsNullOrWhiteSpace(_resendSettings.FromDisplayName)
                ? _smtpSettings.FromDisplayName
                : _resendSettings.FromDisplayName;

            var recipients = to.Select(t => t.Trim()).ToArray();

            // Resend exige "to" no vacio. El primer destinatario va en To y el resto en Bcc,
            // que es lo mas cercano al comportamiento previo donde todo iba por Bcc.
            var toField = new JArray();
            toField.Add(recipients[0]);

            var payload = new JObject
            {
                ["from"] = $"{fromDisplayName} <{fromAddress}>",
                ["to"] = toField,
                ["subject"] = subject,
                ["html"] = mappedBody
            };

            if (recipients.Length > 1)
            {
                var bccField = new JArray();
                for (var i = 1; i < recipients.Length; i++)
                    bccField.Add(recipients[i]);

                payload["bcc"] = bccField;
            }

            using (var request = new HttpRequestMessage(HttpMethod.Post, EmailsEndpoint))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _resendSettings.ApiKey);
                request.Content = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");

                using (var response = await Client.SendAsync(request))
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("[resend-mail] Enviado el template {TemplateType} a {Recipients}. Respuesta: {Response}",
                            templateType, string.Join(",", recipients), content);
                        return true;
                    }

                    _logger.LogError("[resend-mail] Resend respondio {StatusCode} al enviar el template {TemplateType} a {Recipients}. Respuesta: {Response}",
                        (int)response.StatusCode, templateType, string.Join(",", recipients), content);
                    return false;
                }
            }
        }

        private string GetTemplatedBody(EmailTemplateType templateType, Dictionary<string, string> parameters)
        {
            var templateBuilder = new TemplateBuilder(ResolveTemplateLocation());
            var templatedBody = templateBuilder.GetTemplate(templateType);
            var mappedBody = MailValueMapper.Map(templatedBody, parameters);

            return mappedBody;
        }

        private string ResolveTemplateLocation()
        {
            var templateLocation = _smtpSettings.EmailTemplateLocation;

            if (string.IsNullOrWhiteSpace(templateLocation))
            {
                templateLocation = Path.Combine(AppContext.BaseDirectory, "Mail", "Templates");
                _logger.LogWarning("[resend-mail] smtp:emailTemplateLocation vacio, se usa la ruta por defecto {Path}.", templateLocation);
                return templateLocation;
            }

            if (!templateLocation.Trim().EndsWith(Path.DirectorySeparatorChar) && !templateLocation.Trim().EndsWith('/'))
            {
                templateLocation = templateLocation.Trim() + Path.DirectorySeparatorChar;
            }

            if (Directory.Exists(templateLocation))
                return templateLocation;

            // Si la ruta configurada no existe, se busca la misma marca dentro de los templates
            // publicados junto al binario, que es donde quedan tras el build.
            var brand = templateLocation.TrimEnd(Path.DirectorySeparatorChar, '/');
            brand = brand.Substring(brand.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            var fallback = Path.Combine(AppContext.BaseDirectory, "Mail", "Templates", brand) + Path.DirectorySeparatorChar;

            _logger.LogWarning("[resend-mail] La ruta {Path} no existe, se usa el fallback {Fallback}.", templateLocation, fallback);

            return fallback;
        }
    }
}
