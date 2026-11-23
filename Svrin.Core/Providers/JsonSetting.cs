using Newtonsoft.Json;
using Svrin.Core.Helpers;
using Svrin.Core.Properties;

namespace Svrin.Core.Providers
{
    public interface IJsonSetting
    {
        [JsonIgnore]
        string Filename { get; }
    }

    public static class JsonSetting<T> where T : class, IJsonSetting, new()
    {
        public static T Open(string filePath = "")
        {
            T value = new();
            if (string.IsNullOrEmpty(filePath))
                filePath = GetFilePath(value.Filename);

            if (JsonHelper.TryReadToObject(filePath, out T? deserilizedObject))
                value = deserilizedObject!;

            Save(value);

            return value;
        }

        public static void Save(T value, string filePath = "")
        {
            if (value == default)
                return;

            if (string.IsNullOrEmpty(filePath))
                filePath = GetFilePath(value.Filename);

            JsonHelper.TryWriteToFile(filePath, value);
        }

        private static string GetFilePath(string filename)
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Resources.AddInName);

            try
            {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch
            {
                return string.Empty;
            }

            return Path.Combine(path, filename);
        }
    }
}
