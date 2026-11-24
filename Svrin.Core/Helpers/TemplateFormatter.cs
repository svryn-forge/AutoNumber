using System.Text;
using System.Text.RegularExpressions;

namespace Svrin.Core.Helpers
{
    public static class TemplateFormatter
    {
        private static readonly Regex PlaceholderRegex = new Regex(@"\{([^{}:]+)(?::([^{}]+))?\}", RegexOptions.Compiled);

        public static string? ValidateValue(
            string template,
            string value,
            string prefix,
            string level)
        {
            var pattern = new StringBuilder();
            var position = 0;

            foreach (Match match in PlaceholderRegex.Matches(template))
            {
                pattern.Append(Regex.Escape(
                    template.Substring(position, match.Index - position)));

                var name = match.Groups[1].Value;
                var format = match.Groups[2].Success
                    ? match.Groups[2].Value
                    : null;

                switch (name)
                {
                    case "Prefix":
                        pattern.Append(Regex.Escape(prefix));
                        break;

                    case "Уровень":
                    case "Level":
                        pattern.Append(Regex.Escape(level));
                        break;

                    case "N":
                    case "Number":
                        if (format != null && Regex.IsMatch(format, @"^0+$"))
                        {
                            pattern.Append(@"[0-9]{");
                            pattern.Append(format.Length);
                            pattern.Append('}');
                        }
                        else
                        {
                            pattern.Append(@"[1-9][0-9]*");
                        }
                        break;

                    default:
                        pattern.Append(Regex.Escape(match.Value));
                        break;
                }

                position = match.Index + match.Length;
            }

            pattern.Append(Regex.Escape(
                template.Substring(position)));

            return Regex.IsMatch(
                value ?? string.Empty,
                "^" + pattern + "$",
                RegexOptions.CultureInvariant)
                ? null
                : "Значение не соответствует шаблону.";
        }
    }
}
