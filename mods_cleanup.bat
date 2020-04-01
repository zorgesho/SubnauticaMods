@set qmods_path=c:\games\subnautica\QMods
@if not exist %qmods_path% goto :exit

@for /f %%m in (mods_cleanup.txt) do @if exist %qmods_path%\%%m @rmdir %qmods_path%\%%m /q /s
@echo Mods cleaned up!

:exit