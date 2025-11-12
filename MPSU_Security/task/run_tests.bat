@echo off
@chcp 65001 >nul
REM Запуск тестов для Argon2 или Bcrypt (Windows)

IF "%~1"=="" (
    echo Использование: run_tests.bat ^<1^|2^>
    echo 1 - тесты для Argon2 с блокировкой
    echo 2 - тесты для Bcrypt с блокировкой
    echo 1 - тесты для Argon2 с ростом задержки
    echo 2 - тесты для Bcrypt с ростом задержки
    exit /b 1
)

IF "%~1"=="1" (
    echo 🔐 Запуск тестов argon2 с блокировкой
    python -m pytest tests/test_migration_argon2.py tests/test_blocking.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
) ELSE IF "%~1"=="2" (
    echo 🔐 Запуск тестов bcrypt с блокировкой
    python -m pytest tests/test_migration_bcrypt.py tests/test_blocking.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
) ELSE IF "%~1"=="3" (
    echo 🔐 Запуск тестов argon2 с ростом задержки
    python -m pytest tests/test_migration_argon2.py tests/test_delay.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
) ELSE IF "%~1"=="4" (
    echo 🔐 Запуск тестов bcrypt с ростом задержки
    python -m pytest tests/test_migration_bcrypt.py tests/test_delay.py tests/test_password_charset_policy.py tests/test_password_length_policy.py
) ELSE (
  echo Неверный аргумент: %1 (должно быть 1, 2, 3 или 4)
  exit /b 1
)
