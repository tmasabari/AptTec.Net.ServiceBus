@echo off
REM In this script, the if not defined nuget_api_key line checks 
REM if the nuget_api_key environment variable is already set. 
REM If it’s not, the script prompts the user to enter their NuGet API key.
REM If an API key is not provided, the script echoes a message and exits. 
REM If an API key is provided or the environment variable was already set, 
REM the script continues to the dotnet nuget push command.
if not defined nuget_api_key (
    set /p nuget_api_key="Enter your NuGet API key: "
    if "%nuget_api_key%"=="" (
        echo No API key provided, exiting...
        exit /b
    )
)

cd AzureServiceBus
del .\bin\release\*.nupkg
dotnet build --configuration Release
if errorlevel 1 (
    echo AzureServiceBus build operation failed.
    exit /b
)
dotnet pack --no-build --configuration Release

dotnet nuget push "**/*.nupkg" --api-key %nuget_api_key% --source https://api.nuget.org/v3/index.json
if errorlevel 1 (
    echo Push operation failed.
    exit /b
) else (
    echo Push operation succeeded.
)

cd ..
cd ServiceBus.Abstractions
del .\bin\release\*.nupkg
dotnet build --configuration Release
if errorlevel 1 (
    echo ServiceBus.Abstractions build operation failed.
    exit /b
)
dotnet pack --no-build --configuration Release

dotnet nuget push "**/*.nupkg" --api-key %nuget_api_key% --source https://api.nuget.org/v3/index.json
if errorlevel 1 (
    echo Push operation failed.
    exit /b
) else (
    echo Push operation succeeded.
)

cd..