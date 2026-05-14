[CmdletBinding()]
param()

if (!$IsMacOS) {
    return
}

$selectedDeveloperDir = ''
try {
    $selectedDeveloperDir = (& xcode-select -p) 2>$null
} catch {
}

if ($selectedDeveloperDir -and (Test-Path "$selectedDeveloperDir/../Info.plist")) {
    return
}

$candidateDeveloperDirs = @(
    '/Applications/Xcode.app/Contents/Developer'
)

$candidateDeveloperDirs += Get-ChildItem -Path /Applications -Filter 'Xcode*.app' -Directory -ErrorAction SilentlyContinue |
    Sort-Object FullName |
    ForEach-Object { "$($_.FullName)/Contents/Developer" }

$candidateDeveloperDirs |
    Select-Object -Unique |
    Where-Object { Test-Path "$_/../Info.plist" } |
    Select-Object -First 1
