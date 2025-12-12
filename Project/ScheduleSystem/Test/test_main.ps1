cd ..
Write-Host "`n=== ТЕСТ 1: Инициализация базы ===" -ForegroundColor Cyan
dotnet run -- init
Start-Sleep -Seconds 1

Write-Host "`n=== ТЕСТ 2: Добавляем справочники ===" -ForegroundColor Green
dotnet run -- room add --code A-101 --capacity 40 --building "Корпус 1"
dotnet run -- room add --code B-202 --capacity 30
dotnet run -- teacher add --name "Иванов И.И." --email ivan@mail.ru
dotnet run -- teacher add --name "Петрова А.С."
dotnet run -- group add --code CS-101 --size 25 --year 2025
dotnet run -- group add --code MT-201 --size 30
dotnet run -- course add --title "ООП на C#" --code OOP101 --duration 90
dotnet run -- course add --title "Алгоритмы" --code ALG202
Start-Sleep -Seconds 1

Write-Host "`n=== ТЕСТ 3: Добавляем пары (одноразовая + повторяющаяся) ===" -ForegroundColor Green
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --date 2025-12-16 --start 10:00 --end 11:30 --notes "Лекция"
dotnet run -- session add --course 2 --teacher 2 --group 2 --room 2 --date 2025-12-17 --start 14:00 --end 15:30 --notes "Практика"
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --start 12:00 --end 13:30 --dow MON --from 2025-09-01 --to 2025-12-31 --notes "Еженедельно"
Start-Sleep -Seconds 1

Write-Host "`n=== ТЕСТ 4: ВСЕ возможные списки/выводы ===" -ForegroundColor Magenta

Write-Host "`n--- Список аудиторий ---" -ForegroundColor Yellow
dotnet run -- room list

Write-Host "`n--- Список преподавателей ---" -ForegroundColor Yellow
dotnet run -- teacher list

Write-Host "`n--- Список групп ---" -ForegroundColor Yellow
dotnet run -- group list

Write-Host "`n--- Список предметов ---" -ForegroundColor Yellow
dotnet run -- course list

Write-Host "`n--- Список всех пар ---" -ForegroundColor Yellow
dotnet run -- session list

Write-Host "`n--- Список пар по группе ---" -ForegroundColor Yellow
dotnet run -- session list --group 1

Write-Host "`n--- Список пар по преподавателю ---" -ForegroundColor Yellow
dotnet run -- session list --teacher 1

Write-Host "`n--- Список пар по аудитории ---" -ForegroundColor Yellow
dotnet run -- session list --room 1

Write-Host "`n--- Список пар на день ---" -ForegroundColor Yellow
dotnet run -- session list --date 2025-12-16

Write-Host "`n--- Отчёт по группе (текст) ---" -ForegroundColor Yellow
dotnet run -- report group --group 1 --from 2025-09-01 --to 2025-12-31

Write-Host "`n--- Отчёт по группе в CSV ---" -ForegroundColor Yellow
dotnet run -- report group --group 1 --format csv

Write-Host "`n--- Отчёт по группе в JSON ---" -ForegroundColor Yellow
dotnet run -- report group --group 1 --format json

Write-Host "`n--- Отчёт по преподавателю ---" -ForegroundColor Yellow
dotnet run -- report teacher --teacher 1

Write-Host "`n--- Отчёт по аудитории ---" -ForegroundColor Yellow
dotnet run -- report room --room 1

Write-Host "`n--- Отчёт по дню ---" -ForegroundColor Yellow
dotnet run -- report day --date 2025-12-16

Write-Host "`n=== ТЕСТ ЗАВЕРШЁН. База готова для других тестов. ===" -ForegroundColor Green
Pause