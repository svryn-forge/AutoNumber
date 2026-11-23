# AutoNumber

### Версии Revit и целевые платформы

| Проект | Revit 2022 | Revit 2024 | Revit 2025 |
|--------|------------|------------|------------|
| Svrin.Core   | .NET Standard 2.0 | .NET Standard 2.0 | .NET Standard 2.0 |
| Svrin.WPF    | .NET Framework 4.8 | .NET Framework 4.8 | .NET 8 |
| Svrin.App    | .NET Framework 4.8 | .NET Framework 4.8 | .NET 8 |

### Каталог `Build`

Служебные файлы сборки, общие для всех проектов решения.

| Файл | Назначение |
|------|------------|
| `build-script.ps1` | Скрипт сборки всех проектов во всех конфигурациях (`r2022`, `r2024`, `r2025`). |
| `RvtVersions.props` | Сопоставление конфигурации → версия Revit, `TargetFramework`, путь к Revit API. |
| `RevitProject.props` | Общие настройки Revit-проектов: ссылки на `RevitAPI.dll`/`RevitAPIUI.dll`, копирование в папку Revit. |
| `RvtInstallPath.props` | Пути установки Revit (`Revit2022Path`, `Revit2024Path`, `Revit2025Path`). |

# Сборка
Запустить скрипт, он сам найдет все проекты и конфигурации
```powershell
cd D:\AutoNumber\Build
powershell -ExecutionPolicy Bypass -File "build-script.ps1"
```

# Запуск Revit с отладкой

### Предусловия

- В `Build\RvtInstallPath.props` указаны актуальные пути к установленным Revit (`Revit2022Path`, `Revit2024Path`, `Revit2025Path`).
- Выбрана конфигурация решения, соответствующая версии Revit: `r2022`, `r2024` или `r2025`.
- Проект `Svrin.App` собирается в папку `AddIns` Revit (см. `RevitProject.props`), иначе Revit не увидит плагин.
- Revit закрыт перед сборкой — иначе MSBuild не сможет перезаписать `Svrin.App.dll` в `AddIns`.

### Запуск из Visual Studio

1. Откройте решение `AutoNumber.sln`.
2. В toolbar выберите конфигурацию: `r2022`, `r2024` или `r2025`.
3. Убедитесь, что стартовым проектом назначен `Svrin.App` (не `Svrin.Wpf` и не `Svrin.Core`).
4. Нажмите **F5** (или **Debug → Start Debugging**).

Visual Studio запустит `Revit.exe` из пути, указанного в `$(RevitApiPath)` (см. `RevitProject.props` → `StartProgram`). Точки останова в коде плагина начнут срабатывать после загрузки `Svrin.App.dll` в процессе Revit.
