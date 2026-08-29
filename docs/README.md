# APIFE — Documentación de la plataforma

Índice rápido para el equipo. Todo ocurre en un droplet DigitalOcean y el despliegue se hace desde una máquina Windows (PowerShell) vía `scp`/`ssh`.

| Doc | Contenido |
|---|---|
| [ARQUITECTURA.md](ARQUITECTURA.md) | Topología de contenedores, red, dominios, datos y versiones. |
| [DEPLOY-OPERACION.md](DEPLOY-OPERACION.md) | Cómo levantar, buildear, verificar y solucionar problemas. |
| [SOLR.md](SOLR.md) | Motor de búsqueda: por qué Solr 8.11.4, cores, imports DIH, monitoreo. |
| [AUTOMAPPER.md](AUTOMAPPER.md) | Migración AutoMapper 10 → 13 (cómo quedó el código). |

Resumen de una mirada:

```
            Internet (HTTPS 443)
                  │
            nginx (droplet)
        ┌─────────┴──────────┐
  apife.fixiocode.com   apibe.fixiocode.com
     → 127.0.0.1:8080      → 127.0.0.1:8081
        frontendapi          backendapi
        └────────┬─────────────┘
             red cms-network
     ┌───────────┼───────────────┐
  sqlserver    mongo          solr
  2022:1433    mongo:7        solr:8.11.4 :8983
```

- Frontend público del portal: **no desplegado** en este droplet (solo las APIs de contenido).
- Los 3 servicios internos (`sqlserver`, `mongo`, `solr`) NO exponen puertos públicos; `solr` y las APIs solo escuchan en `127.0.0.1`.