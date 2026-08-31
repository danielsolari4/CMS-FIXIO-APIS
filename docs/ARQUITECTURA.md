# Arquitectura de despliegue

## Infraestructura

- **Droplet DigitalOcean**: `159.223.183.214` — Ubuntu 24.04.4, 2 vCPU, 2 GB RAM (+2 GB swap).
- Contenido del monorepo **APIFE** (.NET 10): `Ray.FrontendApi` (API pública de contenido) y `Ray.BackendApi` (API administrativa/backend).
- Acceso: `ssh root@159.223.183.214`. Desde Windows se usa PowerShell con `ssh`/`scp`.

## Dominios y entrada

nginx expone los dos dominios (Let's Encrypt vía Certbot en `/etc/letsencrypt/live/<dominio>/*.pem`):

| Dominio | upstream (127.0.0.1) | Qué es |
|---|---|---|
| `apife.fixiocode.com` | `:8080` | `frontendapi` (Ray.FrontendApi) |
| `apibe.fixiocode.com` | `:8081` | `backendapi` (Ray.BackendApi) |

Config: `/etc/nginx/sites-enabled/{apife,apibe}.fixiocode.com` con `proxy_pass`, `client_max_body_size 100M`, `proxy_read_timeout 300s`.

## Contenedores (Docker Compose)

Proyecto en el droplet: **`/root/api-fe`** (`docker-compose.yml` + `.env` + carpeta `solr/`).

| Contenedor | Imagen | Puertos | Rol |
|---|---|---|---|
| `frontendapi` | `fixiocode/api-fe-frontendapi:latest` (build local, se transfiere) | `127.0.0.1:8080→80` | API de contenido (pública) |
| `backendapi` | `fixiocode/api-fe-backendapi:latest` (build local, se transfiere) | `127.0.0.1:8081→80` | API administrativa (JWT) |
| `solr` | `fixiocode/api-fe-solr:latest` (build en el droplet, config desde repo) | `127.0.0.1:8983→8983` | Motor de búsqueda |
| `sqlserver` | `mcr.microsoft.com/mssql/server:2022-latest` | interno `1433` | Base de datos principal |
| `mongo` | `mongo:7` | interno `27017` | Base secundaria (conteos de vistas, sesiones, etc.) |

> Las imágenes .NET se buildean con Docker Desktop local y se transfieren
> (`docker save` → `scp` → `docker load`); el droplet solo hace
> `docker compose up -d --force-recreate`. Solr se builda en el droplet
> porque su Dockerfile es liviano (`FROM solr:8.11.4` + config).

Todos comparten la red externa **`cms-network`** (se creó una vez: `docker network create cms-network`). Los contenedores se resuelven entre sí por nombre (`sqlserver`, `mongo`, `solr`).

> Los puertos de `sqlserver`/`mongo` no están mapeados a la máquina host; solo se acceden por DNS de la red interna. `solr` y las APIs están limitados a `127.0.0.1`.

## Variables de entorno (arranque)

`/root/api-fe/.env` (secreto) provee `SQL_CONNECTION` y `MONGO_CONNECTION`. El compose los inyecta como:

```yaml
ConnectionStrings__DefaultConnection: ${SQL_CONNECTION}
appSettings__mongoDb__connectionString: ${MONGO_CONNECTION}
appSettings__jwt__issuer: ${JWT_ISSUER:-http://apife.fixiocode.com/}
appSettings__jwt__key: ${JWT_KEY:-VisaCardLabJWT-Auth}
appSettings__solr__url: ${SOLR_URL:-http://solr:8983/solr/}        # frontend + backend
appSettings__syncLayout__url: ${SYNC_LAYOUT_URL:-http://frontendapi/api/LayoutInstance/InternalSyncSolrByNode}
```

Nota: `appSettings__solr__url` apunta a `http://solr:8983/solr/` (DNS interno), por lo que las APIs ya hablan con Solr sin necesidad de rebuild. El `appsettings.json` por defecto del código trae URLs antiguas (`192.168.100.165:8988`, `:8999`) que **quedan pisadas por estas variables en producción**.

## Base de datos

- **SQL Server** (`sqlserver:1433`): base `CMS_STG_TT_NET_CORE`, restaurada desde un bacpac de producción (`fixioCMS`). Usuario app: `sa` (password en `/root/.secrets/sqlserver.env` y en el `.env`).
- **MongoDB** (`mongo:27017`): bases de la plataforma (por ejemplo `assetviewscounts` para "lo más leído"). ⚠ El conteo de vistas no fue restaurado todavía → `GetMostRead` responde 204 (esperado, no es bug).
- No hay `__EFMigrationsHistory`: no se debe ejecutar migraciones en runtime.
- `Ray.Model/NewContext/ModelContext.cs`: `OnConfiguring` lee la env var.

## SDK .NET

- `global.json` pinea **10.0.301**. La máquina local tiene SDK 9 → **no se
  buildea con dotnet local**: el build ocurre con **Docker Desktop** (imagen
  `mcr.microsoft.com/dotnet/sdk:10.0` dentro del contenedor), igual que antes
  en el droplet. La imagen resultante se exporta (`docker save`) y se transfiere
  al droplet, que solo hace `docker load` + `docker compose up -d`.
- Dockerfiles: `Ray.FrontendApi/Dockerfile`, `Ray.BackendApi/Dockerfile` y
  `Ray.CMS/Dockerfile` (multi-stage, publican en .NET 10).
- `deploy/deploy.ps1` hace el flujo completo; `deploy/deploy-solr.ps1` solo la
  config de Solr.

## Paquetes / dependencias clave

- **AutoMapper 13.0.1** (migrado desde 10.1.1) — ver [AUTOMAPPER.md](AUTOMAPPER.md).
- COMET.ICCMS (búsqueda/NuevoContentful), System.Linq.Dynamic.Core, MongoDB.Driver — no requirieron cambios.