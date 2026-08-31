#requires -Version 5.1
<#
.SYNOPSIS
  Deploy de la configuracion de Solr al droplet (rebuild del contenedor solr).

.DESCRIPTION
  Cuando cambia la config de solr (carpeta solr/ del repo: Dockerfile,
  mssql-jdbc, solr_home), este script:
    1. Empaqueta ./solr (excluye las carpetas data/ de los cores)
    2. scp y extrae en /root/api-fe/solr (droplet)
    3. docker compose build solr + up -d --force-recreate --no-deps solr

  El build de solr es liviano (FROM solr:8.11.4 + copy) y NO satura la RAM
  del droplet (a diferencia de los builds .NET). La data de los cores vive
  en el volumen /var/solr, por lo que recrear el contenedor NO pierde datos.

  Requisito: la carpeta solr/ debe existir en el repo local (commitear la
  config). Si no esta, descargala una vez desde el droplet:
    scp -r root@159.223.183.214:/root/api-fe/solr .

.EXAMPLE
  .\deploy-solr.ps1                # sync config + rebuild solr
  .\deploy-solr.ps1 -WhatIf        # muestra los comandos sin ejecutarlos
#>
[CmdletBinding()]
param(
    [string]$Ip    = '159.223.183.214',
    [string]$User  = 'root',
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

$RepoRoot   = Join-Path $PSScriptRoot '..'
$SolrLocal  = Join-Path $RepoRoot 'solr'
$DropletDir = '/root/api-fe'
$TgzPath    = Join-Path $env:TEMP 'solr-config.tar.gz'
$Remote     = "$User@$Ip"

if (-not (Test-Path $SolrLocal)) {
    throw "No existe $SolrLocal en el repo. La config de solr se sube desde el repo. Descargala una vez desde el droplet:
  scp -r root@159.223.183.214:/root/api-fe/solr ."
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

Push-Location $RepoRoot
try {
    Remove-Item $TgzPath -ErrorAction SilentlyContinue

    Invoke-Native 'Empaquetando config de solr' @(
        'tar',
        '--exclude=solr_home/*/data',
        '-czf', $TgzPath, 'solr'
    )

    Invoke-Native 'Subiendo config al droplet' @('scp', $TgzPath, "$Remote`:$DropletDir/")

    $remoteCmd = "cd $DropletDir && tar -xzf solr-config.tar.gz && docker compose build solr && docker compose up -d --force-recreate --no-deps solr && docker image prune -f"
    Invoke-Native 'Build + up solr en el droplet' @('ssh', $Remote, $remoteCmd)

    Invoke-Native 'Estado del contenedor' @('ssh', $Remote, 'docker ps --filter name=solr')
}
finally {
    Pop-Location
    Remove-Item $TgzPath -ErrorAction SilentlyContinue
}

Write-Host "`nDeploy de solr completado." -ForegroundColor Green