cd ..

Write-Host "`n=== ТЕСТ КОНФЛИКТОВ: Инициализация и база ===" -ForegroundColor Cyan
dotnet run -- init
dotnet run -- room add --code A-101 --capacity 40
dotnet run -- teacher add --name "Иванов И.И."
dotnet run -- group add --code CS-101 --size 25
dotnet run -- course add --title "ООП на C#" --code OOP101
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --date 2025-12-16 --start 10:00 --end 11:30
Start-Sleep -Seconds 1

Write-Host "`n=== ТЕСТ 1: Конфликт по аудитории ===" -ForegroundColor Red
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --date 2025-12-16 --start 10:30 --end 12:00

Write-Host "`n=== ТЕСТ 2: Конфликт по преподавателю ===" -ForegroundColor Red
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 2 --date 2025-12-16 --start 10:15 --end 11:15

Write-Host "`n=== ТЕСТ 3: Конфликт по группе ===" -ForegroundColor Red
dotnet run -- session add --course 1 --teacher 2 --group 1 --room 2 --date 2025-12-16 --start 10:00 --end 11:30

Write-Host "`n=== ТЕСТ 4: Поиск всех конфликтов ===" -ForegroundColor Magenta
dotnet run -- session conflicts

Write-Host "`n=== ТЕСТ 5: Повторяющаяся пара с конфликтом (должна отказать) ===" -ForegroundColor Red
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --start 10:00 --end 11:30 --dow TUE --from 2025-12-01 --to 2025-12-31

Write-Host "`n=== ТЕСТ ЗАВЕРШЁН. Очистка... ===" -ForegroundColor Green
dotnet run -- clear
Pause