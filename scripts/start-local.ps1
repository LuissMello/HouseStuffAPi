[CmdletBinding()]
param([switch]$SkipInstall)

$ErrorActionPreference = "Stop"
$apiRoot = Split-Path $PSScriptRoot -Parent
$workspaceRoot = Split-Path $apiRoot -Parent
$frontRoot = Join-Path $workspaceRoot "HouseStuffFront"
$runtimeDir = Join-Path $apiRoot ".local"
$processFile = Join-Path $runtimeDir "processes.json"

function Resolve-DotNet {
    $localSdk = Join-Path $workspaceRoot ".dotnet-sdk-10\dotnet.exe"
    if (Test-Path -LiteralPath $localSdk) { return $localSdk }
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    throw ".NET 10 não foi encontrado. Instale o SDK indicado pelo global.json."
}

function Test-Endpoint([string]$Uri) {
    try {
        $response = Invoke-WebRequest -Uri $Uri -TimeoutSec 2
        return $response.StatusCode -ge 200 -and $response.StatusCode -lt 400
    }
    catch { return $false }
}

function Wait-Endpoint([string]$Uri, [string]$Name, [int]$Seconds = 60) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    while ((Get-Date) -lt $deadline) {
        if (Test-Endpoint $Uri) { return }
        Start-Sleep -Seconds 1
    }
    throw "$Name não ficou pronto em $Seconds segundos. Consulte os logs em $runtimeDir."
}

if (-not (Test-Path -LiteralPath $frontRoot)) { throw "HouseStuffFront precisa estar ao lado de HouseStuffAPi." }
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { throw "Docker não foi encontrado." }
if (-not (Get-Command npm.cmd -ErrorAction SilentlyContinue)) { throw "Node.js/npm não foi encontrado." }

$dotnet = Resolve-DotNet
New-Item -ItemType Directory -Path $runtimeDir -Force | Out-Null

Push-Location $apiRoot
try {
    docker compose up -d postgres
    if ($LASTEXITCODE -ne 0) { throw "Não foi possível iniciar o PostgreSQL." }
    if (-not $SkipInstall) {
        & $dotnet restore HouseStuff.slnx
        if ($LASTEXITCODE -ne 0) { throw "Falha ao restaurar o backend." }
        if (-not (Test-Path -LiteralPath (Join-Path $frontRoot "node_modules"))) {
            Push-Location $frontRoot
            try { npm.cmd ci } finally { Pop-Location }
            if ($LASTEXITCODE -ne 0) { throw "Falha ao instalar o frontend." }
        }
    }
}
finally { Pop-Location }

$existing = if (Test-Path -LiteralPath $processFile) {
    Get-Content -Raw -LiteralPath $processFile | ConvertFrom-Json
}
else { $null }
$registered = [ordered]@{
    startedAt = (Get-Date).ToUniversalTime().ToString("O")
    api = if ($null -ne $existing) { $existing.api } else { $null }
    frontend = if ($null -ne $existing) { $existing.frontend } else { $null }
}
if (-not (Test-Endpoint "http://localhost:5049/health/ready")) {
    $api = Start-Process -FilePath $dotnet -ArgumentList @("run", "--project", "src\HouseStuff.Api\HouseStuff.Api.csproj", "--no-restore", "--configuration", "Release") -WorkingDirectory $apiRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtimeDir "api.stdout.log") -RedirectStandardError (Join-Path $runtimeDir "api.stderr.log")
    $registered.api = [ordered]@{ pid = $api.Id; startedAt = $api.StartTime.ToUniversalTime().ToString("O") }
}

if (-not (Test-Endpoint "http://localhost:3000")) {
    $frontend = Start-Process -FilePath "npm.cmd" -ArgumentList @("run", "dev") -WorkingDirectory $frontRoot -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtimeDir "frontend.stdout.log") -RedirectStandardError (Join-Path $runtimeDir "frontend.stderr.log")
    $registered.frontend = [ordered]@{ pid = $frontend.Id; startedAt = $frontend.StartTime.ToUniversalTime().ToString("O") }
}

$registered | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath $processFile
Wait-Endpoint "http://localhost:5049/health/ready" "API"
Wait-Endpoint "http://localhost:3000" "Frontend"

Write-Host "HouseStuff pronto."
Write-Host "Aplicação: http://localhost:3000"
Write-Host "API:       http://localhost:5049/health/ready"
Write-Host "Acesso:    use a conta configurada nos User Secrets locais."
