# Aurora's BnS Material Tracker — Build Script
# Publishes the app, builds the installer, zips the portable version,
# and creates/updates a GitHub Release with both files.

$Root    = $PSScriptRoot
$ExeName = "Aurora's BnS Material Tracker"   # user-facing filename (without .exe)
$ExeFile = "$ExeName.exe"                     # full filename with extension
$Repo    = "EvansGoethe/Aurora-s-Bns-Material-Tracker"
$gh      = "C:\Program Files\GitHub CLI\gh.exe"

# Read version from csproj (single source of truth)
[xml]$csproj = Get-Content "$Root\BnsMaterialTracker.csproj"
$Version = ($csproj.Project.PropertyGroup | ForEach-Object { $_.Version } |
            Where-Object { $_ } | Select-Object -First 1)
if (-not $Version) { $Version = "1.0.0" }
$Tag = "v$Version"
Write-Host "Version: $Version  ($Tag)" -ForegroundColor DarkCyan

# ── 1. Kill any running instance ──────────────────────────────
Write-Host "[1/4] Stopping any running instance..." -ForegroundColor Cyan
Stop-Process -Name $ExeName -Force -ErrorAction SilentlyContinue
Stop-Process -Name "Auroras BnS Material Tracker" -Force -ErrorAction SilentlyContinue

# ── 2. Publish ────────────────────────────────────────────────
Write-Host "[2/4] Publishing..." -ForegroundColor Cyan
dotnet publish "$Root\BnsMaterialTracker.csproj" `
    -c Release -r win-x64 --no-self-contained `
    -p:PublishSingleFile=true `
    -o "$Root\publish"

if ($LASTEXITCODE -ne 0) { Write-Host "Publish failed." -ForegroundColor Red; exit 1 }

# Rename: AssemblyName cannot have apostrophe (breaks WPF pack URIs)
$from = "$Root\publish\Auroras BnS Material Tracker.exe"
$to   = "$Root\publish\$ExeFile"
if (Test-Path $from) {
    Remove-Item $to -Force -ErrorAction SilentlyContinue
    Rename-Item -Path $from -NewName $ExeFile
}

# ── 3. Build installer ────────────────────────────────────────
Write-Host "[3/4] Building installer..." -ForegroundColor Cyan

$iscc = @(
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Host "Inno Setup not found. Skipping installer + release." -ForegroundColor Yellow
    exit 0
}

New-Item -ItemType Directory -Force "$Root\installer_output" | Out-Null
& $iscc /DAppVersion=$Version "$Root\installer.iss"
if ($LASTEXITCODE -ne 0) { Write-Host "Installer build failed." -ForegroundColor Red; exit 1 }

$SetupFile = "$Root\installer_output\Setup_Aurora_BnS_Material_Tracker_v${Version}.exe"

# ── 3b. Create portable ZIP ───────────────────────────────────
$ZipName    = "Aurora_BnS_Material_Tracker_v${Version}_portable.zip"
$ZipPath    = "$Root\installer_output\$ZipName"
$ZipStaging = "$Root\installer_output\_zip_staging"

Write-Host "     Packaging portable zip..." -ForegroundColor Cyan
Remove-Item $ZipStaging -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory $ZipStaging | Out-Null
Copy-Item "$Root\publish\$ExeFile" $ZipStaging
Copy-Item "$Root\publish\Data"     "$ZipStaging\Data" -Recurse
Remove-Item $ZipPath -Force -ErrorAction SilentlyContinue
Compress-Archive -Path "$ZipStaging\*" -DestinationPath $ZipPath
Remove-Item $ZipStaging -Recurse -Force

# ── 4. GitHub Release ─────────────────────────────────────────
Write-Host "[4/4] Publishing GitHub Release $Tag..." -ForegroundColor Cyan

if (-not (Test-Path $gh)) {
    Write-Host "GitHub CLI not found. Skipping release." -ForegroundColor Yellow
    Write-Host "  Setup : $SetupFile"
    Write-Host "  Zip   : $ZipPath"
    exit 0
}

# Delete existing release/tag if present (allows re-running for same version)
& $gh release delete $Tag --repo $Repo --yes 2>$null
& $gh api "repos/$Repo/git/refs/tags/$Tag" --method DELETE 2>$null

# Create release with auto-generated notes
& $gh release create $Tag `
    --repo  $Repo `
    --title "Aurora's BnS Material Tracker $Tag" `
    --generate-notes `
    $SetupFile `
    $ZipPath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Done!" -ForegroundColor Green
    Write-Host "  Setup    : $SetupFile"
    Write-Host "  Portable : $ZipPath"
    Write-Host "  Release  : https://github.com/$Repo/releases/tag/$Tag"
} else {
    Write-Host "GitHub Release failed." -ForegroundColor Red
    exit 1
}
