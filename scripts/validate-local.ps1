[CmdletBinding()]
param([switch]$SkipStart)

$ErrorActionPreference = "Stop"
$apiRoot = Split-Path $PSScriptRoot -Parent
$workspaceRoot = Split-Path $apiRoot -Parent
$frontRoot = Join-Path $workspaceRoot "HouseStuffFront"
$localSdk = Join-Path $workspaceRoot ".dotnet-sdk-10\dotnet.exe"
$dotnet = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { (Get-Command dotnet -ErrorAction Stop).Source }

function Assert-Exit([int]$ExitCode, [string]$Step) {
    if ($ExitCode -ne 0) { throw "$Step falhou." }
}

Push-Location $apiRoot
try {
    & $dotnet format HouseStuff.slnx --verify-no-changes --no-restore
    Assert-Exit $LASTEXITCODE "Formatação do backend"
    & $dotnet build HouseStuff.slnx --configuration Release --no-restore
    Assert-Exit $LASTEXITCODE "Build do backend"
    & $dotnet test HouseStuff.slnx --configuration Release --no-build --no-restore
    Assert-Exit $LASTEXITCODE "Testes do backend"
}
finally { Pop-Location }

Push-Location $frontRoot
try {
    npm.cmd run lint
    Assert-Exit $LASTEXITCODE "Lint do frontend"
    npm.cmd test
    Assert-Exit $LASTEXITCODE "Testes do frontend"
}
finally { Pop-Location }

if (-not $SkipStart) {
    & (Join-Path $PSScriptRoot "start-local.ps1") -SkipInstall
}

$ready = Invoke-WebRequest -Uri "http://localhost:5049/health/ready" -TimeoutSec 5
if ($ready.StatusCode -ne 200) { throw "Readiness da API falhou." }
$site = Invoke-WebRequest -Uri "http://localhost:3000" -TimeoutSec 5
if ($site.StatusCode -ne 200) { throw "Frontend não respondeu." }

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$login = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/auth/login" -Method Post -WebSession $session -ContentType "application/json" -Body '{"email":"luis@housestuff.local","password":"LuisHouse#2026","rememberMe":false}'
if ($login.email -ne "luis@housestuff.local") { throw "Login do Luis retornou usuário inesperado." }
$residence = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/residences/current" -WebSession $session
if ($residence.name -ne "Casa do Luis") { throw "Residência local de demonstração não foi encontrada." }
$pots = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/pots" -WebSession $session
if ($pots.Count -lt 3) { throw "Potes locais de demonstração não foram encontrados." }
$routine = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/routine" -WebSession $session
if ($null -eq $routine.upcoming -or $null -eq $routine.history) { throw "Visão de rotina retornou contrato inválido." }
Invoke-RestMethod -Uri "http://localhost:5049/api/v1/auth/logout" -Method Post -WebSession $session | Out-Null

Write-Host "HouseStuff validado: build, testes, prontidão, frontend e sessão do Luis estão funcionando."
