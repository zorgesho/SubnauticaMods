@echo off
set qmods_path=c:\games\subnautica\QMods
if not exist %qmods_path% goto :eof

if "%1" == "-reset_config" goto :reset_config

:remove_mods
for /f %%m in (mods_cleanup.txt) do @if exist %qmods_path%\%%m @rmdir %qmods_path%\%%m /q /s
echo mods cleaned up!
goto :eof

:reset_config
if "%2" == "" goto :eof
if exist %qmods_path%\%2\config.json @del %qmods_path%\%2\config.json
echo config.json reseted for %2