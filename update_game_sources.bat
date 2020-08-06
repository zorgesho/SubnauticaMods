@echo off

if %1 == SN goto :SN
if %1 == SNexp goto :SNexp
goto :eof

:SN
if exist c:\games\.subnautica.stable (set game_folder=c:\games\.subnautica.stable) else (set game_folder=c:\games\subnautica)
set game_dll_folder=%game_folder%\Subnautica_Data\Managed
goto :main

:SNexp
set game_folder=d:\games\steamapps\common\subnautica
set game_dll_folder=%game_folder%\Subnautica_Data\Managed
goto :main

:main
set project_depends=%cd%\.dependencies\game%1
set game_sources=%cd%\..\game_sources\%1
::goto :sources

:: copy to project dependencies
copy %game_dll_folder%\Assembly-CSharp.dll %project_depends%\
copy %game_dll_folder%\Assembly-CSharp-firstpass.dll %project_depends%\

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

start c:\tools\tortoise_svn\bin\TortoiseProc.exe /command:diff /path:%game_sources%

echo ===========
echo ALL DONE