:: %1 $(ConfigurationName)
:: %2 $(ProjectName)
:: %3 $(ProjectDir)
:: %4 $(TargetPath)
:: %5 $(TargetDir)

:: name is also in the Mod.cs
set tmp_info_file="run the game to generate configs"

set qmods_path="c:\games\subnautica\QMods"
if not exist %qmods_path% goto :eof

echo %1 | findstr /c:"testbuild" 1>nul
if %ERRORLEVEL% == 0 goto :eof

:: removing .NET Standard assemblies
del %5"System.*.dll" 2>nul
del %5"netstandard.dll" 2>nul
del %5"Microsoft.Win32.Primitives.dll" 2>nul

echo %2.pdb > ..\.pdb-ignore

:: adding info file to version for publish 
echo %1 | findstr /c:"publish" 1>nul
if %ERRORLEVEL% == 0 copy nul %5%tmp_info_file% > nul

:: renaming mod.SN.json/mod.BZ.json to mod.json
if not exist mod.*.json goto :copy_files
del mod.json 2>nul
ren mod.*.json mod.json

:copy_files
echo =====
xcopy %5*.* %qmods_path%\%2\ /e /y /exclude:..\.pdb-ignore
del ..\.pdb-ignore