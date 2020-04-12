:: %1 $(ConfigurationName)
:: %2 $(ProjectName)
:: %3 $(ProjectDir)
:: %4 $(TargetPath)
:: %5 $(TargetDir)

set qmods_path="c:\games\subnautica\QMods"

:: name is also in the Mod.cs
set tmp_info_file="run the game to generate configs"

if %1 == test_build goto :exit
if not exist %qmods_path% goto :exit

xcopy %4 %qmods_path%\%2\ /q /y
xcopy %3mod.json %qmods_path%\%2\ /q /y
if exist %3assets\ xcopy %3assets %qmods_path%\%2\assets\ /e /q /y

if %1 == publish copy nul %qmods_path%\%2\%tmp_info_file%

:exit