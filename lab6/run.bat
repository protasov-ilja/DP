set baseUrl=%CD%

start "FrontendAPI" /d Frontend dotnet Frontend.dll
start "BackendAPI" /d Backend dotnet Backend.dll
start "TextListener" /d TextListener dotnet TextListener.dll
start "TextRankCalc" /d TextRankCalc dotnet TextRankCalc.dll
setlocal enabledelayedexpansion
set ind=0
for /f "usebackq tokens=1*" %%a in ("backend_config.txt") do (
	set "a=%%a"& set "b=%%b"
	if !ind!==2 (
		for /l %%n in (1,1,!b!) do (
			start "VowelConsCounter" /d VowelConsCounter dotnet VowelConsCounter.dll
		)
	)
	if !ind!==3 (
		for /l %%n in (1,1,!b!) do (
			start "VowelConsRater" /d VowelConsRater dotnet VowelConsRater.dll
		)
	)
	set /a ind+=1
)

if errorlevel 0 goto exit
:error
echo Error
:exit