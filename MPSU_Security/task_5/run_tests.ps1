# run.ps1
$args = $args[0]

if ($args -eq "") {
    Write-Output "Использование: run_tests.ps1 <1|2|3|4>"
    Write-Output "1 - тесты для Argon2 с блокировкой"
    Write-Output "2 - тесты для Bcrypt с блокировкой"
    Write-Output "3 - тесты для Argon2 с ростом задержки"
    Write-Output "4 - тесты для Bcrypt с ростом задержки"
    exit 1
} elseif ($args -eq "1") {
    Write-Output "🔐 Запуск тестов argon2 с блокировкой"
    python -m pytest tests/test_migration_argon2.py tests/test_blocking.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
} elseif ($args -eq "2") {
    Write-Output "🔐 Запуск тестов bcrypt с блокировкой"
    python -m pytest tests/test_migration_bcrypt.py tests/test_blocking.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
} elseif ($args -eq "3") {
    Write-Output "🔐 Запуск тестов argon2 с ростом задержки"
    python -m pytest tests/test_migration_argon2.py tests/test_delay.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
} elseif ($args -eq "4") {
    Write-Output "🔐 Запуск тестов bcrypt с ростом задержки"
    python -m pytest tests/test_migration_bcrypt.py tests/test_delay.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
} else {
    Write-Output "Неверный аргумент: $args (должно быть 1, 2, 3 или 4)"
    exit 1
}