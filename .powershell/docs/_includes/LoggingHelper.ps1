# Ensure PowerShell-YAML module is available
if (-not (Get-Module -ListAvailable -Name PoShLog)) {
    Install-Module -Name PoShLog -Force -Scope CurrentUser
    Import-Module -Name PoShLog
}
else {
    Import-Module -Name PoShLog
}

If (-not $levelSwitch) {
    $levelSwitch = New-LevelSwitch -MinimumLevel Information
    # Create new logger
    # This is where you customize, when and how to log
    New-Logger |
    Set-MinimumLevel -ControlledBy $levelSwitch | # You can change this value later to filter log messages
    # Here you can add as many sinks as you want - see https://github.com/PoShLog/PoShLog/wiki/Sinks for all available sinks
    #Add-SinkPowerShell |   # Tell logger to write log messages to console 
    Add-SinkConsole |     # Tell logger to write log messages to console
    #Add-SinkFile -Path './logs/.log' | # Tell logger to write log messages into file
    Start-Logger
    Write-Debug  "New Logger Started" 
}

function Set-DebugMode {
    param(
        [bool]$EnableDebug = $true
    )
    if ($EnableDebug) {
        $levelSwitch.MinimumLevel = 'Debug'
        Write-InfoLog "Debug mode enabled."
    }
    else {
        $levelSwitch.MinimumLevel = 'Information'
        Write-InfoLog "Debug mode disabled."
    }
}

function Get-IsDebug {
    <#
    .SYNOPSIS
        Returns $true if debug or verbose mode is enabled, otherwise $false.
    #>
    # LogEventLevel: Verbose=0, Debug=1, Information=2, ...
    if ([int]$levelSwitch.MinimumLevel -le 1) {
        return $true
    }
    else {
        return $false
    }
}




Write-DebugLog "LoggingHelper.ps1 loaded"