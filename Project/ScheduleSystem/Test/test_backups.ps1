cd ..

Write-Host "`n=== ТЕСТ БЭКАПОВ: Создаём базу ===" -ForegroundColor Cyan
dotnet run -- init
dotnet run -- room add --code A-101 --capacity 40
dotnet run -- teacher add --name "Иванов И.И."
dotnet run -- group add --code CS-101 --size 25
dotnet run -- course add --title "ООП на C#" --code OOP101
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --date 2025-12-16 --start 10:00 --end 11:30
Start-Sleep -Seconds 1

Write-Host "`n=== ТЕСТ 1: Делаем бэкап ===" -ForegroundColor Green
dotnet run -- backup --out test-backup-v1.json

Write-Host "`n=== ТЕСТ 2: Ломаем базу (удаляем пару) ===" -ForegroundColor Red
dotnet run -- session delete 1

Write-Host "`n=== ТЕСТ 3: Восстанавливаем из бэкапа ===" -ForegroundColor Green
dotnet run -- restore --from test-backup-v1.json

Write-Host "`n=== ТЕСТ 4: Проверяем, что всё вернулось (список пар) ===" -ForegroundColor Magenta
dotnet run -- session list

Write-Host "`n=== ТЕСТ 5: Ещё один бэкап после изменений ===" -ForegroundColor Green
dotnet run -- session add --course 1 --teacher 1 --group 1 --room 1 --date 2025-12-17 --start 14:00 --end 15:30
dotnet run -- backup --out test-backup-v2.json

Write-Host "`n=== ТЕСТ 6: Очистка и восстановление заново ===" -ForegroundColor Red
dotnet run -- clear
dotnet run -- restore --from test-backup-v2.json
dotnet run -- session list

Write-Host "`n=== ТЕСТ ЗАВЕРШЁН. Очистка... ===" -ForegroundColor Green
dotnet run -- clear
Pause