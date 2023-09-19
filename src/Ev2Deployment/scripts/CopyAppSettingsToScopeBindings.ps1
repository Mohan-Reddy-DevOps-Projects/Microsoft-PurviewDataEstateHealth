param (
    [Parameter(Mandatory)]
    [string]
    $AppSettingsPath,
    [Parameter(Mandatory)]
    [string]
    $ScopeBindingsPath
)

# Get a minified version of appsettings.json.
$appSettingsJson = Get-Content -Path $AppSettingsPath | Out-String;
$minifiedAppSettingsJson = ConvertFrom-Json $appSettingsJson | ConvertTo-Json -Compress;
$escapedMinifiedAppSettingsJson = $minifiedAppSettingsJson -replace '"', '\\\"';

# Replace the appsettings placeholder in scopeBindings.json.
$scopeBindings = Get-Content -Path $ScopeBindingsPath
$newScopeBindings = $scopeBindings -replace "__REPLACE_APP_SETTINGS_JSON_PLACEHOLDER__", $escapedMinifiedAppSettingsJson;
$newScopeBindings | Set-Content -Path $ScopeBindingsPath;
