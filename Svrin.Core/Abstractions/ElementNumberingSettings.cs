using Svrin.Core.Providers;

namespace Svrin.Core.Abstractions
{
    public class ElementNumberingSettings : IJsonSetting
    {
        public string Filename { get; } = "ElementNumberingSettings.json";

        public List<SettingCategory> CategorySet { get; set; } = [];

        public static void LoadDefault(ElementNumberingSettings settings)
        {
            if (settings.CategorySet.Count != 0)
                return;

            settings.CategorySet.AddRange([
                new()
                {
                    // BuiltInCategory.OST_Walls
                    Id = -2000011L,
                    Name = "Стены",
                    ParameterName = "Марка",
                    Prefix = "С"
                },
                new()
                {
                    // BuiltInCategory.OST_Doors
                    Id = -2000023L,
                    Name = "Двери",
                    ParameterName = "Марка",
                    Prefix = "Д"
                },
                new()
                {
                    // BuiltInCategory.OST_Windows
                    Id = -2000014L,
                    Name = "Окна",
                    ParameterName = "Марка",
                    Prefix = "О"
                },
                new()
                {
                    // BuiltInCategory.OST_Rooms
                    Id = -2000160L,
                    Name = "Помещения",
                    ParameterName = "Номер",
                    Prefix = "Р"
                }
            ]);
        }
    }
}
