@echo off
set ASPNETCORE_URLS=http://localhost:5000
cd /d %~dp0\src\AiClientManager.Web
(dotnet restore) || exit /b 1
(dotnet run) || exit /b 1
