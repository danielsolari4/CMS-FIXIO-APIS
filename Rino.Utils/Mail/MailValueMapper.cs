using System.Collections.Generic;
using System.Text;

namespace Rino.Utils.Mail
{
    internal static class MailValueMapper
    {
        internal static string Map(string templatedBody, Dictionary<string, string> parameters)
        {
            var stringBuilder = new StringBuilder(templatedBody);

            foreach (var parameter in parameters.Keys)
            {
                stringBuilder.Replace(parameter, parameters[parameter]);
            }

            return stringBuilder.ToString();
        }
    }
}
