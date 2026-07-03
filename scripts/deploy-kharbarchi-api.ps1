[CmdletBinding()]
param(
    [string]$SshHost = "khp-erp",
    [string]$RemoteRoot = "/srv/kharbarchi",
    [string]$ServiceName = "kharbarchi-api.service",
    [string]$HealthUrl = "http://127.0.0.1:5100/health"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $projectRoot "Kharbarchi.Server\Kharbarchi.Server.csproj"
$releaseId = [DateTime]::UtcNow.ToString("yyyyMMdd-HHmmssZ")
$localPublish = Join-Path $env:TEMP "kharbarchi-api-$releaseId"
$localArchive = "$localPublish.tar.gz"
$remoteStage = "$RemoteRoot/incoming/$releaseId"
$remotePublish = "$RemoteRoot/publish"
$remoteArchive = "$remoteStage/kharbarchi-api.tar.gz"

if ($RemoteRoot -ne "/srv/kharbarchi") {
    throw "RemoteRoot must remain /srv/kharbarchi unless the server service definition is intentionally changed first."
}

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory)]
        [string]$Command,

        [Parameter(Mandatory)]
        [string[]]$Arguments
    )

    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Command failed with exit code $LASTEXITCODE."
    }
}

Write-Host "Publishing API release $releaseId..." -ForegroundColor Cyan
Invoke-NativeCommand "dotnet" @(
    "publish",
    $projectPath,
    "-c",
    "Release",
    "--no-restore",
    "-o",
    $localPublish
)

Write-Host "Creating release archive..." -ForegroundColor Cyan
Invoke-NativeCommand "tar" @(
    "-czf",
    $localArchive,
    "-C",
    $localPublish,
    "."
)

Write-Host "Creating isolated server staging directory..." -ForegroundColor Cyan
Invoke-NativeCommand "ssh" @(
    $SshHost,
    "mkdir",
    "-p",
    "$remoteStage/api"
)

Write-Host "Uploading release..." -ForegroundColor Cyan
Invoke-NativeCommand "scp" @(
    $localArchive,
    "${SshHost}:$remoteArchive"
)

Write-Host "Extracting release..." -ForegroundColor Cyan
Invoke-NativeCommand "ssh" @(
    $SshHost,
    "tar",
    "-xzf",
    $remoteArchive,
    "-C",
    "$remoteStage/api"
)

Write-Host "Synchronizing the publish directory and removing stale publish files..." -ForegroundColor Yellow
Invoke-NativeCommand "ssh" @(
    $SshHost,
    "rsync",
    "-a",
    "--delete",
    "$remoteStage/api/",
    "$remotePublish/"
)

Write-Host "Removing temporary server staging files..." -ForegroundColor Cyan
Invoke-NativeCommand "ssh" @(
    $SshHost,
    "rm",
    "-rf",
    "--",
    $remoteStage
)

Write-Host "Restarting $ServiceName. Enter the server sudo password if requested..." -ForegroundColor Yellow
Invoke-NativeCommand "ssh" @(
    "-t",
    $SshHost,
    "sudo",
    "systemctl",
    "restart",
    $ServiceName
)

Write-Host "Verifying service status..." -ForegroundColor Cyan
Invoke-NativeCommand "ssh" @(
    $SshHost,
    "systemctl",
    "status",
    $ServiceName,
    "--no-pager"
)

Write-Host "Verifying health endpoint..." -ForegroundColor Cyan
Invoke-NativeCommand "ssh" @(
    $SshHost,
    "curl",
    "-fsS",
    $HealthUrl
)

Write-Host "Deployment completed successfully." -ForegroundColor Green
Write-Host "Production settings remain in /etc/kharbarchi-api.env and are never copied or deleted by this script."
