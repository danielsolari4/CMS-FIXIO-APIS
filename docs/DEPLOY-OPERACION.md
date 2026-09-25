# Deploy y operación

El droplet **ya no compila**. Las imágenes (.NET 10) se buildean con
Docker Desktop local, se exportan (`docker save` -> `tar -gz`), se suben
por `scp` y en el droplet solo se hace `docker load` + `docker compose up -d`.
Esto evita saturar la RAM del droplet (2 GB) que hacía que el build .NET
OOM-killeara a `solr`.

Desde Windows (PowerShell), desde la raíz del repo `APIS-TODAS`:

```powershell
# Build + deploy de frontendapi y backendapi
.\deploy\deploy.ps1

# Solo una API
.\deploy\deploy.ps1 -Service frontendapi
.\deploy\deploy.ps1 -Service backendapi

# Config de solr (solo si cambió la carpeta solr/ del repo)
.\deploy\deploy-solr.ps1
```

> Config del droplet versionada en el repo: `docker-compose.yml` (raíz, idéntico
> al droplet), `deploy/droplet/.env` (credenciales reales de SQL/Mongo) y `solr/`
> (Dockerfile + `solr_home` + jar mssql-jdbc). `deploy-solr.ps1` sube la config y
> builda en el droplet solo si cambió `solr/`.

`docker compose up -d --force-recreate frontendapi backendapi` se ejecuta
en el droplet (`/root/api-fe`) sin `--build`. `pull_policy: never` hace que
compose falle rápido si la imagen no está cargada.

> **Ojo con `solr`**: los índices viven en la capa del contenedor
> (`/opt/solr/server/solr/<Core>/data`, p. ej. `User` ~104MB, `Keywords` ~67MB);
> el volumen `/var/solr` solo guarda logs/config. Un `--force-recreate`/rebuild
> de solr **borra los índices** → repoblarlos con `full-import` (ver [SOLR.md](SOLR.md)).
> Los deploys normales no tocan el contenedor `solr`.

## Comandos de verificación frecuentes

```bash
docker ps                                          # estados
docker logs frontendapi --tail 100                 # logs de la API
docker logs solr --tail 100                        # logs de Solr
docker stats solr                                  # memoria del contenedor
curl -s http://127.0.0.1:8983/solr/admin/cores?action=STATUS   # cores de Solr
```

Pruebas funcionales por HTTPS:

```bash
# API pública de contenido (datos reales desde Solr)
curl -s "https://apife.rinocode.com/api/Keyword/GetAllSolr?q=*:*&rows=5"
curl -s "https://apife.rinocode.com/api/Category/GetAllSolr?q=*:*&rows=5"
curl -s "https://apife.rinocode.com/api/Channel/GetChannels?q=*:*&rows=5"
curl -s "https://apife.rinocode.com/api/Author/GetAllSolr?q=IsEnabled:true&rows=5"

# Backend protegido por JWT (401 = JWT OK, solo falla la auth)
curl -s -o /dev/null -w "%{http_code}" "https://apibe.rinocode.com/api/Account/UserInfo"

# Solr directo (solo en el droplet)
curl -s "http://127.0.0.1:8983/solr/Channel/select?q=*:*&rows=0"
```

## Lo que YA está operativo

- AutoMapper migrado y desplegado → las APIs compilan en .NET 10.
- Solr 8.11.4 corriendo con los **20 cores** configurados (ver [SOLR.md](SOLR.md)).
- Cores poblados vía `full-import`: Author, Category, Channel, Keywords (484.576), Menu, SettingsCore.
- Endpoints de búsqueda respondiendo con datos reales por HTTPS.

## Pendientes

- Tras un recreate de solr (pérdida de índices) repoblar con `full-import` según [SOLR.md](SOLR.md).
- Restaurar `assetviewscounts` en Mongo para que `GetMostRead` deje de dar 204.
- Core `Theme` sin config en el backup de Solr (solo existe el SP en SQL) — decidir si se clona de otro core.

## Lecciones aprendidas (evitar repetir errores)

- **PowerShell ↔ bash**: escribir scripts `.ps1`/`.sh` en archivo y transferirlos (scp). Las comillas anidadas en `ssh "..." -Command "..."` comen `$variables`. En el droplet NO existe PowerShell (todo bash).
- **sqlcmd contra el contenedor**: no usar `-Q` con comillas anidadas. Escribir el `.sql`, `scp` al droplet, `docker cp` dentro de `sqlserver` y ejecutarlo con `-i archivo.sql`.
- **AutoMapper encadenado** (`CreateMap().ForAllMembers(...)`) ya no compila: ver [AUTOMAPPER.md](AUTOMAPPER.md). No reintroducir el patrón.
- Si un archivo aparece a medias en el droplet tras un `scp` con caracteres extraños, re-transferirlo completo.