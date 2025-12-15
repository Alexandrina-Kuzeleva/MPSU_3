# sched возможности

* Инициализация и хранение данных в локальной БД
* Управление аудиториями, преподавателями, группами и курсами
* Разовые и повторяющиеся занятия
* Поиск конфликтов расписания
* Экспорт и импорт данных (CSV, JSON)
* Формирование отчётов (по группам, преподавателям, аудиториям, датам)

---
## /ScheduleSystem

## Сборка и установка

```bash
dotnet clean
dotnet build
dotnet pack -c Release
cd nupkg
dotnet tool install --global sched --version 1.0.0 --add-source .
```

После установки команда `sched` будет доступна глобально.

---

## Инициализация и обслуживание БД

```bash
sched init [--db PATH]
sched clear
```

* `init` — создаёт базу данных
* `clear` — удаляет все данные

---

## Резервное копирование

```bash
sched backup --out backup.json
sched restore --from backup.json
```

---

## Аудитории (Rooms)

```bash
sched room add --code A-101 --capacity 30 --building "Main"
sched room list [--building NAME] [--min-capacity N] [--max-capacity N]
sched room show <id|code>
sched room update <id|code> [--code NEW] [--capacity N] [--building B]
sched room delete <id|code>
```

---

## Преподаватели (Teachers)

```bash
sched teacher add --name "Ivanov I.I." --email "ivanov@univ.edu"
sched teacher list [--name-like PATTERN]
sched teacher show <id>
sched teacher update <id> [--name NEW] [--email NEW]
sched teacher delete <id>
```

---

## Учебные группы (Groups)

```bash
sched group add --code CS-2025 --size 25 --year 2025
sched group list [--year N] [--min-size N] [--max-size N]
sched group show <id|code>
sched group update <id|code> [--code NEW] [--size N] [--year N]
sched group delete <id|code>
```

---

## Курсы (Courses)

```bash
sched course add --title "Algorithms" --code CS101 --duration 90
sched course list [--title-like PATTERN] [--min-duration N] [--max-duration N]
sched course show <id|code>
sched course update <id|code> [--title NEW] [--code NEW] [--duration N]
sched course delete <id|code>
```

---

## Занятия (Sessions)

### Разовое занятие

```bash
sched session add --course 1 --teacher 1 --group 1 --room 1 \
  --date 2025-11-27 --start 10:00 --end 11:30 --notes "Lecture"
```

### Повторяющееся занятие

```bash
sched session add --course 1 --teacher 1 --group 1 --room 1 \
  --dow MON --from 2025-09-01 --to 2025-12-31 \
  --start 10:00 --end 11:30 --notes "Weekly lecture"
```

### Управление и анализ

```bash
sched session list [--group ID] [--teacher ID] [--room ID] [--date DATE]
sched session conflicts
sched session show <id>
sched session update <id> [--room ID] [--start HH:MM] [--end HH:MM]
sched session delete <id>
```

---

## Экспорт и импорт

### CSV

```bash
sched export csv --entity sessions --out sessions.csv
sched import csv --entity sessions --file sessions.csv [--mode append|replace]
```

### JSON

```bash
sched export json --entity sessions --out sessions.json
sched export json --out full_backup.json
sched import json --entity sessions --file sessions.json
```

---

## Отчёты

```bash
sched report group --group 1 [--from DATE] [--to DATE] [--format text|csv|json]
sched report teacher --teacher 1 [--from DATE] [--to DATE]
sched report room --room 1 [--from DATE] [--to DATE]
sched report day --date 2025-11-27
sched report week --group 1 --from 2025-11-24 --to 2025-11-30
```

---

## Коды завершения

| Код | Описание            | Пример                           |
| --: | ------------------- | -------------------------------- |
|   0 | Успешное выполнение | `sched room list`                |
|   1 | Общая ошибка        | Неизвестная команда              |
|   2 | Ошибка валидации    | Неверный формат времени          |
|   3 | Не найдено          | Аудитория с ID 999 не существует |
|   4 | Конфликт            | Аудитория занята в это время     |
|   5 | Ошибка файла        | Файл не найден                   |

---
## /ScheduleSystem.Tests
## Тестирование

```bash
dotnet test
```

Из проекта ScheduleSystem доступно `./acceptance-tests.ps1`