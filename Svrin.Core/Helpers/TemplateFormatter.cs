using System.Text;
using System.Text.RegularExpressions;

namespace Svrin.Core.Helpers
{
    public static class TemplateFormatter
    {
        private static readonly Regex PlaceholderRegex = new Regex(@"\{([^{}:]+)(?::([^{}]+))?\}", RegexOptions.Compiled);

        public static string? ValidateValue(string template, string value)
        {
            var pattern = new StringBuilder();
            var position = 0;

            foreach (Match match in PlaceholderRegex.Matches(template))
            {
                pattern.Append(Regex.Escape(
                    template.Substring(position, match.Index - position)));

                var name = match.Groups[1].Value;                        // "N" / "Number" / ...
                var format = match.Groups[2].Success                       // "0", "00" или null
                    ? match.Groups[2].Value
                    : null;

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
                        if (format != null && Regex.IsMatch(format, @"^0+$"))
                            pattern.Append(@"\d{" + format.Length + "}");
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
