[CmdletBinding()]
param(
    [switch]$OpenBrowsers
)

$SolutionRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $SolutionRoot

function Start-ProjectWindow([string]$Title, [string]$ProjectPath) {
    $cmd = "cd `"$SolutionRoot`"; dotnet run --project `"$ProjectPath`""
    Start-Process powershell -ArgumentList '-NoExit', '-Command', $cmd -WindowStyle Normal | Out-Null
    Write-Host "Started $Title" -ForegroundColor Green
}

Write-Host "Starting Kharbarchi.Server first..." -ForegroundColor Cyan
Start-ProjectWindow 'Server API' '.\Kharbarchi.Server\Kharbarchi.Server.csproj'
Start-Sleep -Seconds 8

Write-Host "Starting Admin Client..." -ForegroundColor Cyan
Start-ProjectWindow 'Admin Client' '.\Kharbarchi.Client\Kharbarchi.Client.csproj'
Start-Sleep -Seconds 3

Write-Host "Starting Store Client..." -ForegroundColor Cyan
Start-ProjectWindow 'Store Client' '.\Kharbarchi.Store\Kharbarchi.Store.csproj'
Start-Sleep -Seconds 3

if ($OpenBrowsers) {
    Start-Process 'https://localhost:7100/swagger'
    Start-Process 'https://localhost:3030/local-admin'
    Start-Process 'https://localhost:3131'
}

Write-Host ""
Write-Host "Run order:" -ForegroundColor Yellow
Write-Host "1) Server API  : https://localhost:7100"
Write-Host "2) ERP         : https://localhost:3030/local-admin"
Write-Host "3) Shop        : https://localhost:3131"
