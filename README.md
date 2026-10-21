# Затерянные в Реутове — Unity 6

Студия: **JAH NOMAD**. Исходный оригинальный проект на C# для Unity Build Automation.

## Статус (Sprint 0)

В ветку main загружены:
- `Assets/Scripts/Core/BootFlowController.cs` — заставка, русское меню, настройки, кнопки;
- `Assets/Editor/ReutovSceneSetup.cs` — создание стартовой сцены при первом импорте;
- `Assets/Editor/ReutovAssetImporter.cs` — настройки импорта графики;
- `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`, `.gitignore`.

**ВАЖНО: этот коммит ещё не является готовым игровым билдом.** В GitHub пока отсутствуют оригинальные файлы `Assets/Resources/*.png` и `JahNomadSting.ogg`. Пока они не добавлены, изображения заставки и меню отсутствуют. Компиляция в Unity Cloud не проверена.

## Необходимые оригинальные ресурсы

`Backdrop.png, StudioLogo.png, GameTitle.png, NewGame.png, Settings.png, Quit.png, Back.png, Skip.png, SoundOn.png, SoundOff.png, PrologueCard.png, JahNomadSting.ogg`.

Эти файлы имеются в локальном архиве `Zateryannye_Reutove_Unity_Sprint0_Step1.zip`, доступном в чате. Их нужно загрузить в каталог `Assets/Resources/`. Исходный коммерческий APK, чужие игровые ресурсы и секреты GitHub **не загружать**.

## Cloud Build

После загрузки графики и звука настройте Unity Build Automation: репозиторий `Jahnomad/Jahnomad`, ветка `main`, корень Unity-проекта — `/`, версия Unity `6000.0.58f2`, целевая платформа Web. Сборка и журнал ошибок обязательны для подтверждения запуска.
