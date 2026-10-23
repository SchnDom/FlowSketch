param([Parameter(Mandatory=$true)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$imageDirectory = Join-Path $OutputDirectory 'images'
New-Item -ItemType Directory -Path $imageDirectory -Force | Out-Null
Copy-Item -Path (Join-Path $projectRoot '_media/help/*.png') -Destination $imageDirectory
Copy-Item -LiteralPath (Join-Path $projectRoot "_media/favicon-256x256.png") -Destination (Join-Path $imageDirectory "flowsketch-icon.png")
$guide = Join-Path $projectRoot 'docs/Hilfe.md'
Copy-Item -LiteralPath $guide -Destination (Join-Path $OutputDirectory 'Hilfe.md')
$content = [System.Text.StringBuilder]::new()
$navigation = [System.Text.StringBuilder]::new()
$chapterNumber = 0
foreach ($line in (Get-Content -LiteralPath $guide -Encoding utf8)) {
    $safe = [System.Net.WebUtility]::HtmlEncode($line)
    if ($line.StartsWith('## ')) {
        if ($chapterNumber -gt 0) { [void]$content.AppendLine('</section>') }
        $chapterNumber++
        $title = [System.Net.WebUtility]::HtmlEncode($line.Substring(3))
        [void]$navigation.AppendLine("<a href='#kapitel-$chapterNumber'>$title</a>")
        [void]$content.AppendLine("<section id='kapitel-$chapterNumber'><h2>$title</h2>")
    } elseif ($line.StartsWith('### ')) {
        [void]$content.AppendLine('<h3>' + [System.Net.WebUtility]::HtmlEncode($line.Substring(4)) + '</h3>')
    } elseif ($line.StartsWith('# ')) {
        [void]$content.AppendLine('<h1>' + [System.Net.WebUtility]::HtmlEncode($line.Substring(2)) + '</h1>')
    } elseif ($line -match '^!\[([^\]]+)\]\(images/([a-z0-9-]+\.png)\)$') {
        $caption = [System.Net.WebUtility]::HtmlEncode($Matches[1])
        $imageName = $Matches[2]
        if (-not (Test-Path -LiteralPath (Join-Path $imageDirectory $imageName))) { throw "Hilfebild fehlt: $imageName" }
        [void]$content.AppendLine("<figure><a href='images/$imageName'><img src='images/$imageName' alt='$caption'></a><figcaption>$caption</figcaption></figure>")
    } elseif ($line.Trim().Length -gt 0) {
        [void]$content.AppendLine("<p>$safe</p>")
    }
}
if ($chapterNumber -gt 0) { [void]$content.AppendLine('</section>') }
$template = @'
<!doctype html>
<html lang="de"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>FlowSketch &#8211; Hilfe</title><style>
*{box-sizing:border-box}body{margin:0;color:#203544;background:#f2f5f7;font:16px/1.6 "Segoe UI",sans-serif}
.layout{display:grid;grid-template-columns:290px minmax(0,1fr);max-width:1450px;margin:auto}
nav{position:sticky;top:0;height:100vh;overflow:auto;padding:24px;background:#e8eff3}nav a{display:block;padding:7px;color:#005b83;text-decoration:none}
input{width:100%;padding:10px;margin:12px 0;border:1px solid #8b9aa4;border-radius:4px}
main{background:white;padding:35px;min-width:0}section{scroll-margin-top:20px;border-top:1px solid #d7e0e6;padding:20px 0}
.flowsketch-icon{width:72px;height:72px;object-fit:contain;border:0;background:transparent;padding:0;margin-bottom:20px}h1,h2{line-height:1.25}figure{margin:24px 0}img{display:block;width:auto;max-width:100%;max-height:750px;border:1px solid #d7e0e6}
figcaption{font-size:14px;color:#536775;margin-top:8px}a{color:#005b83}
@media(max-width:850px){.layout{display:block}nav{position:static;height:auto}main{padding:20px}}
@media print{nav{display:none}.layout{display:block}main{padding:0}section{break-inside:auto}figure{break-inside:avoid}img{max-height:500px}}
</style></head><body><div class="layout"><nav aria-label="Inhaltsverzeichnis"><b>FlowSketch &#8211; Hilfe</b><input id="search" type="search" placeholder="Hilfe durchsuchen" aria-label="Hilfe durchsuchen">__NAV__</nav><main><img class="flowsketch-icon" src="images/flowsketch-icon.png" alt="FlowSketch">__CONTENT__</main></div>
<script>document.getElementById('search').addEventListener('input',function(){const q=this.value.toLocaleLowerCase('de');document.querySelectorAll('main section').forEach(s=>s.hidden=!s.textContent.toLocaleLowerCase('de').includes(q));});</script></body></html>
'@
$html = $template.Replace('__NAV__',$navigation.ToString()).Replace('__CONTENT__',$content.ToString())
Set-Content -LiteralPath (Join-Path $OutputDirectory 'Hilfe.html') -Value $html -Encoding utf8
Write-Output "Bebilderte Hilfe erstellt: $OutputDirectory"
