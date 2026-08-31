#requires -Version 5.1
<#
.SYNOPSIS
  Deploy de APIS-TODAS (frontendapi / backendapi) al droplet DigitalOcean.

.DESCRIPTION
  El droplet YA NO compila: las imagenes se buildean en Docker Desktop local,
  se exportan (docker save), se suben por scp y en el droplet solo se hace
  docker load + docker compose up -d --force-recreate.

  solr NO se buildea con este flujo (la carpeta solr/ no esta en el repo);
  queda la imagen existente del droplet. Si algun dia hay que rebuildearlo:
  docker compose up -d --build solr  (manual, en el droplet).

  Flujo por servicio:
    docker build (local) -> docker save -> tar.gz -> scp ->
    docker load + docker compose up -d --force-recreate <servicios>.

  Requisitos: Docker Desktop corriendo, ssh/scp/tar (Windows 10/11) y
  acceso SSH al droplet. El .env del droplet (/root/api-fe/.env) no se toca.

.EXAMPLE
  .\deploy.ps1                              # build + deploy frontendapi y backendapi
  .\deploy.ps1 -Service frontendapi         # solo la API publica
  .\deploy.ps1 -Service backendapi          # solo la API administrativa
  .\deploy.ps1 -Branch main                 # fetch + checkout + pull y deploya
  .\deploy.ps1 -WhatIf                      # muestra los comandos sin ejecutarlos
#>
[CmdletBinding()]
param(
    [string]$Ip     = '159.223.183.214',
    [string]$User   = 'root',
    [string]$Branch = '',
    [ValidateSet('', 'frontendapi', 'backendapi', 'solr')]
    [string]$Service = '',
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

$RepoRoot   = Join-Path $PSScriptRoot '..'
$DropletDir = '/root/api-fe'
$Remote     = "$User@$Ip"

$ImageMap = @{
    'frontendapi' = 'fixiocode/api-fe-frontendapi:latest'
    'backendapi'  = 'fixiocode/api-fe-backendapi:latest'
}
$BaseMap = @{
    'frontendapi' = 'api-fe-frontendapi'
    'backendapi'  = 'api-fe-backendapi'
}

$Services = @('frontendapi', 'backendapi')
if ($Service) {
    if ($Service -eq 'solr') {
        throw 'solr no se buildea con este flujo (la carpeta solr/ no esta en el repo). Hacelo manual en el droplet: docker compose up -d --build solr'
    }
    $Services = @($Service)
}

function Invoke-Native {
    param(
        [string]$Label,
        [string[]]$Command
    )
    Write-Host "`n>>> $Label" -ForegroundColor Cyan
    Write-Host '    ' ($Command -join ' ') -ForegroundColor DarkGray
    if ($WhatIf) { return }
    $rest = @($Command | Select-Object -Skip 1)
    & $Command[0] @rest
    if ($LASTEXITCODE -ne 0) {
        throw "Fallo: $Label (exit $LASTEXITCODE)"
    }
}

if (-not $WhatIf) {
    docker info *> $null
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Desktop no está corriendo. Levantalo y volvé a ejecutar.'
    }
}

Push-Location $RepoRoot
try {
    if ($Branch) {
        Invoke-Native 'fetch origin'          @('git', 'fetch', 'origin')
        Invoke-Native "checkout $Branch"      @('git', 'checkout', $Branch)
        Invoke-Native "pull origin $Branch"   @('git', 'pull', 'origin', $Branch)
    }

    $dirty      = @(& git status --porcelain 2>&1)
    $statusExit = $LASTEXITCODE
    if ($statusExit -ne 0) { throw "No se pudo leer el estado de git en $RepoRoot" }
    if ($dirty.Count -gt 0) {
        Write-Host "`nHay cambios sin commitear en $RepoRoot :" -ForegroundColor Yellow
        $dirty | Select-Object -First 10 | ForEach-Object { Write-Host "  $_" -ForegroundColor Yellow }
        if ($dirty.Count -gt 10) { Write-Host "  (... $(($dirty.Count - 10)) mas)" -ForegroundColor Yellow }
        $ans = Read-Host '¿Buildear y deployar igual? [s/N]'
        if ($ans -notmatch '^(s|si|sí|y|yes)$') { Write-Host 'Abortado.'; exit 0 }
    }

    $dockerfileMap = @{
        'frontendapi' = 'Ray.FrontendApi/Dockerfile'
        'backendapi'  = 'Ray.BackendApi/Dockerfile'
    }

    foreach ($svc in $Services) {
        $image = $ImageMap[$svc]
        $base  = $BaseMap[$svc]
        $tar   = Join-Path $env:TEMP "$base.tar"
        $tgz   = Join-Path $env:TEMP "$base.tar.gz"

        Remove-Item $tar, $tgz -ErrorAction SilentlyContinue

        Invoke-Native "Build $svc (Docker Desktop, local)" @(
            'docker', 'build',
            '-f', $dockerfileMap[$svc],
            '-t', $image, '.'
        )
        Invoke-Native "Exportando $svc"   @('docker', 'save', '-o', $tar, $image)
        Invoke-Native "Comprimiendo $svc" @('tar', '-czf', $tgz, '-C', $env:TEMP, "$base.tar")
        Invoke-Native "Subiendo $svc al droplet" @('scp', $tgz, "$Remote`:$DropletDir/")
    }

    $loadCmds = foreach ($svc in $Services) {
        $base = $BaseMap[$svc]
        "tar -xzf $base.tar.gz && docker load -i $base.tar"
    }
    $upSvc   = $Services -join ' '
    $remoteCmd = "cd $DropletDir && " + ($loadCmds -join ' && ') + " && docker compose up -d --force-recreate $upSvc && docker image prune -f"
    Invoke-Native 'Cargar imagenes + up en el droplet' @('ssh', $Remote, $remoteCmd)

    Invoke-Native 'Estado frontendapi' @('ssh', $Remote, 'docker ps --filter name=frontendapi')
    Invoke-Native 'Estado backendapi'  @('ssh', $Remote, 'docker ps --filter name=backendapi')
    Invoke-Native 'Smoke test apife (HTTP)' @('ssh', $Remote, 'curl -sI http://127.0.0.1:8080')
}
finally {
    Pop-Location
    foreach ($svc in $Services) {
        $base = $BaseMap[$svc]
        Remove-Item (Join-Path $env:TEMP "$base.tar"), (Join-Path $env:TEMP "$base.tar.gz") -ErrorAction SilentlyContinue
    }
}

Write-Host "`nDeploy completado: https://apife.fixiocode.com / https://apibe.fixiocode.com" -ForegroundColor Green