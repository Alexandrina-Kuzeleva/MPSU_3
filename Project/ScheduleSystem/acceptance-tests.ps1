Write-Host "Acceptance Tests for Schedule System" -ForegroundColor Cyan
Write-Host "Running 7 acceptance scenarios..." -ForegroundColor Yellow
Write-Host ""

$global:allTestsPassed = $true
$testDataPath = "test-acceptance-data"

function Test-Scenario {
    param(
        [string]$Name,
        [ScriptBlock]$Action,
        [int]$ExpectedExitCode = 0
    )
    
    Write-Host "`n[$(Get-Date -Format 'HH:mm:ss')] Testing: $Name" -ForegroundColor White
    
    try {
        & $Action
        $exitCode = $LASTEXITCODE
        
        if ($exitCode -eq $ExpectedExitCode) {
            Write-Host "PASSED (exit code: $exitCode)" -ForegroundColor Green
            return $true
        } else {
            Write-Host "FAILED - Expected exit code $ExpectedExitCode, got $exitCode" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "FAILED with exception: $_" -ForegroundColor Red
        return $false
    }
}

function Cleanup-TestData {
    if (Test-Path $testDataPath) {
        Remove-Item -Path $testDataPath -Recurse -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path "test_*.csv") {
        Remove-Item -Path "test_*.csv" -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path "test_*.json") {
        Remove-Item -Path "test_*.json" -Force -ErrorAction SilentlyContinue
    }
}

Cleanup-TestData

# Инициализация 
$scenario1 = Test-Scenario "1. Initialization" {
    & sched init --db $testDataPath
} -ExpectedExitCode 0

if ($scenario1) {
    if (Test-Path $testDataPath) {
        $files = Get-ChildItem $testDataPath -Filter "*.json"
        Write-Host "  Created $($files.Count) JSON files in $testDataPath" -ForegroundColor Gray
    }
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario1

#Добавление данных
$scenario2 = Test-Scenario "2. Adding sample data" {
    & sched room add --code A-101 --capacity 30 --building "Main"
    & sched room add --code B-201 --capacity 50 --building "Secondary" 
    & sched room add --code C-301 --capacity 20 --building "Library"
    & sched room add --code D-401 --capacity 40 --building "Science"
    & sched room add --code E-501 --capacity 60 --building "Sports"
    
    1..10 | ForEach-Object {
        & sched teacher add --name "Teacher $_" --email "teacher$_@univ.edu"
    }
    
    1..20 | ForEach-Object {
        & sched group add --code "GROUP-$_" --size (20 + $_) --year (2023 + ($_ % 3))
    }
    
    1..30 | ForEach-Object {
        & sched course add --title "Course $_" --code "CRS$_" --duration (90 + ($_ * 5))
    }

    return $LASTEXITCODE
} -ExpectedExitCode 0

if ($scenario2) {
    Write-Host "  Added: 5 rooms, 10 teachers, 20 groups, 30 courses" -ForegroundColor Gray
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario2

# Повторяющаяся пара
$scenario3 = Test-Scenario "3. Recurring session" {
    & sched session add --course 1 --teacher 1 --group 1 --room 1 `
        --dow MON --from 2025-09-01 --to 2025-12-31 `
        --start 10:00 --end 11:30 --notes "Weekly Algorithms lecture"
} -ExpectedExitCode 0

if ($scenario3) {
    $output = & sched session list --group 1 2>&1 | Out-String
    $mondaySessions = ($output -split "`n" | Where-Object { $_ -like "*MON*" }).Count
    Write-Host "  Created $mondaySessions Monday sessions" -ForegroundColor Gray
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario3

# Конфлит
$scenario4 = Test-Scenario "4. Conflict detection" {
    & sched session add --course 2 --teacher 2 --group 2 --room 2 `
        --date 2025-11-27 --start 10:00 --end 11:30 `
        --notes "First session"
    
    & sched session add --course 3 --teacher 3 --group 3 --room 2 `
        --date 2025-11-27 --start 11:00 --end 12:00 `
        --notes "Should conflict"
} -ExpectedExitCode 4  

if ($scenario4) {
    Write-Host "  Correctly rejected conflict with exit code 4" -ForegroundColor Gray
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario4

#Экспорт CSV
$scenario5 = Test-Scenario "5. CSV export" {
    & sched export csv --entity sessions --out test_export.csv
} -ExpectedExitCode 0

if ($scenario5) {
    if (Test-Path "test_export.csv") {
        $lines = Get-Content "test_export.csv" | Measure-Object -Line
        $header = Get-Content "test_export.csv" -First 1
        Write-Host "  Exported $($lines.Lines) lines to CSV" -ForegroundColor Gray
        Write-Host "  Header: $header" -ForegroundColor Gray
        
        if ($lines.Lines -gt 1 -and $header -eq "Date,Time,Course,Teacher,Group,Room,Notes") {
            Write-Host "  CSV format is correct" -ForegroundColor Green
        }
    }
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario5

#Импорт CSV с ошибками
$scenario6 = Test-Scenario "6. CSV import with errors" {
    @"
Date,Time,Course,Teacher,Group,Room,Notes
2025-11-27,10:00-11:30,Algorithms,Teacher 1,GROUP-1,A-101,Valid session 1
2025-11-27,INVALID_TIME,Mathematics,Teacher 2,GROUP-2,B-201,Bad time format
2025-11-27,13:00-14:30,,Teacher 3,GROUP-3,C-301,Missing course
2025-11-28,09:00-10:30,Physics,Teacher 4,GROUP-4,D-401,Valid session 2
2025-13-45,10:00-11:30,Biology,Teacher 5,GROUP-5,E-501,Invalid date
"@ | Out-File -FilePath "test_import_errors.csv" -Encoding UTF8

    & sched import csv --entity sessions --file "test_import_errors.csv"
} -ExpectedExitCode 0

if ($scenario6) {
    $output = & sched session list 2>&1 | Out-String
    $sessionCount = ($output -split "`n" | Where-Object { $_ -match "\d{4}-\d{2}-\d{2}" }).Count
    Write-Host "  After import: $sessionCount total sessions (should add 2 valid)" -ForegroundColor Gray
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario6

#Поиск конфликтов
$scenario7 = Test-Scenario "7. Find conflicts" {
    & sched session add --course 4 --teacher 1 --group 1 --room 1 `
        --date 2025-12-01 --start 14:00 --end 15:30
        
    & sched session add --course 5 --teacher 1 --group 2 --room 3 `
        --date 2025-12-01 --start 14:30 --end 16:00
        
    & sched session conflicts
} -ExpectedExitCode 0

if ($scenario7) {
    Write-Host "  Conflict search completed" -ForegroundColor Gray
}

$global:allTestsPassed = $global:allTestsPassed -and $scenario7

Write-Host "`n" + ("=" * 60) -ForegroundColor Cyan
Write-Host "ACCEPTANCE TESTS SUMMARY" -ForegroundColor Cyan
Write-Host "=" * 60 -ForegroundColor Cyan

$scenarios = @($scenario1, $scenario2, $scenario3, $scenario4, $scenario5, $scenario6, $scenario7)
for ($i = 0; $i -lt $scenarios.Count; $i++) {
    $status = if ($scenarios[$i]) { "PASSED" } else { "FAILED" }
    Write-Host "Scenario $($i+1): $status" -ForegroundColor $(if ($scenarios[$i]) { "Green" } else { "Red" })
}

Write-Host "`nOverall: $(if ($global:allTestsPassed) { 'ALL TESTS PASSED' } else { 'SOME TESTS FAILED' })" -ForegroundColor $(if ($global:allTestsPassed) { "Green" } else { "Red" })

Cleanup-TestData

Write-Host "`nTest data cleaned up." -ForegroundColor Gray

exit $(if ($global:allTestsPassed) { 0 } else { 1 })