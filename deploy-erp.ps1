[CmdletBinding()]
param(
    [string]$SolutionRoot = "",
    [string]$ProjectRelativePath = "Kharbarchi.Server\Kharbarchi.Server.csproj",
    [string]$ClientProjectRelativePath = "Kharbarchi.Client\Kharbarchi.Client.csproj",

    # سرور ERP
    [string]$ErpSshTarget = "khp-erp",

    # اگر alias سرور فروشگاه را داری اینجا بگذار.
    # اگر خالی بماند، اسکریپت از تو می‌پرسد.
    [string]$ShopSshTarget = "",

    [string]$RemoteAppPath = "/srv/kharbarchi/publish",
    [string]$RemoteUiPath = "/srv/kharbarchi/erp-ui",
    [string]$NginxSiteName = "kharbarchi-erp-3030",
    [int]$NginxPort = 3030,
    [string]$ErpPublicUrl = "http://2.179.72.80:3030/",
    [string]$ServiceName = "kharbarchi-api",
    [string]$AppDllName = "Kharbarchi.Server.dll",
    [string]$Runtime = "linux-x64",
    [string]$AspNetCoreUrls = "http://127.0.0.1:5100",
    [string]$HealthUrl = "http://127.0.0.1:5100/health",
    [string]$AspNetCoreEnvironment = "Production",
    [string]$EnvironmentFile = "/etc/kharbarchi-api.env",
    [string]$LinuxServiceUser = "khpadmin",
    [bool]$UpdateSystemdService = $true,
    [switch]$ValidateOnly,

    # سرویس‌های احتمالی فروشگاه؛ اسکریپت هرکدام وجود داشته باشد را start/restart می‌کند
    [string[]]$ShopServicesToStart = @("mysql", "mariadb", "nginx", "apache2", "php8.3-fpm", "php8.2-fpm", "php8.1-fpm")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($SolutionRoot)) {
    $SolutionRoot = $PSScriptRoot
}

function Assert-CommandExists {
    param([string]$CommandName)

    $cmd = Get-Command $CommandName -ErrorAction SilentlyContinue
    if (-not $cmd) {
        throw "Command not found: $CommandName"
    }
}

function Invoke-Native {
    param(
        [string]$Command,
        [string[]]$Arguments
    )

    & $Command @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code ${LASTEXITCODE}: $Command $($Arguments -join ' ')"
    }
}

Assert-CommandExists "dotnet"
Assert-CommandExists "ssh"
Assert-CommandExists "scp"
Assert-CommandExists "tar"
Assert-CommandExists "curl.exe"

$stamp = Get-Date -Format "yyyyMMdd_HHmmss"

$projectPath = Join-Path $SolutionRoot $ProjectRelativePath
$clientProjectPath = Join-Path $SolutionRoot $ClientProjectRelativePath
$deployRoot = Join-Path $SolutionRoot "_deploy"
$publishDir = Join-Path $deployRoot "publish_erp_$stamp"
$uiPublishDir = Join-Path $deployRoot "publish_erp_ui_$stamp"
$uiWwwRoot = Join-Path $uiPublishDir "wwwroot"
$archivePath = Join-Path $deployRoot "kharbarchi_erp_publish_$stamp.tar.gz"
$uiArchivePath = Join-Path $deployRoot "kharbarchi_erp_ui_$stamp.tar.gz"
$localRemoteBashPath = Join-Path $deployRoot "remote_deploy_$stamp.sh"

if (-not (Test-Path $projectPath)) {
    throw "Project file not found: $projectPath"
}

if (-not (Test-Path $clientProjectPath)) {
    throw "ERP UI project file not found: $clientProjectPath"
}

if ([string]::IsNullOrWhiteSpace($ErpSshTarget)) {
    throw "ErpSshTarget cannot be empty."
}

foreach ($sshTarget in @($ErpSshTarget, $ShopSshTarget)) {
    if (-not [string]::IsNullOrWhiteSpace($sshTarget) -and $sshTarget -notmatch '^[A-Za-z0-9_.@-]+$') {
        throw "Unsafe SSH target: $sshTarget"
    }
}

if ($RemoteAppPath -cne "/srv/kharbarchi/publish") {
    throw "RemoteAppPath must be exactly /srv/kharbarchi/publish. Refusing unsafe deployment target: $RemoteAppPath"
}

if ($RemoteUiPath -cne "/srv/kharbarchi/erp-ui") {
    throw "RemoteUiPath must be exactly /srv/kharbarchi/erp-ui. Refusing unsafe UI target: $RemoteUiPath"
}

if ($NginxSiteName -cne "kharbarchi-erp-3030" -or $NginxPort -ne 3030) {
    throw "Nginx site name and port must remain kharbarchi-erp-3030 and 3030."
}

$parsedPublicUrl = $null
$publicUrlIsValid = [Uri]::TryCreate($ErpPublicUrl, [UriKind]::Absolute, [ref]$parsedPublicUrl)
if ((-not $publicUrlIsValid) -or
    $parsedPublicUrl.Scheme -cne "http" -or
    $parsedPublicUrl.Port -ne $NginxPort -or
    $parsedPublicUrl.AbsolutePath -cne "/") {
    throw "ErpPublicUrl must be an absolute HTTP root URL on port $NginxPort."
}

if ($Runtime -cne "linux-x64") {
    throw "Runtime must be exactly linux-x64."
}

if ($AspNetCoreUrls -cne "http://127.0.0.1:5100" -or $HealthUrl -cne "http://127.0.0.1:5100/health") {
    throw "ERP API binding and health URL must remain on 127.0.0.1:5100."
}

if ($ServiceName -cne "kharbarchi-api" -or $AppDllName -cne "Kharbarchi.Server.dll") {
    throw "ServiceName and AppDllName must remain kharbarchi-api and Kharbarchi.Server.dll for this deployment."
}

