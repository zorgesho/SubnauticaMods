:: %1 $(ConfigurationName)
:: %2 $(ProjectName)
:: %3 $(ProjectDir)
:: %4 $(TargetPath)
:: %5 $(TargetDir)

:: name is also in the Mod.cs
set tmp_info_file="run the game to generate configs"

set qmods_path="c:\games\subnautica\QMods"
if not exist %qmods_path% goto :eof

echo %1 | findstr /c:"testbuild" 1>nul && goto :eof

echo %2.pdb > ..\.pdb-ignore

:: adding info file to version for publish 
echo %1 | findstr /c:"publish" 1>nul && copy nul %5%tmp_info_file% > nul

:: renaming mod.SN.json/mod.BZ.json to mod.json if any
if exist mod.*.json move mod.*.json mod.json > nul

echo =====
xcopy %5*.* %qmods_path%\%2\ /e /y /exclude:..\.pdb-ignore
del ..\.pdb-ignore