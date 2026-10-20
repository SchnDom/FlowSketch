param()
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
$projectFile = Join-Path $projectRoot 'src/PID2Process/PID2Process.csproj'
[xml]$projectDefinition = Get-Content -LiteralPath $projectFile -Raw
$releaseVersion = [string]$projectDefinition.Project.PropertyGroup.Version
if ($releaseVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'Ungültige Release-Version.' }
$releaseFolder = Join-Path $projectRoot "bin/$releaseVersion"
& dotnet publish $projectFile -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o $releaseFolder
if ($LASTEXITCODE -ne 0) { throw 'Release-Publish fehlgeschlagen.' }
$documentationFolder = Join-Path $releaseFolder 'Dokumentation'
$diagnosticsFolder = Join-Path $releaseFolder 'Diagnose'
New-Item -ItemType Directory -Path $documentationFolder, $diagnosticsFolder -Force | Out-Null
foreach ($documentName in @('README.md', 'ProjektZustand.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $documentName) -Destination $documentationFolder
}
Copy-Item -LiteralPath (Join-Path $projectRoot '_media/companyInfo.txt') -Destination $documentationFolder
$symbolsFile = Join-Path $releaseFolder 'FlowSketch.pdb'
if (Test-Path -LiteralPath $symbolsFile) {
    Move-Item -LiteralPath $symbolsFile -Destination (Join-Path $diagnosticsFolder 'FlowSketch.pdb') -Force
}
$exampleFolder = Join-Path $releaseFolder 'Beispiele'
New-Item -ItemType Directory -Path $exampleFolder -Force | Out-Null
Copy-Item -Path (Join-Path $projectRoot 'examples/*') -Destination $exampleFolder
& (Join-Path $PSScriptRoot 'Build-Help.ps1') -OutputDirectory $documentationFolder
Write-Output "Release $releaseVersion bereit: $releaseFolder"
