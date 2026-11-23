using System.Text;
using System.Text.RegularExpressions;

namespace Svrin.Core.Helpers
{
    internal static class TemplateFormatter
    {
        private static readonly Regex PlaceholderRegex = new(@"\{(\w+)(?::([^}]+))?\}", RegexOptions.Compiled);

        public static string? FormatTemplate(string template, string level, string number, string prefix, out string? error)
        {
            error = null;
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var sb = new StringBuilder();
            int pos = 0;

            foreach (Match m in PlaceholderRegex.Matches(template))
            {
                sb.Append(template, pos, m.Index - pos);
                pos = m.Index + m.Length;

                var name = m.Groups[1].Value;
                var format = m.Groups[2].Success ? m.Groups[2].Value : null;

                string? value = name switch
                {
                    "Уровень" or "Level" => level,
                    "N" or "Number" => number,
                    "Д" or "Prefix" => prefix,
                    _ => null
                };

                if (value == null)
                {
                    error = $"Неизвестный плейсхолдер {{{name}}}.";
                    return null;
                }

                if (!string.IsNullOrEmpty(format) && int.TryParse(value, out var intVal))
                    sb.Append(intVal.ToString(format));
                else
                    sb.Append(value);
            }

            sb.Append(template, pos, template.Length - pos);

            return sb.ToString();
        }

        public static string? ValidateTemplate(string template, string level, string number, string prefix)
        {
            if (string.IsNullOrWhiteSpace(template))
                return "Шаблон пуст.";

            FormatTemplate(template, level, number, prefix, out var error);
            if (error != null) return error;

            if (template.Count(c => c == '{') != template.Count(c => c == '}'))
                return "Непарные скобки в шаблоне.";

            return null;
        }
    }
}
