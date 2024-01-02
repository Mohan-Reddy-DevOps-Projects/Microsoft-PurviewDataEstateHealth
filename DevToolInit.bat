if NOT "%MDB_IMAGE_PIPELINE%" == "" (
     :: DevTool installation in headless environment. If you are using SPO feed, you can just copy paste this command, otherwise, you need to fill out nugetfeedorganization and nugetfeedname argument.
     call powershell "iex ""& { $(irm aka.ms/InstallTool.ps1) } DevTool --useazuremanagedidentity '--nugetfeedorganization=msdata/Purview Data Governance' --nugetfeedname=PurviewDataGov"""
) 

if "%MDB_IMAGE_PIPELINE%" == "" (
    :: normal installation that require auth interaction
    call powershell "iex ""& { $(irm aka.ms/InstallTool.ps1) } DevTool"""
)

:: Get System PATH
for /f "tokens=3*" %%A in ('reg query "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v Path') do set syspath=%%A%%B

:: Get User Path
for /f "tokens=3*" %%A in ('reg query "HKCU\Environment" /v Path') do set userpath=%%A%%B

:: Set Refreshed Path
set PATH=%userpath%;%syspath%
