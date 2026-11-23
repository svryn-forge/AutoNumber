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

# Категории, доступные для фильтрации

Добавил все сразу, пользователь сам может выбрать в настройках, что ему нужно.

Там же, нужно указать наименование целевого параметра, префикс и разрядность для номера.

Шаблон для нового значения взял из тз

```csharp
private string _template = "{Prefix}-{Уровень}-{N:000}";
```

# Решения по порядку нумерации, конфликтам и хранению настроек

- Порядок нумерации - разные категории нумеруются отдельно с разделением по уровням, в зависимости от координат элемента (LocationCurve или LocationPoint). Элементы без уровня или без положения не учитываются.

```csharp
var byCategory = vm.Elements.GroupBy(e => e.CategoryId);
...
.OrderBy(e => e.LevelElevation)      // уровни: снизу вверх
.ThenByDescending(e => e.Y)          // Y: "сверху вниз" на плане
.ThenBy(e => e.X)                    // X: "слева направо" на плане
.ThenBy(e => e.Id)                   // по Id
```
- Если конфликты найдены, пользователю предлагается продолжить, пропустив элементы с одинаковыми занчачениями, после выполнения пользователь может открыть отчёт, где будет перечислена основная информация о конфликтующих элементах;
- Настройки и логи сохраняются в %appdata%\AutoNumber

# Допущения/техдолг из-за неизвестных сроков выполнения

- Основной алгоритм работает внутри одной транзакции, что ограничивает вывод информации об ошибках и отменах от IFailuresPreprocessor, но работает значительно быстрее, чем тоже самое внутри TransactionGroup
- Не удалось протестировать проблемы при совместной работе, потенциально может потребоваться расширение до обработки конкретных ошибок из BuiltInFailures.WorksharingFailures (не стал добавлять, на данный момент может быть избыточно)
- Отказался от реализации DI - слишком долго.
- Отказался от реализации 2 локалей (ru-RU, en-EN) для ui - не долго (все стринги вынести в ресурсы), но выглядит избыточно в данном случае.
- Svrin.Core.Abstractions.ElementFromModel - слишком много ответственности для этого объекта.

# Замеры производительности

Revit 2022
Исходные данные: 11_000 стен

Загрузка в ui : ~1.5 сек

Обновление значений параметров: 41,28 сек

Поиск
- по id 0,0000681 сек
- строковые значения 0,0000490 - 0,0000876 сек

*Для версий 2024 и 2025 проверил запуск и все функции на небольших моделях.

**В основном окне можно включить отображение времени выполнения.

Пример реализации:

```csharp
void/T SomeMethod(...)
{
    bool hasErrors = false;
    Stopwatch stopwatch = Stopwatch.StartNew();
    try
    {
        ...
    }
	...
    finally
    {
        stopwatch.Stop();
        if (!hasErrors && ShowTimeReport)
        {
            MessageBox.Show($"{Resources.TimeResult}: {stopwatch.Elapsed}", Resources.Report);
        }
    }
}
```

# Известные ограничения

### Ожидание ввода с клавиатуры

Реализовано для поля Поиск в UI
```csharp
Task.Delay(300, token).ContinueWith(_ =>
```

### Панель создается только в поддерживаемых версиях

```csharp
public class ExternalApplication : IExternalApplication
...
private static readonly IReadOnlyCollection<int> _supportedVersions =
[
    2022, 2024, 2025
];
```

### Кнопка активна в открытом документе (в семействе неактивна)

```csharp
internal class RenumberCommand : IExternalCommand, IExternalCommandAvailability
...
public bool IsCommandAvailable(UIApplication applicationData, CategorySet _)
{
    if (!applicationData.IsValid())
        return false;

    var uiDoc = applicationData.ActiveUIDocument;
    if (!uiDoc.IsValid())
        return false;

    var doc = uiDoc.Document;
    return doc.IsValid() && !doc.IsFamilyDocument;
}
```

### Запись значений в параметры производится только при открытом документе если, загруженные ранее, данные находились в том же документе

```csharp
internal class ApplyEventHandler : RvtEventHandler<NumberingVM?>
...
public override void Execute(UIApplication app, NumberingVM? vm)
...
if (!app.IsValid()) return;

var uidoc = app.ActiveUIDocument;
if (!uidoc.IsValid()) return;

var doc = uidoc.Document;
if (!doc.IsValid()) return;

if (!string.Equals(
    doc.Title,
    documentTitle,
    StringComparison.InvariantCulture))
    return;
```

# Использование ИИ

Большая часть работы это выжимки из ранее выполненных задач, поэтому ИИ использовал в основном, чтобы всё привести к общему виду.

Тесты сгенерированы полностью, просмотрены и проверены.