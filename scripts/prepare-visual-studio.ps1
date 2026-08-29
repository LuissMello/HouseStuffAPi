[CmdletBinding()]
param([switch]$SkipInstall)

$ErrorActionPreference = "Stop"
$apiRoot = Split-Path $PSScriptRoot -Parent
$workspaceRoot = Split-Path $apiRoot -Parent
$frontRoot = Join-Path $workspaceRoot "HouseStuffFront"
$apiProject = Join-Path $apiRoot "src\HouseStuff.Api\HouseStuff.Api.csproj"

function Assert-LastExit([string]$Step) {
    if ($LASTEXITCODE -ne 0) { throw "$Step falhou." }
}

function Resolve-DotNet {
    $localSdk = Join-Path $workspaceRoot ".dotnet-sdk-8\dotnet.exe"
    if (Test-Path -LiteralPath $localSdk) { return $localSdk }
    $command = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($command) { return $command.Source }
    throw ".NET 8 não foi encontrado. Instale o SDK 8.0 indicado pelo global.json."
}

if (-not (Test-Path -LiteralPath $frontRoot)) {
    throw "HouseStuffFront precisa estar ao lado de HouseStuffAPi."
}

$dotnet = Resolve-DotNet
$sdk8 = & $dotnet --list-sdks | Where-Object { $_ -match '^8\.' } | Select-Object -First 1
if (-not $sdk8) { throw "O SDK .NET 8 não está instalado." }

$node = Get-Command node -ErrorAction SilentlyContinue
$npm = Get-Command npm.cmd -ErrorAction SilentlyContinue
if (-not $node -or -not $npm) { throw "Node.js 22 e npm não foram encontrados." }
$nodeVersion = [Version]((& $node.Source --version).TrimStart('v'))
if ($nodeVersion -lt [Version]'22.13.0') { throw "Node.js 22.13 ou superior é necessário." }

$secretLines = & $dotnet user-secrets list --project $apiProject
Assert-LastExit "Leitura dos User Secrets"
$hasDatabase = $secretLines | Where-Object { $_ -match '^ConnectionStrings:HouseStuff\s*=\s*\S+' }
if (-not $hasDatabase) {
    throw 'Configure ConnectionStrings:HouseStuff nos User Secrets antes de iniciar. Consulte docs\VISUAL_STUDIO.md.'
}

if (-not $SkipInstall) {
    Push-Location $apiRoot
    try {
        & $dotnet restore HouseStuff.sln
        Assert-LastExit "Restauração do backend"
    }
    finally { Pop-Location }

    Push-Location $frontRoot
    try {
        & $npm.Source ci
        Assert-LastExit "Instalação do frontend"
    }
    finally { Pop-Location }
}

Write-Host "HouseStuff preparado para o Visual Studio."
Write-Host "Banco: PostgreSQL configurado em User Secrets (valor não exibido)."
Write-Host "Solução: $apiRoot\HouseStuff.VisualStudio.sln"
Write-Host "Depois de abrir a solução, selecione 'HouseStuff completo' e pressione F5."
