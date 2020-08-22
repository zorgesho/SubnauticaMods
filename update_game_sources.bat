:: usage: update_game_sources.bat <SN|BZ>
:: uses currently selected branch
@echo off

set games_folder=c:\games
set /p game_ver=<%games_folder%\branch%1%.info

if %1 == SN goto :SN
if %1 == BZ goto :BZ
goto :eof

:SN
set game_folder=%games_folder%\subnautica
set game_dll_folder=%game_folder%\Subnautica_Data\Managed

set /p buildtime=<%game_folder%\Subnautica_Data\StreamingAssets\__buildtime.txt
set /p version=<%game_folder%\Subnautica_Data\StreamingAssets\SNUnmanagedData\plastic_status.ignore
goto :main

:BZ
set game_folder=%games_folder%\subnautica.bz
set game_dll_folder=%game_folder%\SubnauticaZero_Data\Managed

set /p buildtime=<%game_folder%\SubnauticaZero_Data\StreamingAssets\__buildtime.txt
set /p version=<%game_folder%\SubnauticaZero_Data\StreamingAssets\SNUnmanagedData\plastic_status.ignore
goto :main

:main
set project_depends=%cd%\.dependencies\game%game_ver%
set game_sources=%cd%\..\game_sources\%game_ver%
::goto :sources

:: update dlls in project dependencies
xcopy %game_dll_folder%\*.dll %project_depends% /u /y /q

:: publicize
copy c:\programming\assembly_publicizer\AssemblyPublicizer.exe %game_dll_folder% 1>nul
cd /d %game_dll_folder%
echo Publicizing Assembly-CSharp.dll ...
AssemblyPublicizer.exe --input=Assembly-CSharp.dll --output=%project_depends%\Assembly-CSharp.pb.dll

echo Publicizing Assembly-CSharp-firstpass.dll ...
AssemblyPublicizer.exe --input=Assembly-CSharp-firstpass.dll --output=%project_depends%\Assembly-CSharp-firstpass.pb.dll
del AssemblyPublicizer.exe

:: generate projects
copy %game_dll_folder%\Assembly-CSharp.dll %game_sources%\dlls
copy %game_dll_folder%\Assembly-CSharp-firstpass.dll %game_sources%\dlls

:sources
echo Generating sources for Assembly-CSharp.dll ...
ilspycmd %game_sources%\dlls\Assembly-CSharp.dll -p -o %game_sources%\Assembly-CSharp\ -r %game_dll_folder%

echo Generating sources for Assembly-CSharp-firstpass.dll ...
ilspycmd %game_sources%\dlls\Assembly-CSharp-firstpass.dll -p -o %game_sources%\Assembly-CSharp-firstpass\ -r %game_dll_folder%

start c:\tools\tortoise_svn\bin\TortoiseProc.exe /command:commit /path:%game_sources% /logmsg:"v%version% (%buildtime%)"

echo ===========
echo ALL DONE (%game_ver%)