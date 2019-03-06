set baseUrl=%CD%

start /d Frontend dotnet Frontend.dll
start /d Backend dotnet Backend.dll
start /d TextListener dotnet TextListener.dll

if errorlevel 0 goto exit
:error
echo Error
:exit