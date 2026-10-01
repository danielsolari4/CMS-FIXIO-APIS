#requires -Version 5.1
<#
.SYNOPSIS
  Deploy de la configuracion de Solr al droplet (rebuild del contenedor solr).

.DESCRIPTION
  Cuando cambia la config de solr (carpeta solr/ del repo: Dockerfile,
  mssql-jdbc, solr_home), este script:
    1. Empaqueta ./solr (excluye las carpetas data/ de los cores)
    2. scp y extrae en /root/api-fe/solr (droplet), y sube docker-compose.yml
    3. Hace backup best-effort de los indices actuales
    4. Migra los indices actuales al volumen api-fe-solr-data
    5. docker compose build solr + up -d --force-recreate --no-deps solr
    6. Valida que /var/solr este montado

  El build de solr es liviano (FROM solr:8.4.0 + copy) y NO satura la RAM
  del droplet (a diferencia de los builds .NET). La data de los cores debe
  vivir en /var/solr/data/<core>, respaldada por el volumen api-fe-solr-data.
  Por eso docker-compose.yml debe montar api-fe-solr-data en /var/solr.

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
$SolrDataVolume = 'api-fe-solr-data'

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
    $timestamp = Get-Date -Format 'yyyyMMdd-HHmmss'

    Invoke-Native 'Empaquetando config de solr' @(
        'tar',
        '--exclude=solr_home/*/data',
        '-czf', $TgzPath, 'solr'
    )

    Invoke-Native 'Subiendo config al droplet' @('scp', $TgzPath, "$Remote`:$DropletDir/")
    Invoke-Native 'Backup docker-compose remoto' @('ssh', $Remote, "cd $DropletDir && if [ -f docker-compose.yml ]; then cp docker-compose.yml docker-compose.yml.bak-$timestamp; fi")
    Invoke-Native 'Subiendo docker-compose.yml' @('scp', (Join-Path $RepoRoot 'docker-compose.yml'), "$Remote`:$DropletDir/docker-compose.yml")

    $backupName = "solr-data-before-deploy-$timestamp.tar.gz"
    $backupCmd = "cd $DropletDir && mkdir -p solr-backups && if docker ps --format '{{.Names}}' | grep -qx solr; then docker exec solr sh -lc 'rm -rf /tmp/solr-index-export && mkdir -p /tmp/solr-index-export && if [ -d /var/solr/data ] && find /var/solr/data -mindepth 2 -maxdepth 2 -type d -name index | grep -q .; then for d in /var/solr/data/*; do core=`$(basename ""`$d""); mkdir -p ""/tmp/solr-index-export/`$core/data""; cp -a ""`$d/."" ""/tmp/solr-index-export/`$core/data/""; done; else cd /opt/solr/server/solr && for d in */data; do core=`${d%/data}; mkdir -p ""/tmp/solr-index-export/`$core/data""; cp -a ""`$d/."" ""/tmp/solr-index-export/`$core/data/""; done; fi && cd /tmp/solr-index-export && tar -czf /tmp/$backupName */data' && docker cp solr:/tmp/$backupName solr-backups/$backupName || true; fi"
    Invoke-Native 'Backup best-effort de indices actuales' @('ssh', $Remote, $backupCmd)

    $migrateCmd = "cd $DropletDir && tar -xzf solr-config.tar.gz && docker volume create $SolrDataVolume >/dev/null && if [ -f solr-backups/$backupName ]; then docker run --rm --user root -v $SolrDataVolume`:/target -v $DropletDir/solr-backups:/backups solr:8.4.0 sh -lc 'rm -rf /target/data && mkdir -p /target/data /tmp/solr-index-backup && tar -xzf /backups/$backupName -C /tmp/solr-index-backup && for d in /tmp/solr-index-backup/*/data; do core=`$(basename `$(dirname ""`$d"")); mkdir -p ""/target/data/`$core""; cp -a ""`$d/."" ""/target/data/`$core/""; done; chown -R 8983:8983 /target && chmod 775 /target'; fi"
    Invoke-Native 'Migrando indices actuales al volumen persistente' @('ssh', $Remote, $migrateCmd)

    $remoteCmd = "cd $DropletDir && docker compose build solr && docker compose up -d --force-recreate --no-deps solr && docker inspect solr --format '{{range .Mounts}}{{println .Destination}}{{end}}' | grep -qx '/var/solr' && docker image prune -f"
    Invoke-Native 'Build + up solr en el droplet' @('ssh', $Remote, $remoteCmd)

    Invoke-Native 'Estado del contenedor' @('ssh', $Remote, 'docker ps --filter name=solr')
}
finally {
    Pop-Location
    Remove-Item $TgzPath -ErrorAction SilentlyContinue
}

Write-Host "`nDeploy de solr completado." -ForegroundColor Green
