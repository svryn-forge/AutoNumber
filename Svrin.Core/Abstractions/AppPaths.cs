using Svrin.Core.Properties;

namespace Svrin.Core.Abstractions
{
    public class AppPaths
    {
        public static readonly string AddInDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            Resources.AddInName);

        public static readonly string LogDirectory = Path.Combine(
            AddInDirectory,
            "_log");
    }
}
