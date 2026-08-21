[CmdletBinding()]
param([switch]$StopDatabase)

$ErrorActionPreference = "Stop"
$apiRoot = Split-Path $PSScriptRoot -Parent
$runtimeDir = Join-Path $apiRoot ".local"
$processFile = Join-Path $runtimeDir "processes.json"

function Stop-ProcessTree([int]$ProcessId) {
    $children = Get-CimInstance Win32_Process -Filter "ParentProcessId = $ProcessId" -ErrorAction SilentlyContinue
    foreach ($child in $children) { Stop-ProcessTree $child.ProcessId }
    Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
}

function Stop-RegisteredProcess($Entry) {
    if ($null -eq $Entry) { return }
    $process = Get-Process -Id ([int]$Entry.pid) -ErrorAction SilentlyContinue
    if ($null -eq $process) { return }
    $expected = [DateTimeOffset]::Parse([string]$Entry.startedAt).UtcDateTime
    if ([Math]::Abs(($process.StartTime.ToUniversalTime() - $expected).TotalSeconds) -gt 2) {
        Write-Warning "PID $($Entry.pid) foi reutilizado e não será encerrado."
        return
    }
    Stop-ProcessTree $process.Id
}

if (Test-Path -LiteralPath $processFile) {
    $registered = Get-Content -Raw -LiteralPath $processFile | ConvertFrom-Json
    Stop-RegisteredProcess $registered.api
    Stop-RegisteredProcess $registered.frontend
    Remove-Item -LiteralPath $processFile -Force
}

if ($StopDatabase) {
    Push-Location $apiRoot
    try { docker compose stop postgres } finally { Pop-Location }
}

Write-Host "Processos locais registrados pelo HouseStuff foram encerrados."
