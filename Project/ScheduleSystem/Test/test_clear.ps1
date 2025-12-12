cd ..

Write-Host "`n=== ТЕСТ ОЧИСТКИ: Текущая база (перед очисткой) ===" -ForegroundColor Cyan
dotnet run -- session list

Write-Host "`n=== Выполняем очистку... ===" -ForegroundColor Red
dotnet run -- clear

Write-Host "`n=== Проверяем, что база пуста ===" -ForegroundColor Green
dotnet run -- session list  # Должно быть "No sessions"

Write-Host "`n=== ТЕСТ ЗАВЕРШЁН. База очищена. ===" -ForegroundColor Green
Pause