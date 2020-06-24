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

echo %2.pdb > ..\.pdb-ignore

if %1 == publish copy nul %5%tmp_info_file% > nul

echo =====
xcopy %5*.* %qmods_path%\%2\ /e /y /exclude:..\.pdb-ignore
del ..\.pdb-ignore

:exit