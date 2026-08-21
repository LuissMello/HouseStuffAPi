[CmdletBinding()]
param([switch]$SkipStart)

$ErrorActionPreference = "Stop"
$apiRoot = Split-Path $PSScriptRoot -Parent
$workspaceRoot = Split-Path $apiRoot -Parent
$frontRoot = Join-Path $workspaceRoot "HouseStuffFront"
$localSdk = Join-Path $workspaceRoot ".dotnet-sdk-10\dotnet.exe"
$dotnet = if (Test-Path -LiteralPath $localSdk) { $localSdk } else { (Get-Command dotnet -ErrorAction Stop).Source }
$artifactsRoot = Join-Path $apiRoot ".local\validation-artifacts"

function Assert-Exit([int]$ExitCode, [string]$Step) {
    if ($ExitCode -ne 0) { throw "$Step falhou." }
}

function Get-UserSecrets {
    $values = @{}
    $lines = & $dotnet user-secrets list --project (Join-Path $apiRoot "src\HouseStuff.Api\HouseStuff.Api.csproj")
    if ($LASTEXITCODE -ne 0) { return $values }
    foreach ($line in $lines) {
        if ($line -match '^(.+?)\s*=\s*(.*)$') { $values[$matches[1].Trim()] = $matches[2] }
    }
    return $values
}

Push-Location $apiRoot
try {
    & $dotnet format HouseStuff.slnx --verify-no-changes --no-restore
    Assert-Exit $LASTEXITCODE "Formatação do backend"
    & $dotnet restore HouseStuff.slnx --artifacts-path $artifactsRoot
    Assert-Exit $LASTEXITCODE "Restauração isolada do backend"
    & $dotnet build HouseStuff.slnx --configuration Release --no-restore --artifacts-path $artifactsRoot
    Assert-Exit $LASTEXITCODE "Build do backend"
    & $dotnet test HouseStuff.slnx --configuration Release --no-build --no-restore --artifacts-path $artifactsRoot
    Assert-Exit $LASTEXITCODE "Testes do backend"
}
finally { Pop-Location }

Push-Location $frontRoot
try {
    $lint = Start-Process -FilePath "npm.cmd" -ArgumentList @("run", "lint") -WorkingDirectory $frontRoot -WindowStyle Hidden -Wait -PassThru
    Assert-Exit $lint.ExitCode "Lint do frontend"
    $tests = Start-Process -FilePath "npm.cmd" -ArgumentList @("test") -WorkingDirectory $frontRoot -WindowStyle Hidden -Wait -PassThru
    Assert-Exit $tests.ExitCode "Testes do frontend"
}
finally { Pop-Location }

if (-not $SkipStart) {
    & (Join-Path $PSScriptRoot "start-local.ps1") -SkipInstall
}

$ready = Invoke-WebRequest -Uri "http://localhost:5049/health/ready" -TimeoutSec 5
if ($ready.StatusCode -ne 200) { throw "Readiness da API falhou." }
$site = Invoke-WebRequest -Uri "http://localhost:3000" -TimeoutSec 5
if ($site.StatusCode -ne 200) { throw "Frontend não respondeu." }

$secrets = Get-UserSecrets
$smokeEmail = if ($secrets.ContainsKey("DevelopmentAdmin:Email")) { $secrets["DevelopmentAdmin:Email"] } else { "admin@housestuff.local" }
$smokePassword = if ($secrets.ContainsKey("DevelopmentAdmin:Password")) { $secrets["DevelopmentAdmin:Password"] } else { "HouseStuff#2026" }
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$loginBody = @{ email = $smokeEmail; password = $smokePassword; rememberMe = $false } | ConvertTo-Json
$login = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/auth/login" -Method Post -WebSession $session -ContentType "application/json" -Body $loginBody
if ($login.email -ne $smokeEmail) { throw "Login de validação retornou usuário inesperado." }
$residence = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/residences/current" -WebSession $session
if ([string]::IsNullOrWhiteSpace($residence.name)) { throw "Residência da conta de validação não foi encontrada." }
$pots = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/pots" -WebSession $session
if ($pots.Count -lt 1) { throw "Nenhum pote foi encontrado para a conta de validação." }
$routine = Invoke-RestMethod -Uri "http://localhost:5049/api/v1/routine" -WebSession $session
if ($null -eq $routine.upcoming -or $null -eq $routine.history) { throw "Visão de rotina retornou contrato inválido." }
Invoke-RestMethod -Uri "http://localhost:5049/api/v1/auth/logout" -Method Post -WebSession $session | Out-Null

Write-Host "HouseStuff validado: build, testes, prontidão, frontend e sessão configurada estão funcionando."
