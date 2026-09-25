using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Utils.Extensions
{
    public static class StringExtensions
    {
        public static string GetChunckedFileExtension(this string path)
        {
            var extensions = path.Split('.');
            return $".{extensions[extensions.Length - 2]}";
        }
    }
}
