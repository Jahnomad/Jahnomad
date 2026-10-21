# Утвержденные ресурсы — загрузка одним ZIP

Сюда можно загрузить **один** файл `Reutov_Unity_Menu_Assets_v2.zip`.

Workflow `Import approved Reutov artwork` проверит его содержимое и скопирует:
- `Assets/Resources/MenuMaster.png` — утверждённый главный экран, надпись «ЛАРЁК» на киоске.
- `Assets/Resources/MenuTheme.ogg` — оригинальная полная синтвейв-композиция.

Ресурсы автоматически попадут в ветку `main`. Затем нужна одна новая **Unity Cloud WebGL**-сборка: статическая публикация не перекомпилирует C#.
