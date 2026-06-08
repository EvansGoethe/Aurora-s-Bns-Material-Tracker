# Aurora's BnS Material Tracker — Build Script
# Publishes the app and rebuilds the installer in one step.

$Root    = $PSScriptRoot
$ExeName = "Aurora's BnS Material Tracker"   # user-facing filename (without .exe)
$ExeFile = "$ExeName.exe"                     # full filename with extension

# ── 1. Kill any running instance ──────────────────────────────
Write-Host "[1/3] Stopping any running instance..." -ForegroundColor Cyan
Stop-Process -Name $ExeName -Force -ErrorAction SilentlyContinue
Stop-Process -Name "Auroras BnS Material Tracker" -Force -ErrorAction SilentlyContinue

# ── 2. Publish ────────────────────────────────────────────────
Write-Host "[2/3] Publishing..." -ForegroundColor Cyan
dotnet publish "$Root\BnsMaterialTracker.csproj" `
    -c Release -r win-x64 --no-self-contained `
    -p:PublishSingleFile=true `
    -o "$Root\publish"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed." -ForegroundColor Red
    exit 1
}

# Rename: AssemblyName cannot have apostrophe (breaks WPF pack URIs),
# so we build as "Auroras..." then rename the final exe for the user-facing filename.
$from = "$Root\publish\Auroras BnS Material Tracker.exe"
$to   = "$Root\publish\$ExeFile"
if (Test-Path $from) {
    Remove-Item $to -Force -ErrorAction SilentlyContinue
    Rename-Item -Path $from -NewName $ExeFile
}

# ── 3. Build installer ────────────────────────────────────────
Write-Host "[3/3] Building installer..." -ForegroundColor Cyan

$iscc = @(
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Host "Inno Setup not found. Skipping installer build." -ForegroundColor Yellow
    Write-Host "Download from: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 0
}

New-Item -ItemType Directory -Force "$Root\installer_output" | Out-Null
& $iscc "$Root\installer.iss"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Done!" -ForegroundColor Green
    Write-Host "  EXE      : $Root\publish\$ExeFile"
    Write-Host "  Installer: $Root\installer_output\"
} else {
    Write-Host "Installer build failed." -ForegroundColor Red
    exit 1
}
