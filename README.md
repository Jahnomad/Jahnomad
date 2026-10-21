# Затерянные в Реутове — JAH NOMAD

**Sprint 0** — стартовая заставка, русское меню и вступление. Оригинальный Unity 6 / C# проект (без сторонних коммерческих игровых ресурсов).

## Проверенный комплект для следующей облачной сборки

- `Assets/Scenes/00_Boot.unity` — физическая сцена, находящаяся в GitHub, с постоянным GUID.
- `Assets/Scenes/00_Boot.unity.meta` — метаданные сцены.
- `ProjectSettings/EditorBuildSettings.asset` — включённая стартовая сцена.
- `Assets/Scripts/Core/BootFlowController.cs` — самостоятельная инициализация при загрузке сцены, заставка, меню, настройки, навигация и WebGL-уведомление о выходе.
- `Assets/Scripts/Core/ReutovRuntimeArt.cs` — оригинальные процедурные пиксельные изображения фона Реутова и русского меню, создаваемые в самой игре при отсутствии PNG. Не зависит от импорта в редакторе.
- `Assets/Editor/ReutovAssetImporter.cs` — настройки импорта будущих статических PNG.
- `Packages/manifest.json` — UGUI, Audio, ImageConversion.
- `ProjectSettings/ProjectVersion.txt` — Unity 6000.0.58f2.
- `tools/validate_unity.py` + `.github/workflows/unity-source-check.yml` — независимый предсборочный контроль.

## Что выявлено по логам Unity Cloud

- Сборка #1: отсутствие Unity Audio при компиляции C# — исправлено.
- Сборка #2: `There were no scenes configured to build!` — устранено сохранением сцены и EditorBuildSettings в GitHub.
- Старую автоматическую генерацию сцены и PNG в редакторе удалили, чтобы устранить зависимость от времени вызова editor callbacks.
- Автоматическая проверка GitHub Actions завершилась **Success**: https://github.com/Jahnomad/Jahnomad/actions/runs/39593402447

## Следующая проверка

Цель `Reutov-WebGL-Test`, ветка `main`, корень проекта `/`, Unity 6000.0.58f2, Windows Micro. Следующая сборка должна взять последнюю ревизию кода. Для чистой проверки сцены рекомендуется **одна Clean Build**.

**Не утверждаем, что игра уже скомпилирована:** Unity Editor в локальной среде отсутствует, GitHub CI проверяет файлы статически; требуется реальная WebGL-сборка #3 и проверка её журнала.

Звук `JahNomadSting.ogg` в GitHub пока не загружен; запуск без аудиоресурса обработан безопасно. Это пока тест заставки и меню, а не полноценный игровой уровень.
