using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ray.Utils.Text
{
    public static class FrontSlug
    {
        private const string Accents =
            "ÀÁÂÃÄÅĄàáâãäåąÒÓÔÕÕÖØòóôõöøÈÉÊËĘèéêëðęÇĆçćÐÌÍÎÏìíîïÙÚÛÜùúûüÑñŠŚšśŸÿýŽŹŻžźżŁłŃńàáãảạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệđùúủũụưừứửữựòóỏõọôồốổỗộơờớởỡợìíỉĩịäëïîöüûñçýỳỹỵỷ";

        private const string Fixes =
            "AAAAAAAaaaaaaaOOOOOOOooooooEEEEEeeeeeeCCccDIIIIiiiiUUUUuuuuNnSSssYyyZZZzzzLlNnaaaaaaaaaaaaaaaaaeeeeeeeeeeeduuuuuuuuuuuoooooooooooooooooiiiiiaeiiouuncyyyyy";

        private static readonly Dictionary<char, string> AccentMap = BuildAccentMap();
        private static readonly Regex NonSlugChars = new Regex("[^a-z0-9-]+", RegexOptions.Compiled);
        private static readonly Regex RepeatedDelimiter = new Regex("-+", RegexOptions.Compiled);

        public static string Slugify(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var harmonized = StripAccents(input).Trim().ToLowerInvariant();
            harmonized = NonSlugChars.Replace(harmonized, "-");
            harmonized = RepeatedDelimiter.Replace(harmonized, "-");

            if (harmonized.StartsWith("-"))
                harmonized = harmonized.Substring(1);
            if (harmonized.EndsWith("-"))
                harmonized = harmonized.Substring(0, harmonized.Length - 1);

            return harmonized;
        }

        public static string SectionSlug(IList<string> nodesEn)
        {
            if (nodesEn == null || nodesEn.Count == 0)
                return "seccion";

            var slug = Slugify(nodesEn[nodesEn.Count - 1]);
            return string.IsNullOrEmpty(slug) ? "seccion" : slug;
        }

        private static Dictionary<char, string> BuildAccentMap()
        {
            var map = new Dictionary<char, string>();

            for (var i = 0; i < Accents.Length; i++)
            {
                var c = Accents[i];
                if (!map.ContainsKey(c))
                    map[c] = i < Fixes.Length ? Fixes[i].ToString() : string.Empty;
            }

            return map;
        }

        private static string StripAccents(string input)
        {
            var sb = new StringBuilder(input.Length);

            foreach (var c in input)
                sb.Append(AccentMap.TryGetValue(c, out var replacement) ? replacement : c);

            return sb.ToString();
        }
    }
}
