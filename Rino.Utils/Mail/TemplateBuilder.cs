using System.Collections.Generic;
using System.IO;

namespace Rino.Utils.Mail
{
    public class TemplateBuilder
    {
        public string TemplateLocation { get; set; }

        private Dictionary<EmailTemplateType, string> templates = new Dictionary<EmailTemplateType, string>();

        public TemplateBuilder(string templateLocation)
        {
            TemplateLocation = templateLocation;

            templates.Add(EmailTemplateType.ConfirmUserRequest, "ConfirmUserRequestEmailTemplate.html");
            templates.Add(EmailTemplateType.CreateUserRequest, "CreateUserRequestEmailTemplate.html");
            templates.Add(EmailTemplateType.ForgotPasswordConfirmation, "ForgotPasswordConfirmationEmailTemplate.html");
            templates.Add(EmailTemplateType.ResetPassword, "ResetPasswordEmailTemplate.html");
            templates.Add(EmailTemplateType.CreateUser, "CreateUserEmailTemplate.html");
            templates.Add(EmailTemplateType.ResendInvitation, "ResendInvitationEmailTemplateFE.html");
        }

        public string GetTemplate(EmailTemplateType templateType)
        {
            var templateFile = templates[templateType];
            var body = string.Empty;

            var templateFullname = Path.Combine(TemplateLocation, templateFile);

            using (var reader = new StreamReader(templateFullname))
            {
                body = reader.ReadToEnd();
            }

            return body;
        }
    }
}
