# CLI — команды и опции (полный набор)

Ярлык CLI: `sched` (пример). Структура команд: `sched <entity> <action> [options]`.

1. Общие:

   * `sched init [--db PATH]` — инициализация БД и создание файла конфигурации. 50/50
   * `sched config show` — показать текущие настройки. 
   * `sched backup --out path` — экспорт полной БД в файл (JSON). 100
   * `sched restore --from path` — восстановление из бэкапа. 100

2. Rooms:

   * `sched room add --code CODE --capacity N [--building B] [--attr JSON]` 100
   * `sched room list [--filter <expr>] [--sort field]`100
   * `sched room show <id|code>` 100
   * `sched room update <id|code> [--code] [--capacity] ...` 100
   * `sched room delete <id|code>`100

3. Teachers:

   * `sched teacher add --name "Ivanov I." [--email ...]` 100
   * `sched teacher list [--filter]`100
   * `sched teacher show <id>` 100
   * `sched teacher update <id> ...` 100
   * `sched teacher delete <id>` 100

4. Groups:

   * `sched group add --code CS-2025 --size 30 [--year 2025]` 100
   * `sched group list` 100/ `show` 100 / `update` 100/ `delete` 100

5. Courses:

   * `sched course add --title "Algorithms" [--code ALGO101] [--duration 90]` 100
   * `sched course list` 100/ `show` 100/ `update`100 / `delete`100

6. Sessions (основная часть):

   * `sched session add --course COURSE_ID --teacher TEACHER_ID --group GROUP_ID --room ROOM_ID --date YYYY-MM-DD --start HH:MM --end HH:MM [--notes ""]`
     Пример повторения: `--dow MON` и `--recurrence weekly --from 2025-09-01 --to 2025-12-31` 100
   * `sched session list [--group GROUP|--teacher TEACHER|--room ROOM|--date DATE|--from DATE --to DATE] [--conflicts-only]` 100
   * `sched session show SESSION_ID` 100
   * `sched session update SESSION_ID [--room ROOM_ID] [--start HH:MM] ...`100
   * `sched session delete SESSION_ID` 100
   * `sched session find-conflicts [--from DATE --to DATE]` — вывести все конфликты. 100
   * `sched session auto-generate --group GROUP_ID --pattern pattern.json` — (опция) автоматическая генерация по шаблону.

7. Import / Export:

   * `sched import csv --entity sessions --file path.csv [--mode append|replace]`100
   * `sched export csv --entity sessions --out path.csv [--from DATE --to DATE]`100
   * `sched import json --file path.json` / `sched export json --out path.json`100

8. Reports:

   * `sched report group --group GROUP_ID --from DATE --to DATE [--format text|csv|json]` 100
   * `sched report teacher --teacher TEACHER_ID ...`
   * `sched report room --room ROOM_ID ...`
   * `sched report day --date DATE` — расписание на день.

dotnet clean
dotnet build
dotnet pack -c Release
cd nupkg
dotnet tool install --global sched --version 1.0.0 --add-source .