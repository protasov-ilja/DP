rem this bat file is build netcore project for release
IF %1=="" GOTO empty_params

SET semVer=%1
SET baseUrl=%CD%

dotnet publish Frontend -c Release -o %baseUrl%\%1\Frontend
dotnet publish Backend -c Release -o %baseUrl%\%1\Backend

CD %baseUrl%\%1

MD "config"
COPY /Y "%baseUrl%\run.bat" "%baseUrl%\%1\run.bat"
COPY /Y "%baseUrl%\stop.bat" "%baseUrl%\%1\stop.bat"
COPY /Y "%baseUrl%\config.txt" "%baseUrl%\%1\backend_config.txt"
COPY /Y "%baseUrl%\config.txt" "%baseUrl%\%1\frontend_config.txt"

IF ERRORLEVEL 0 GOTO exit

:empty_params
@ECHO empty parameters please input semVer MAJOR.MINOR.PATCH

:exit