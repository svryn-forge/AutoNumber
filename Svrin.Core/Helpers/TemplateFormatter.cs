using System.Text;
using System.Text.RegularExpressions;

namespace Svrin.Core.Helpers
{
    internal static class TemplateFormatter
    {
        private static readonly Regex PlaceholderRegex = new Regex(@"\{([^{}]+)\}", RegexOptions.Compiled);

        public static string? ValidateValue(string template, string value)
        {
            var pattern = new StringBuilder();
            var position = 0;

            foreach (Match match in PlaceholderRegex.Matches(template))
            {
                pattern.Append(Regex.Escape(
                    template.Substring(position, match.Index - position)));

                var parts = match.Groups[1].Value.Split(':');
                var name = parts[0];

                switch (name)
                {
                    case "Prefix":
                        pattern.Append(@"\p{L}");
                        break;

                    case "Уровень":
                    case "Level":
                        pattern.Append(".+?");
                        break;

                    case "N":
                    case "Number":
                        if (parts.Length == 2 && Regex.IsMatch(parts[1], @"^0+$"))
                            pattern.Append(@"\d{" + parts[1].Length + "}");
                        else
                            pattern.Append(@"\d+");
                        break;

                    default:
                        pattern.Append(Regex.Escape(match.Value));
                        break;
                }

                position = match.Index + match.Length;
            }

            pattern.Append(Regex.Escape(
                template.Substring(position)));

            return Regex.IsMatch(value ?? string.Empty, "^" + pattern + "$")
                ? null
                : "Значение не соответствует шаблону.";
        }
    }
}