if ($LinuxServiceUser -notmatch '^[a-z_][a-z0-9_-]*$') {
    throw "Unsafe Linux service user: $LinuxServiceUser"
}

if ($EnvironmentFile -cne "/etc/kharbarchi-api.env") {
    throw "EnvironmentFile must remain /etc/kharbarchi-api.env."
}

if ($AspNetCoreEnvironment -cne "Production") {
    throw "ERP deployment requires AspNetCoreEnvironment=Production."
}

if (-not $UpdateSystemdService) {
    throw "UpdateSystemdService cannot be disabled because the deployment must validate and install the safe systemd unit."
}

foreach ($service in $ShopServicesToStart) {
    if ($service -notmatch '^[A-Za-z0-9_.@-]+$') {
        throw "Unsafe systemd service name: $service"
    }
}

if (-not (Test-Path $deployRoot)) {
    New-Item -Path $deployRoot -ItemType Directory | Out-Null
}

Write-Host ""
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Kharbarchi ERP Deploy"
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Project        : $projectPath"
Write-Host "ERP UI Project : $clientProjectPath"
Write-Host "SSH Target     : $ErpSshTarget"
Write-Host "Remote App Path: $RemoteAppPath"
Write-Host "Remote UI Path : $RemoteUiPath"
Write-Host "Public ERP URL : $ErpPublicUrl"
Write-Host "Service        : $ServiceName"
Write-Host "Runtime        : $Runtime"
Write-Host "Stamp          : $stamp"
Write-Host ""

Write-Host "=== Building and publishing ERP API ===" -ForegroundColor Cyan
Write-Host "Output: $publishDir"
Write-Host ""

Invoke-Native "dotnet" @(
    "publish",
    $projectPath,
    "-c",
    "Release",
    "-r",
    $Runtime,
    "--self-contained",
    "false",
    "-o",
    $publishDir
)

$publishedDll = Join-Path $publishDir $AppDllName

if (-not (Test-Path $publishedDll)) {
    throw "Publish succeeded, but $AppDllName was not found in: $publishDir"
}

Write-Host ""
Write-Host "=== Creating TAR.GZ archive ===" -ForegroundColor Cyan
Write-Host "Archive: $archivePath"
Write-Host ""

if (Test-Path $archivePath) {
    throw "Refusing to overwrite existing deployment archive: $archivePath"
}

Invoke-Native "tar" @(
    "-czf",
    $archivePath,
    "-C",
    $publishDir,
    "."
)

if (-not (Test-Path $archivePath)) {
    throw "Archive file was not created: $archivePath"
}

Write-Host ""
Write-Host "=== Building and publishing ERP UI ===" -ForegroundColor Cyan
Write-Host "Output: $uiPublishDir"
Write-Host ""

Invoke-Native "dotnet" @(
    "publish",
    $clientProjectPath,
    "-c",
    "Release",
    "-o",
    $uiPublishDir
)

$uiIndexPath = Join-Path $uiWwwRoot "index.html"
if (-not (Test-Path $uiIndexPath)) {
    throw "ERP UI publish succeeded, but wwwroot/index.html was not found: $uiIndexPath"
}

$requiredUiJavaScriptRelativePaths = @(
    "assets\js\kharbarchi-lux-admin.js",
    "assets\js\khb-ux-polish.js"
)
foreach ($relativePath in $requiredUiJavaScriptRelativePaths) {
    $publishedAssetPath = Join-Path $uiWwwRoot $relativePath
    if (-not (Test-Path $publishedAssetPath -PathType Leaf)) {
        throw "ERP UI publish is missing required JavaScript asset: $publishedAssetPath"
    }
}

