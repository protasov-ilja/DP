set baseUrl=%CD%

start /d Frontend dotnet Frontend.dll
start /d Backend dotnet Backend.dll
start /d TextListener dotnet TextListener.dll
start /d TextRankCalc dotnet TextRankCalc.dll
setlocal enabledelayedexpansion
set cnt=0
for /f "usebackq tokens=1*" %%a in ("backend_config.txt") do (
	set "a=%%a"& set "b=%%b"
	if !cnt!==2 (
		for /l %%n in (1,1,!b!) do (
			start /d VowelConsCounter dotnet VowelConsCounter.dll
		)
	)
	if !cnt!==3 (
		for /l %%n in (1,1,!b!) do (
			start /d VowelConsRater dotnet VowelConsRater.dll
		)
	)
	set /a cnt+=1
)

if errorlevel 0 goto exit
:error
echo Error
:exit