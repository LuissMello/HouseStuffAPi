[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path -Parent $PSScriptRoot
$trackingPath = Join-Path $repositoryRoot 'docs\tracking\project.json'
$project = Get-Content -Raw -LiteralPath $trackingPath | ConvertFrom-Json

$taskStatus = @{
    completed  = '[x]'
    inProgress = '[>]'
    blocked    = '[!]'
    pending    = '[ ]'
}

$roadmap = [System.Collections.Generic.List[string]]::new()
$roadmap.Add('# Roadmap')
$roadmap.Add('')
$roadmap.Add('> Arquivo gerado por `scripts/generate-project-docs.ps1`. Edite somente `docs/tracking/project.json`.')
$roadmap.Add('')
$roadmap.Add("Atualizado em: $($project.updatedAt).")
$roadmap.Add('')
foreach ($stage in $project.stages) {
    $roadmap.Add("## $($stage.id) — $($stage.name)")
    $roadmap.Add('')
    $roadmap.Add($stage.outcome)
    $roadmap.Add('')
    foreach ($task in @($project.tasks | Where-Object stageId -eq $stage.id)) {
        $roadmap.Add("- $($taskStatus[$task.status]) ``$($task.id)`` — $($task.title) — $($task.result)")
    }
    $roadmap.Add('')
}

$completed = @($project.tasks | Where-Object status -eq 'completed').Count
$active = @($project.tasks | Where-Object status -eq 'inProgress')
$next = @($project.tasks | Where-Object status -eq 'pending' | Select-Object -First 1)
$status = [System.Collections.Generic.List[string]]::new()
$status.Add('# Status do projeto')
$status.Add('')
$status.Add('> Arquivo gerado por `scripts/generate-project-docs.ps1`. Edite somente `docs/tracking/project.json`.')
$status.Add('')
$status.Add("Atualizado em: $($project.updatedAt).")
$status.Add('')
$status.Add("- Situação: **$($project.status)**.")
$status.Add("- Progresso: **$completed de $($project.tasks.Count) tarefas concluídas**.")
$status.Add("- Etapa atual: **$($project.currentStageId)**.")
$status.Add("- Tarefa ativa: **$(if ($active.Count) { $active[0].id } else { 'nenhuma' })**.")
$status.Add("- Próxima tarefa proposta: **$(if ($next.Count) { $next[0].id } else { 'nenhuma' })**.")
$status.Add('')
$status.Add('## Regra de conclusão')
$status.Add('')
$status.Add('Funcionalidades só são concluídas quando backend, frontend e integração podem ser executados e testados pelo usuário.')

Set-Content -LiteralPath (Join-Path $repositoryRoot 'docs\ROADMAP.md') -Value ($roadmap -join [Environment]::NewLine) -Encoding utf8
Set-Content -LiteralPath (Join-Path $repositoryRoot 'docs\STATUS.md') -Value ($status -join [Environment]::NewLine) -Encoding utf8

Write-Host 'ROADMAP.md e STATUS.md gerados a partir de docs/tracking/project.json.'
