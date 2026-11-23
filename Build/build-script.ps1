# запуск в консоли
# cd D:\AutoNumber\Build
# powershell -ExecutionPolicy Bypass -File "build-script.ps1"

#
# build-all.ps1 — собирает все проекты
# во всех конфигурациях до первой ошибки
# Порядок сборки: Core → Wpf → App
#

$root = "D:\AutoNumber"

$versions = @(
    "r2022",
    "r2024",
    "r2025"
)

# Порядок сборки
$orderedProjects = @(
    "$root\Svrin.Core\Svrin.Core.csproj",
    "$root\Svrin.Wpf\Svrin.Wpf.csproj",
    "$root\Svrin.App\Svrin.App.csproj"
)

# Проверяем, что все проекты существуют
foreach ($projPath in $orderedProjects) {
    if (-not (Test-Path $projPath)) {
        Write-Host "Проект не найден: $projPath" -ForegroundColor Red
        exit 1
    }
}

# Информационный вывод по всем найденным проектам
$allProjects = Get-ChildItem -Path $root -Recurse -Filter *.csproj -File |
    Where-Object {
        $_.FullName -notmatch '\\(bin|obj|\.git|\.vs)\\'
    }

Write-Host "Найдено проектов в каталоге: $($allProjects.Count)" -ForegroundColor Cyan
$allProjects | ForEach-Object { Write-Host "  $($_.FullName)" }

# Предупреждение, если порядок сборки не покрывает все найденные проекты
$orderedNames = $orderedProjects | ForEach-Object { Split-Path $_ -Leaf }
$foundNames   = $allProjects    | ForEach-Object { $_.Name }
$missing = $foundNames | Where-Object { $_ -notin $orderedNames }
if ($missing) {
    Write-Host "`nВНИМАНИЕ: следующие проекты не включены в порядок сборки:" -ForegroundColor Yellow
    $missing | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
}

# Очистка bin/obj
Write-Host "`nОчистка bin/obj во всех проектах..." -ForegroundColor Cyan
foreach ($projPath in $orderedProjects) {
    $projDir = Split-Path $projPath -Parent
    foreach ($sub in @("bin", "obj")) {
        $dir = Join-Path $projDir $sub
        if (Test-Path $dir) {
            Remove-Item $dir -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  Удалено: $dir" -ForegroundColor DarkGray
        }
    }
}

# Сборка
foreach ($projPath in $orderedProjects) {

    $projName = Split-Path $projPath -Leaf

    Write-Host "`n=== $projName ===" -ForegroundColor Magenta

    foreach ($config in $versions) {

        Write-Host "`nBuilding $projName [$config] ..." -ForegroundColor Cyan

        dotnet build $projPath -c $config --nologo

        if ($LASTEXITCODE -ne 0) {

            Write-Host ""
            Write-Host "(╯°□°）╯︵ ┻━┻ BUILD FAILED" -ForegroundColor Red
            Write-Host "Проект: $projPath" -ForegroundColor Red
            Write-Host "Конфигурация: $config" -ForegroundColor Red
            Write-Host "Код ошибки: $LASTEXITCODE" -ForegroundColor Red

            exit 1
        }

        Write-Host "SUCCESS: $projName [$config]" -ForegroundColor Green
    }
}

Write-Host "`nВсе проекты во всех конфигурациях собраны успешно." -ForegroundColor Green

exit 0