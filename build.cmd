@echo off
set Build="%SYSTEMDRIVE%\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MsBuild.exe"
if exist publish rd /s /q publish
%Build% "NET40/Afx.Data.Entity/Afx.Data.Entity.csproj" /t:Rebuild /p:Configuration=Release
%Build% "NET45/Afx.Data.Entity/Afx.Data.Entity.csproj" /t:Rebuild /p:Configuration=Release
dotnet build "NETStandard2.0/Afx.Data.Entity/Afx.Data.Entity.csproj" -c Release 
cd publish
del /q/s *.pdb
del /q/s EntityFramework*
pause