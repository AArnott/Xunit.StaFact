if ((git -C $PSScriptRoot rev-parse --is-shallow-repository) -eq 'true')
{
    Write-Host "Shallow clone detected, disabling NBGV Git engine so the build can succeed."
    $env:NBGV_GitEngine='Disabled'
}

$appleDeveloperDir = & "$PSScriptRoot/../tools/Get-AppleDeveloperDir.ps1"
if ($appleDeveloperDir)
{
    Write-Host "Setting DEVELOPER_DIR to $appleDeveloperDir so dotnet can build macOS targets."
    $env:DEVELOPER_DIR = $appleDeveloperDir
}
