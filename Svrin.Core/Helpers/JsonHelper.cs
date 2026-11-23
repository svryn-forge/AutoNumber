using Newtonsoft.Json;
using System.Text;

namespace Svrin.Core.Helpers
{
    public static class JsonHelper
    {
        public static T? DeserializeObject<T>(string? text) where T : class
        {
            T? result = null;

            if (string.IsNullOrWhiteSpace(text))
                return result;

            try
            {
                result = JsonConvert.DeserializeObject<T>(text!);
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public static string? SerializeObject<T>(T? obj) where T : class
        {
            string? result = null;
            if (obj == null)
                return result;

            try
            {
                result = JsonConvert.SerializeObject(obj);
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public static bool TryReadToObject<T>(string? filePath, out T? obj)
            where T : class
        {
            obj = DeserializeObject<T>(ReadAllText(filePath));
            return obj != null;
        }

        public static bool TryWriteToFile<T>(string? filePath, T? obj)
            where T : class
        {
            WriteAllText(filePath, SerializeObject(obj));
            return File.Exists(filePath);
        }

        public static void WriteAllText(string? filePath, string? value)
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(value))
                return;

            try
            {
                File.WriteAllText(
                    filePath,
                    value,
                    Encoding.UTF8);
            }
            catch { }
        }

        public static string? ReadAllText(string? filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    return File.ReadAllText(filePath, Encoding.UTF8);
                }
                catch { }
            }

            return null;
        }
    }
}