$uiIndexHtml = Get-Content $uiIndexPath -Raw
foreach ($requiredScriptUrl in @("/assets/js/kharbarchi-lux-admin.js", "/assets/js/khb-ux-polish.js")) {
    if ($uiIndexHtml.IndexOf("src=`"$requiredScriptUrl`"", [StringComparison]::Ordinal) -lt 0) {
        throw "ERP UI index.html is missing the exact script reference: $requiredScriptUrl"
    }
}

if ($uiIndexHtml.IndexOf("/assets/js/kharbarchi-lux-admin.js", [StringComparison]::Ordinal) -gt
    $uiIndexHtml.IndexOf("_framework/blazor.webassembly.js", [StringComparison]::Ordinal)) {
    throw "kharbarchi-lux-admin.js must load before Blazor starts."
}

$uiProductionSettingsPath = Join-Path $uiWwwRoot "appsettings.Production.json"
if (-not (Test-Path $uiProductionSettingsPath)) {
    throw "ERP UI publish is missing wwwroot/appsettings.Production.json."
}

$uiProductionSettings = Get-Content $uiProductionSettingsPath -Raw | ConvertFrom-Json
if ($uiProductionSettings.Api.BaseUrl -cne "/") {
    throw "Production ERP UI Api:BaseUrl must be exactly '/' for same-origin Nginx proxying."
}

Write-Host ""
Write-Host "=== Creating ERP UI TAR.GZ archive ===" -ForegroundColor Cyan
Write-Host "Archive: $uiArchivePath"
Write-Host ""

if (Test-Path $uiArchivePath) {
    throw "Refusing to overwrite existing ERP UI deployment archive: $uiArchivePath"
}

Invoke-Native "tar" @(
    "-czf",
    $uiArchivePath,
    "-C",
    $uiWwwRoot,
    "."
)

if (-not (Test-Path $uiArchivePath)) {
    throw "ERP UI archive file was not created: $uiArchivePath"
}

$remoteArchivePath = "/tmp/kharbarchi_erp_publish_$stamp.tar.gz"
$remoteUiArchivePath = "/tmp/kharbarchi_erp_ui_$stamp.tar.gz"
$remoteBashPath = "/tmp/kharbarchi_remote_deploy_$stamp.sh"

$updateSystemdValue = "false"
if ($UpdateSystemdService) {
    $updateSystemdValue = "true"
}

Write-Host ""
Write-Host "=== Creating Linux deploy script ===" -ForegroundColor Cyan
Write-Host "Local bash: $localRemoteBashPath"
Write-Host ""

$remoteScriptTemplate = @'
set -euo pipefail

REMOTE_APP_PATH="__REMOTE_APP_PATH__"
REMOTE_UI_PATH="__REMOTE_UI_PATH__"
SERVICE_NAME="__SERVICE_NAME__"
APP_DLL_NAME="__APP_DLL_NAME__"
ARCHIVE_PATH="__ARCHIVE_PATH__"
UI_ARCHIVE_PATH="__UI_ARCHIVE_PATH__"
NGINX_SITE_NAME="__NGINX_SITE_NAME__"
NGINX_PORT="__NGINX_PORT__"
PUBLIC_ERP_URL="__PUBLIC_ERP_URL__"
STAMP="__STAMP__"
ASPNETCORE_URLS_VALUE="__ASPNETCORE_URLS__"
ASPNETCORE_ENVIRONMENT_VALUE="__ASPNETCORE_ENVIRONMENT__"
HEALTH_URL="__HEALTH_URL__"
ENVIRONMENT_FILE_VALUE="__ENVIRONMENT_FILE__"
LINUX_SERVICE_USER="__LINUX_SERVICE_USER__"
UPDATE_SYSTEMD_SERVICE="__UPDATE_SYSTEMD_SERVICE__"

unit_exists() {
    local service_name="$1"
    systemctl list-unit-files "$service_name.service" --no-legend 2>/dev/null |
        awk '{print $1}' |
        grep -Fxq "$service_name.service"
}

show_service_failure() {
    local service_name="$1"
    echo ""
    echo "Status for failed service: $service_name"
    sudo systemctl --no-pager --full status "$service_name" || true
    echo ""
    echo "Recent logs for failed service: $service_name"
    sudo journalctl -u "$service_name" -n 200 --no-pager || true
}

echo ""
echo "============================================================"
echo " Kharbarchi Remote Deploy"
echo "============================================================"
echo "Remote app path : $REMOTE_APP_PATH"
echo "Remote UI path  : $REMOTE_UI_PATH"
echo "Service         : $SERVICE_NAME"
echo "API archive     : $ARCHIVE_PATH"
echo "UI archive      : $UI_ARCHIVE_PATH"
echo "Public ERP URL  : $PUBLIC_ERP_URL"
echo "Stamp           : $STAMP"
echo ""

# محافظ امنیتی برای جلوگیری از پاک شدن مسیر اشتباه
if [ -z "$REMOTE_APP_PATH" ] || [ "$REMOTE_APP_PATH" = "/" ] || [ "$REMOTE_APP_PATH" = "/srv" ] || [ "$REMOTE_APP_PATH" = "/srv/" ] || [ "$REMOTE_APP_PATH" = "/srv/kharbarchi" ] || [ "$REMOTE_APP_PATH" = "/srv/kharbarchi/" ]; then
    echo "ERROR: Refusing to delete unsafe path: $REMOTE_APP_PATH"
    exit 1
fi

case "$REMOTE_APP_PATH" in
    /srv/kharbarchi/*)
        ;;
    *)
        echo "ERROR: RemoteAppPath must be under /srv/kharbarchi/"
        echo "Current value: $REMOTE_APP_PATH"
        exit 1
        ;;
esac

if [ "$REMOTE_UI_PATH" != "/srv/kharbarchi/erp-ui" ]; then
    echo "ERROR: RemoteUiPath must be exactly /srv/kharbarchi/erp-ui."
    exit 1
fi

if [ ! -f "$ARCHIVE_PATH" ]; then
    echo "ERROR: uploaded archive was not found: $ARCHIVE_PATH"
    exit 1
fi

if [ ! -f "$UI_ARCHIVE_PATH" ]; then
    echo "ERROR: uploaded ERP UI archive was not found: $UI_ARCHIVE_PATH"
    exit 1
fi

if ! command -v tar >/dev/null 2>&1; then
    echo "ERROR: tar is not installed on server."
    exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
    echo "ERROR: curl is required for the API health check."
    exit 1
fi

if ! command -v nginx >/dev/null 2>&1; then
    echo "ERROR: nginx is required to serve the ERP UI on port $NGINX_PORT."
    exit 1
fi

if ! unit_exists nginx; then
    echo "ERROR: nginx.service is not installed."
    exit 1
fi

if ! command -v ss >/dev/null 2>&1; then
    echo "ERROR: ss is required to validate that Kestrel remains localhost-only."
    exit 1
fi

echo ""
echo "Stopping service before configuration validation and deployment..."
if unit_exists "$SERVICE_NAME"; then
    sudo systemctl stop "$SERVICE_NAME"
else
    echo "Service does not exist yet. It will be created if UPDATE_SYSTEMD_SERVICE=true."
fi

if [ ! -f "$ENVIRONMENT_FILE_VALUE" ]; then
    echo "ERROR: required environment file does not exist: $ENVIRONMENT_FILE_VALUE"
    exit 1
fi

for required_key in \
    ConnectionStrings__MySqlConnection \
    Jwt__Key \
    WooCommerce__ConsumerKey \
    WooCommerce__ConsumerSecret \
    Site__PublicUrl \
    Barook__CpgTerminalCode \
    Barook__CpgPassword; do
    if ! sudo grep -Eq "^[[:space:]]*$required_key[[:space:]]*=" "$ENVIRONMENT_FILE_VALUE"; then
        echo "ERROR: $required_key is missing from $ENVIRONMENT_FILE_VALUE"
        exit 1
    fi
done

if ! sudo grep -Eq "^[[:space:]]*WooCommerce__EnvironmentType[[:space:]]*=[[:space:]]*['\"]?Production['\"]?[[:space:]]*$" "$ENVIRONMENT_FILE_VALUE"; then
    echo "ERROR: WooCommerce__EnvironmentType=Production is required in $ENVIRONMENT_FILE_VALUE"
    exit 1
fi

if ! sudo grep -Eq "^[[:space:]]*WooCommerce__BaseUrl[[:space:]]*=[[:space:]]*['\"]?https://www\.Kharbarchi\.ir/['\"]?[[:space:]]*$" "$ENVIRONMENT_FILE_VALUE"; then
    echo "ERROR: WooCommerce__BaseUrl=https://www.Kharbarchi.ir/ is required in $ENVIRONMENT_FILE_VALUE"
    exit 1
fi

if ! sudo grep -Eq "^[[:space:]]*Site__PublicUrl[[:space:]]*=[[:space:]]*['\"]?https://www\.Kharbarchi\.ir/['\"]?[[:space:]]*$" "$ENVIRONMENT_FILE_VALUE"; then
    echo "ERROR: Site__PublicUrl=https://www.Kharbarchi.ir/ is required in $ENVIRONMENT_FILE_VALUE"
    exit 1
fi

if ! sudo grep -Eq "^[[:space:]]*WooCommerce__VerifySsl[[:space:]]*=[[:space:]]*['\"]?true['\"]?[[:space:]]*$" "$ENVIRONMENT_FILE_VALUE"; then
    echo "ERROR: WooCommerce__VerifySsl=true is required in $ENVIRONMENT_FILE_VALUE"
    exit 1
fi

if ! sudo grep -Eq "^[[:space:]]*ConnectionStrings__MySqlConnection[[:space:]]*=.*[Dd]atabase[[:space:]]*=[[:space:]]*Kharbarchi_erp([;'\"]|[[:space:]]*$)" "$ENVIRONMENT_FILE_VALUE"; then
    echo "ERROR: ConnectionStrings__MySqlConnection must select database name exactly Kharbarchi_erp."
    exit 1
fi

echo ""
echo "Starting ERP database service..."
DATABASE_SERVICE=""
for candidate in mysql mariadb; do
    if unit_exists "$candidate"; then
        echo "Found database service: $candidate"
        if ! sudo systemctl start "$candidate"; then
            show_service_failure "$candidate"
            exit 1
        fi
        if ! sudo systemctl is-active --quiet "$candidate"; then
            show_service_failure "$candidate"
            exit 1
        fi
        DATABASE_SERVICE="$candidate"
        echo "ACTIVE: $candidate"
        break
    fi
done

if [ -z "$DATABASE_SERVICE" ]; then
    echo "ERROR: neither mysql.service nor mariadb.service is installed on the ERP server."
    exit 1
fi

echo ""
echo "Deleting old remote app path..."
sudo rm -rf "$REMOTE_APP_PATH"

echo ""
echo "Creating clean remote app path..."
sudo mkdir -p "$REMOTE_APP_PATH"
sudo chown -R "$LINUX_SERVICE_USER:$LINUX_SERVICE_USER" "$REMOTE_APP_PATH"

echo ""
echo "Extracting new archive..."
tar -xzf "$ARCHIVE_PATH" -C "$REMOTE_APP_PATH"

if [ ! -f "$REMOTE_APP_PATH/$APP_DLL_NAME" ]; then
    echo "ERROR: $APP_DLL_NAME was not found after extraction."
    echo "Remote app path content:"
    ls -lah "$REMOTE_APP_PATH" || true
    exit 1
fi

sudo chown -R "$LINUX_SERVICE_USER:$LINUX_SERVICE_USER" "$REMOTE_APP_PATH"

echo ""
echo "Listing deployed files:"
ls -lah "$REMOTE_APP_PATH"

echo ""
echo "Deploying ERP UI static files..."
sudo mkdir -p "$REMOTE_UI_PATH"
sudo chown -R "$LINUX_SERVICE_USER:$LINUX_SERVICE_USER" "$REMOTE_UI_PATH"
tar -xzf "$UI_ARCHIVE_PATH" -C "$REMOTE_UI_PATH"

if [ ! -f "$REMOTE_UI_PATH/index.html" ]; then
    echo "ERROR: ERP UI index.html was not found after extraction."
    echo "Remote UI path content:"
    ls -lah "$REMOTE_UI_PATH" || true
    exit 1
fi

for required_asset in \
    "assets/js/kharbarchi-lux-admin.js" \
    "assets/js/khb-ux-polish.js"; do
    if [ ! -f "$REMOTE_UI_PATH/$required_asset" ]; then
        echo "ERROR: required ERP UI JavaScript asset is missing: $REMOTE_UI_PATH/$required_asset"
        find "$REMOTE_UI_PATH" -type f \( -name 'kharbarchi-lux-admin.js' -o -name 'khb-ux-polish.js' \) -print || true
        exit 1
    fi
done

echo "Required ERP UI JavaScript assets:"
find "$REMOTE_UI_PATH" -type f \( -name 'kharbarchi-lux-admin.js' -o -name 'khb-ux-polish.js' \) -print

sudo chown -R "$LINUX_SERVICE_USER:$LINUX_SERVICE_USER" "$REMOTE_UI_PATH"

echo ""
echo "Writing Nginx ERP site..."
NGINX_SITE_AVAILABLE="/etc/nginx/sites-available/$NGINX_SITE_NAME"
NGINX_SITE_ENABLED="/etc/nginx/sites-enabled/$NGINX_SITE_NAME"
NGINX_SITE_CANDIDATE="/tmp/$NGINX_SITE_NAME.$STAMP.conf"
NGINX_TEST_CONFIG="/tmp/$NGINX_SITE_NAME.$STAMP.nginx.conf"
NGINX_SITE_BACKUP="$NGINX_SITE_AVAILABLE.before-$STAMP"

sudo tee "$NGINX_SITE_CANDIDATE" >/dev/null <<'NGINX_EOF'
server {
    listen __NGINX_PORT__;
    server_name _;

    root __REMOTE_UI_PATH__;
    index index.html;

    client_max_body_size 100M;

    location ^~ /_framework/ {
        add_header Blazor-Environment Production always;
        try_files $uri =404;
    }

    location ~* \.(?:js|mjs|css|json|wasm|dll|dat|pdb|map|br|gz|ico|png|jpg|jpeg|svg|webp|woff|woff2|ttf)$ {
        try_files $uri =404;
    }

    location = /health {
        proxy_pass http://127.0.0.1:5100/health;
        proxy_http_version 1.1;
        proxy_set_header Host www.Kharbarchi.ir;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api/ {
        proxy_pass http://127.0.0.1:5100/api/;
        proxy_http_version 1.1;
        proxy_set_header Host www.Kharbarchi.ir;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_read_timeout 300;
        proxy_connect_timeout 300;
        proxy_send_timeout 300;
    }

    location ^~ /swagger {
        return 404;
    }

    location / {
        try_files $uri $uri/ /index.html;
    }
}
NGINX_EOF

sudo tee "$NGINX_TEST_CONFIG" >/dev/null <<EOF
pid /tmp/nginx-$STAMP.pid;
error_log stderr;
events {
    worker_connections 64;
}
http {
    include /etc/nginx/mime.types;
    access_log off;
    include $NGINX_SITE_CANDIDATE;
}
EOF

echo ""
echo "Validating candidate Nginx site before installation..."
if ! sudo nginx -t -c "$NGINX_TEST_CONFIG"; then
    echo "ERROR: Candidate Nginx configuration validation failed. The live site was not changed."
    exit 1
fi

NGINX_PORT_CONFLICTS="$(
    sudo grep -RIlE '^[[:space:]]*listen[[:space:]]+3030([[:space:];]|$)' /etc/nginx/sites-enabled 2>/dev/null |
        grep -Fvx "$NGINX_SITE_ENABLED" || true
)"
if [ -n "$NGINX_PORT_CONFLICTS" ]; then
    echo "ERROR: Another enabled Nginx site also listens on port 3030:"
    printf '%s\n' "$NGINX_PORT_CONFLICTS"
    exit 1
fi

if [ -f "$NGINX_SITE_AVAILABLE" ]; then
    sudo cp -a "$NGINX_SITE_AVAILABLE" "$NGINX_SITE_BACKUP"
fi

sudo cp "$NGINX_SITE_CANDIDATE" "$NGINX_SITE_AVAILABLE"
sudo ln -sfn "$NGINX_SITE_AVAILABLE" "$NGINX_SITE_ENABLED"

if ! sudo nginx -t; then
    echo "ERROR: Installed Nginx configuration validation failed."
    if [ -f "$NGINX_SITE_BACKUP" ]; then
        echo "Restoring previous Nginx site file."
        sudo cp -a "$NGINX_SITE_BACKUP" "$NGINX_SITE_AVAILABLE"
        sudo nginx -t || true
    fi
    exit 1
fi

echo ""
echo "Installed Nginx site:"
sudo sed -n '1,240p' "$NGINX_SITE_AVAILABLE"

if [ "$UPDATE_SYSTEMD_SERVICE" = "true" ]; then
    echo ""
    echo "Writing systemd service file..."

    SERVICE_FILE="/etc/systemd/system/$SERVICE_NAME.service"

    sudo tee "$SERVICE_FILE" >/dev/null <<EOF
[Unit]
Description=Kharbarchi ASP.NET Core API
Wants=network-online.target
After=network-online.target mysql.service mariadb.service
StartLimitIntervalSec=120
StartLimitBurst=3

[Service]
EnvironmentFile=$ENVIRONMENT_FILE_VALUE
WorkingDirectory=$REMOTE_APP_PATH
ExecStart=/usr/bin/dotnet /srv/kharbarchi/publish/Kharbarchi.Server.dll
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=$SERVICE_NAME
User=$LINUX_SERVICE_USER
Environment=ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT_VALUE
Environment=DOTNET_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT_VALUE
Environment=ASPNETCORE_URLS=$ASPNETCORE_URLS_VALUE

[Install]
WantedBy=multi-user.target
EOF

    EXPECTED_EXEC_START="ExecStart=/usr/bin/dotnet /srv/kharbarchi/publish/Kharbarchi.Server.dll"
    if sudo grep -Fq "dotnet-" "$SERVICE_FILE"; then
        echo "ERROR: malformed systemd ExecStart contains 'dotnet-'."
        sudo systemctl cat "$SERVICE_NAME" || sudo sed -n '1,200p' "$SERVICE_FILE"
        exit 1
    fi

    if ! sudo grep -Fxq "$EXPECTED_EXEC_START" "$SERVICE_FILE"; then
        echo "ERROR: systemd ExecStart is not exactly: $EXPECTED_EXEC_START"
        sudo systemctl cat "$SERVICE_NAME" || sudo sed -n '1,200p' "$SERVICE_FILE"
        exit 1
    fi

    echo ""
    echo "Generated systemd unit:"
    sudo systemctl cat "$SERVICE_NAME" || sudo sed -n '1,200p' "$SERVICE_FILE"

    if command -v systemd-analyze >/dev/null 2>&1; then
        if ! sudo systemd-analyze verify "$SERVICE_FILE"; then
            echo "WARNING: systemd-analyze reported a dependency warning. Exact ExecStart validation still passed."
        fi
    fi

    sudo systemctl daemon-reload
    sudo systemctl enable "$SERVICE_NAME"
else
    echo ""
    echo "Skipping systemd service update because UPDATE_SYSTEMD_SERVICE=false."
    sudo systemctl daemon-reload
fi

echo ""
echo "Starting service..."
sudo systemctl reset-failed "$SERVICE_NAME" || true
sudo systemctl start "$SERVICE_NAME"

echo ""
echo "Waiting for API activity and health..."
STARTED=0
for attempt in $(seq 1 12); do
    if sudo systemctl is-active --quiet "$SERVICE_NAME" &&
       curl --fail --silent --show-error --max-time 5 "$HEALTH_URL" >/dev/null; then
        STARTED=1
        break
    fi
    sleep 5
done

echo ""
echo "Checking service status..."
sudo systemctl --no-pager --full status "$SERVICE_NAME" || true

if [ "$STARTED" != "1" ]; then
    echo "ERROR: $SERVICE_NAME did not remain active and pass health check at $HEALTH_URL."
    show_service_failure "$SERVICE_NAME"
    sudo systemctl stop "$SERVICE_NAME" || true
    exit 1
fi

echo "SUCCESS: $SERVICE_NAME is active and the health endpoint passed."

echo ""
echo "Starting or reloading Nginx..."
sudo systemctl enable nginx
if sudo systemctl is-active --quiet nginx; then
    sudo systemctl reload nginx
else
    sudo systemctl start nginx
fi

if ! sudo systemctl is-active --quiet nginx; then
    show_service_failure nginx
    exit 1
fi

echo ""
echo "Validating Kestrel remains private..."
if ! ss -lntH | awk '{print $4}' | grep -Fxq "127.0.0.1:5100"; then
    echo "ERROR: Kestrel is not listening on the required internal endpoint 127.0.0.1:5100."
    ss -lnt || true
    exit 1
fi

if ss -lntH | awk '{print $4}' | grep -Eq '^(0\.0\.0\.0|\[::\]|\*):5100$'; then
    echo "ERROR: Kestrel port 5100 is publicly bound. Refusing successful deployment."
    ss -lnt || true
    exit 1
fi

echo "SUCCESS: Kestrel is listening only on 127.0.0.1:5100."

echo ""
echo "Validating ERP UI through Nginx..."
curl --fail --silent --show-error --head --max-time 15 "http://127.0.0.1:$NGINX_PORT/" >/dev/null
curl --fail --silent --show-error --head --max-time 15 "http://127.0.0.1:$NGINX_PORT/index.html" >/dev/null

for asset_url in \
    "/assets/js/kharbarchi-lux-admin.js" \
    "/assets/js/khb-ux-polish.js"; do
    ASSET_RESULT="$(
        curl --silent --show-error --head --output /dev/null \
            --write-out '%{http_code} %{content_type}' \
            --max-time 15 \
            "http://127.0.0.1:$NGINX_PORT$asset_url"
    )"
    case "$ASSET_RESULT" in
        "200 application/javascript"*|"200 text/javascript"*|"200 application/x-javascript"*)
            echo "VALID JS: $asset_url ($ASSET_RESULT)"
            ;;
        *)
            echo "ERROR: $asset_url did not return JavaScript. Result: $ASSET_RESULT"
            exit 1
            ;;
    esac
done

ROOT_LEVEL_JS_STATUS="$(
    curl --silent --output /dev/null --write-out '%{http_code}' --head --max-time 10 \
        "http://127.0.0.1:$NGINX_PORT/kharbarchi-lux-admin.js"
)"
if [ "$ROOT_LEVEL_JS_STATUS" != "404" ]; then
    echo "ERROR: missing root-level JavaScript must return 404, but returned $ROOT_LEVEL_JS_STATUS."
    exit 1
fi

LEGACY_JS_STATUS="$(
    curl --silent --output /dev/null --write-out '%{http_code}' --head --max-time 10 \
        "http://127.0.0.1:$NGINX_PORT/js/kharbarchi-lux-admin.js"
)"
if [ "$LEGACY_JS_STATUS" != "404" ]; then
    echo "ERROR: obsolete /js asset URL must return 404, but returned $LEGACY_JS_STATUS."
    exit 1
fi

MISSING_STATIC_STATUS="$(
    curl --silent --output /dev/null --write-out '%{http_code}' --head --max-time 10 \
        "http://127.0.0.1:$NGINX_PORT/assets/js/__khb_missing_static_validation__.js"
)"
if [ "$MISSING_STATIC_STATUS" != "404" ]; then
    echo "ERROR: missing static assets must return 404, but returned $MISSING_STATIC_STATUS."
    exit 1
fi

if ! UI_HTML="$(curl --fail --silent --show-error --max-time 15 "http://127.0.0.1:$NGINX_PORT/")"; then
    show_service_failure nginx
    exit 1
fi

if ! printf '%s' "$UI_HTML" | grep -Eqi '<!doctype html|<html'; then
    echo "ERROR: Nginx root did not return ERP UI HTML."
    exit 1
fi

NGINX_HEALTH="$(curl --fail --silent --show-error --max-time 10 "http://127.0.0.1:$NGINX_PORT/health")"
if [ "$NGINX_HEALTH" != "Healthy" ]; then
    echo "ERROR: Nginx health proxy did not return Healthy."
    exit 1
fi

SWAGGER_STATUS="$(curl --silent --output /dev/null --write-out '%{http_code}' --max-time 10 "http://127.0.0.1:$NGINX_PORT/swagger/index.html")"
if [ "$SWAGGER_STATUS" != "404" ]; then
    echo "ERROR: Public Swagger route must return 404, but returned $SWAGGER_STATUS."
    exit 1
fi

echo "SUCCESS: ERP UI HTML, proxied health, and Swagger blocking checks passed."

echo ""
echo "Last logs:"
sudo journalctl -u "$SERVICE_NAME" -n 80 --no-pager || true

echo ""
echo "Final ERP service status:"
echo "ACTIVE: $DATABASE_SERVICE"
echo "ACTIVE: $SERVICE_NAME"
echo "ACTIVE: nginx"
echo "UI INDEX: $REMOTE_UI_PATH/index.html"
echo "PUBLIC ERP: $PUBLIC_ERP_URL"

echo ""
echo "Remote deploy finished successfully."
'@

$remoteScript = $remoteScriptTemplate `
    -replace "__REMOTE_APP_PATH__", $RemoteAppPath `
    -replace "__REMOTE_UI_PATH__", $RemoteUiPath `
    -replace "__SERVICE_NAME__", $ServiceName `
    -replace "__APP_DLL_NAME__", $AppDllName `
    -replace "__ARCHIVE_PATH__", $remoteArchivePath `
    -replace "__UI_ARCHIVE_PATH__", $remoteUiArchivePath `
    -replace "__NGINX_SITE_NAME__", $NginxSiteName `
    -replace "__NGINX_PORT__", $NginxPort `
    -replace "__PUBLIC_ERP_URL__", $ErpPublicUrl `
    -replace "__STAMP__", $stamp `
    -replace "__ASPNETCORE_URLS__", $AspNetCoreUrls `
    -replace "__ASPNETCORE_ENVIRONMENT__", $AspNetCoreEnvironment `
    -replace "__HEALTH_URL__", $HealthUrl `
    -replace "__ENVIRONMENT_FILE__", $EnvironmentFile `
    -replace "__LINUX_SERVICE_USER__", $LinuxServiceUser `
    -replace "__UPDATE_SYSTEMD_SERVICE__", $updateSystemdValue

# خیلی مهم: پایان خط لینوکسی LF
$remoteScriptLinux = $remoteScript -replace "`r`n", "`n" -replace "`r", "`n"

if ($remoteScriptLinux -match "(?m)^ExecStart=.*dotnet-") {
    throw "Generated Bash contains malformed systemd text 'dotnet-'."
}

$expectedExecStart = "ExecStart=/usr/bin/dotnet /srv/kharbarchi/publish/Kharbarchi.Server.dll"
if ($remoteScriptLinux -notmatch "(?m)^$([Regex]::Escape($expectedExecStart))$") {
    throw "Generated Bash does not contain the exact required ExecStart: $expectedExecStart"
}

$expectedNginxRoot = "    root /srv/kharbarchi/erp-ui;"
if ($remoteScriptLinux -notmatch "(?m)^$([Regex]::Escape($expectedNginxRoot))$") {
    throw "Generated Bash does not contain the required Nginx ERP UI root."
}

if ($remoteScriptLinux -notmatch "(?m)^    listen 3030;$") {
    throw "Generated Bash does not contain the required Nginx listen port 3030."
}

$expectedSpaFallback = "        try_files `$uri `$uri/ /index.html;"
if ($remoteScriptLinux -notmatch "(?m)^$([Regex]::Escape($expectedSpaFallback))$") {
    throw "Generated Bash does not contain the required SPA fallback."
}

$expectedStaticAssetLocation = "    location ~* \.(?:js|mjs|css|json|wasm|dll|dat|pdb|map|br|gz|ico|png|jpg|jpeg|svg|webp|woff|woff2|ttf)$ {"
if ($remoteScriptLinux -notmatch "(?m)^$([Regex]::Escape($expectedStaticAssetLocation))$") {
    throw "Generated Bash does not contain the required static-asset 404 location."
}

foreach ($requiredAsset in @(
    'assets/js/kharbarchi-lux-admin.js',
    'assets/js/khb-ux-polish.js'
)) {
    if ($remoteScriptLinux.IndexOf($requiredAsset, [StringComparison]::Ordinal) -lt 0) {
        throw "Generated Bash does not validate required ERP UI asset: $requiredAsset"
    }
}

if ($remoteScriptLinux -match "(?m)^\s*proxy_pass\s+http://(0\.0\.0\.0|2\.179\.72\.80):5100") {
    throw "Generated Nginx configuration attempts to proxy to a public Kestrel address."
}

if ($remoteScriptLinux.IndexOf("`r", [StringComparison]::Ordinal) -ge 0) {
    throw "Generated Bash contains CR characters; LF-only output is required."
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($localRemoteBashPath, $remoteScriptLinux, $utf8NoBom)

if ($ValidateOnly) {
    Write-Host ""
    Write-Host "VALIDATION SUCCEEDED: API/UI publish, TAR.GZ archives, LF-only Bash, exact systemd, and Nginx checks passed." -ForegroundColor Green
    Write-Host "No files were uploaded and no remote services were changed."
    return
}

Write-Host ""
Write-Host "=== Uploading archive to ERP server ===" -ForegroundColor Cyan
Write-Host "Remote archive: ${ErpSshTarget}:$remoteArchivePath"
Write-Host ""

Invoke-Native "scp" @(
    $archivePath,
    "${ErpSshTarget}:$remoteArchivePath"
)

Write-Host ""
Write-Host "=== Uploading ERP UI archive to ERP server ===" -ForegroundColor Cyan
Write-Host "Remote UI archive: ${ErpSshTarget}:$remoteUiArchivePath"
Write-Host ""

Invoke-Native "scp" @(
    $uiArchivePath,
    "${ErpSshTarget}:$remoteUiArchivePath"
)

Write-Host ""
Write-Host "=== Uploading deploy script to ERP server ===" -ForegroundColor Cyan
Write-Host "Remote bash: ${ErpSshTarget}:$remoteBashPath"
Write-Host ""

Invoke-Native "scp" @(
    $localRemoteBashPath,
    "${ErpSshTarget}:$remoteBashPath"
)

Write-Host ""
Write-Host "=== Deploying on ERP server ===" -ForegroundColor Cyan
Write-Host "If sudo asks for password, enter the Linux password for user: $LinuxServiceUser"
Write-Host ""

# -tt باعث می‌شود sudo بتواند پسورد بپرسد
Invoke-Native "ssh" @(
    "-tt",
    $ErpSshTarget,
    "chmod +x '$remoteBashPath' && bash -n '$remoteBashPath' && bash '$remoteBashPath'"
)

Write-Host ""
Write-Host "=== Validating public ERP UI from Windows ===" -ForegroundColor Cyan
Invoke-Native "curl.exe" @(
    "--fail",
    "--silent",
    "--show-error",
    "--head",
    "--max-time",
    "15",
    $ErpPublicUrl
)

function Invoke-RemoteServiceStartup {
    param(
        [string]$TargetName,
        [string]$SshTarget,
        [string[]]$Services
    )

    if ([string]::IsNullOrWhiteSpace($SshTarget)) {
        Write-Host ""
        Write-Host "Skipping $TargetName service startup because no SSH target was provided." -ForegroundColor Yellow
        return
    }

    $servicesJoined = $Services -join " "

    $remoteStartupScriptTemplate = @'
set -euo pipefail

TARGET_NAME="__TARGET_NAME__"
SERVICES="__SERVICES__"

unit_exists() {
    local service_name="$1"
    systemctl list-unit-files "$service_name.service" --no-legend 2>/dev/null |
        awk '{print $1}' |
        grep -Fxq "$service_name.service"
}

echo ""
echo "============================================================"
echo " Starting services on $TARGET_NAME"
echo "============================================================"
echo "SSH target services: $SERVICES"
echo ""

FAILED=0
EXISTING=0

for svc in $SERVICES; do
    if unit_exists "$svc"; then
        EXISTING=$((EXISTING + 1))
        echo ""
        echo "Restarting service: $svc"
        if ! sudo systemctl restart "$svc"; then
            echo "RESTART FAILED: $svc"
            FAILED=1
        fi
    else
        echo "Skipping missing service: $svc"
    fi
done

echo ""
echo "Checking service status..."

for svc in $SERVICES; do
    if unit_exists "$svc"; then
        if sudo systemctl is-active --quiet "$svc"; then
            echo "ACTIVE: $svc"
        else
            echo "NOT ACTIVE: $svc"
            sudo systemctl --no-pager --full status "$svc" || true
            sudo journalctl -u "$svc" -n 120 --no-pager || true
            FAILED=1
        fi
    fi
done

if [ "$EXISTING" = "0" ]; then
    echo "WARNING: no recognized services are installed on $TARGET_NAME."
fi

if [ "$FAILED" = "1" ]; then
    echo ""
    echo "ERROR: Some services on $TARGET_NAME are not active."
    exit 1
fi

echo ""
echo "SUCCESS: Services on $TARGET_NAME are active."
'@

    $remoteStartupScript = $remoteStartupScriptTemplate `
        -replace "__TARGET_NAME__", $TargetName `
        -replace "__SERVICES__", $servicesJoined

    $remoteStartupScriptLinux = $remoteStartupScript -replace "`r`n", "`n" -replace "`r", "`n"
    $startupScriptLocalPath = Join-Path $deployRoot "start_services_$TargetName`_$stamp.sh"
    $startupScriptRemotePath = "/tmp/start_services_$TargetName`_$stamp.sh"

    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($startupScriptLocalPath, $remoteStartupScriptLinux, $utf8NoBom)

    Write-Host ""
    Write-Host "=== Uploading startup script for $TargetName ===" -ForegroundColor Cyan
    Invoke-Native "scp" @(
        $startupScriptLocalPath,
        "${SshTarget}:$startupScriptRemotePath"
    )

    Write-Host ""
    Write-Host "=== Starting services on $TargetName ===" -ForegroundColor Cyan
    Write-Host "If sudo asks for password, enter the Linux password for that server user."
    Write-Host ""

    Invoke-Native "ssh" @(
        "-tt",
        $SshTarget,
        "chmod +x '$startupScriptRemotePath' && bash -n '$startupScriptRemotePath' && bash '$startupScriptRemotePath'"
    )
}

Write-Host ""
Write-Host "=== ERP database and API services were started and verified by the deploy step ===" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== Starting Shop server services ===" -ForegroundColor Cyan
Invoke-RemoteServiceStartup `
    -TargetName "SHOP" `
    -SshTarget $ShopSshTarget `
    -Services $ShopServicesToStart
Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host " DEPLOY FINISHED SUCCESSFULLY"
Write-Host "============================================================" -ForegroundColor Green
Write-Host "Remote app path: $RemoteAppPath"
Write-Host "Remote UI path : $RemoteUiPath"
Write-Host "Public ERP URL : $ErpPublicUrl"
Write-Host "Service        : $ServiceName"
Write-Host ""
Write-Host "Check service:"
Write-Host "ssh $ErpSshTarget `"sudo systemctl status $ServiceName --no-pager`""
Write-Host ""
Write-Host "Check logs:"
Write-Host "ssh $ErpSshTarget `"sudo journalctl -u $ServiceName -n 150 --no-pager`""
