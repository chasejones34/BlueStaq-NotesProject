@echo off
setlocal

cd /d "%~dp0"

echo ========================================
echo Team Notes API - Setup and Run
echo ========================================

where dotnet >nul 2>&1
if errorlevel 1 (
    echo .NET SDK was not found. Install the .NET SDK first.
    pause
    exit /b 1
)

where docker >nul 2>&1
if errorlevel 1 (
    echo Docker was not found. Install Docker Desktop first.
    pause
    exit /b 1
)

if not exist "src\TeamNotes.Api\appsettings.Development.json" (
    if not exist "src\TeamNotes.Api\appsettings.Development.example.json" (
        echo The development configuration template is missing.
        pause
        exit /b 1
    )

    copy /Y "src\TeamNotes.Api\appsettings.Development.example.json" "src\TeamNotes.Api\appsettings.Development.json" >nul
    echo Created appsettings.Development.json from the template.
    echo Update that file with local secrets, then press any key to continue.
    notepad "src\TeamNotes.Api\appsettings.Development.json"
    pause
)

echo Starting PostgreSQL...
docker compose up -d postgres
if errorlevel 1 (
    echo PostgreSQL failed to start.
    pause
    exit /b 1
)

echo Waiting for PostgreSQL to become healthy...
for /L %%N in (1,1,30) do (
    docker inspect -f "{{.State.Health.Status}}" teamnotes-postgres 2>nul | findstr /I /C:"healthy" >nul
    if not errorlevel 1 goto database_ready
    timeout /t 1 /nobreak >nul
)

echo PostgreSQL did not become healthy within 30 seconds.
docker compose ps
pause
exit /b 1

:database_ready
echo PostgreSQL is healthy.

echo Restoring .NET packages...
dotnet restore src\TeamNotes.Api\TeamNotes.Api.csproj
if errorlevel 1 goto failed

dotnet restore tests\TeamNotes.Api.Tests\TeamNotes.Api.Tests.csproj
if errorlevel 1 goto failed

echo Building the API...
dotnet build src\TeamNotes.Api\TeamNotes.Api.csproj --no-restore
if errorlevel 1 goto failed

echo Running tests...
dotnet test tests\TeamNotes.Api.Tests\TeamNotes.Api.Tests.csproj --no-restore
if errorlevel 1 goto failed

echo Starting the API...
echo Swagger: http://localhost:5082/swagger
dotnet run --project src\TeamNotes.Api --no-build
exit /b 0

:failed
echo Setup failed. Review the error above.
pause
exit /b 1
