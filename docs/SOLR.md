# Solr — motor de búsqueda

## Cómo se llegó a esta solución

La plataforma ICCMS depende de Solr con el **DataImportHandler (DIH)** para casi toda la consulta pública (búsquedas, sitios, menús, galerías, canales, etc.). El nuevo droplet no tenía Solr, así que se replicó usando la configuración original de producción.

### Origen de los archivos

- Backup del usuario (máquina local): `C:\Users\Daniel\Downloads\solr_backup\solr_backup\var\solr\data`
- Contiene solo **configs** de 20 cores (carpetas `conf/` + `core.properties` + `solr.xml`), **sin índices** → hay que poblarlos con `full-import`.
- Staging intermedio local: `C:\Users\Daniel\AppData\Local\Temp\opencode\solr_home` (dataSources retargeteados a SQL local).
- En el droplet: **`/root/api-fe/solr/solr_home`** (idéntico al staging), `Dockerfile`, `mssql-jdbc-9.4.1.jre8.jar`.

### Por qué Solr 8.11.4 (importante)

El `luceneMatchVersion` de los `solrconfig.xml` dice 6.6.0, pero el `solr.xml` del backup pertenece a **Solr 8.x** (`<int name="maxBooleanClauses">`, `zkACLProvider`, etc.). Los intentos con imagen `6.6.6` y `7.7.3` murieron con:

```
Unknown configuration value in solr.xml: maxBooleanClauses
```

⇒ la base correcta es **`solr:8.11.4`**.

## Deploy

`/root/api-fe/solr/Dockerfile`:

```dockerfile
FROM solr:8.11.4
COPY /solr_home /opt/solr/server/solr
COPY /mssql-jdbc-9.4.1.jre8.jar /opt/solr/contrib/dataimporthandler/lib/mssql-jdbc.jar
RUN chown -R solr:solr /opt/solr/server/solr && chown solr:solr /opt/solr/contrib/dataimporthandler/lib/mssql-jdbc.jar
USER solr
```

Servicio en `docker-compose.yml`:

```yaml
solr:
  build: { context: ./solr, dockerfile: Dockerfile }
  container_name: solr
  restart: unless-stopped
  networks: [cms-network]
  ports: ["127.0.0.1:8983:8983"]
  environment:
    - SOLR_HEAP=512m
    - SOLR_HOME=/opt/solr/server/solr
```

Dos trampas resueltas:

1. La imagen `solr:8` busca los cores en `/var/solr/data` por defecto → se fija **`SOLR_HOME=/opt/solr/server/solr`** (adonde se copió el home).
2. **No es 6.x ni 7.x** (ver arriba).

## Cores (20)

`Author, Category, Channel, Gallery, Keywords, Layout, LayoutInstance, LayoutInstanceByNode, Media, Menu, News, NewsVersion, Node, Pages, PrintEdition, ProgrammingGuide, SettingsCore, URLRedirect, User, Widget`

Cada core tiene su `conf/db-data-config.xml` con `dataSource` apuntando a SQL local:

```
jdbc:sqlserver://sqlserver:1433;databaseName=CMS_STG_TT_NET_CORE;  user=sa  (password en .env/.secrets)
```

- 14 cores importan vía **stored procedures**: `dbo.GetAllSolrX`, `dbo.GetDeltaSolrX`, `dbo.GetDeltaQuerySolrX` (X = nombre del core). Verificado: **todos los SP existen** en `CMS_STG_TT_NET_CORE`.
- `Menu` y `SettingsCore`: consultas SQL directas.
- `LayoutInstanceByNode` y `NewsVersion`: no se llenan por DIH (documentos publicados por la API).

## Imports (poblar índices)

```bash
# Full import de un core (clean = borra lo previo, commit = indexa)
curl -s "http://127.0.0.1:8983/solr/<Core>/dataimport?command=full-import&clean=true&commit=true"

# Monitorear (status pasa a "busy" y termina en "idle")
curl -s "http://127.0.0.1:8983/solr/<Core>/dataimport?command=status&wt=json"

# Consultar cantidad de docs
curl -s "http://127.0.0.1:8983/solr/<Core>/select?q=*:*&rows=0&wt=json"
```

Estado de imports (última corrida):

| Core | Docs | Nota |
|---|---|---|
| Keywords | 484.576 | OK |
| Author | 229 | OK |
| Category | 340 | OK |
| Channel | 9 | OK |
| Menu / SettingsCore | 3 / 4 | OK |
| Gallery | 0 | **abortado** — es gigantesco (millones de filas entre Assets y subconsultas; a los 22 min iba en ~287k docs con ~900k consultas a SQL) |
| News / Media / Node / resto | 0 | pendientes |

Los imports se lanzan mejor con un script (`/root/run_imports.sh` en el droplet) que encola cada core y espera `idle`. Para los pesados (Gallery, Media, News) correrlos **en background** (`nohup`) y vigilar RAM (`docker stats`): el droplet tiene solo 2 GB.

## Integración con la aplicación

- Las APIs leen `appSettings__solr__url = http://solr:8983/solr/` (env del compose). Ya hay prueba end-to-end por HTTPS (`/api/Keyword/GetAllSolr`, `/api/Channel/GetChannels`, etc.).
- `Rino.Utils/Solr/SolrHelper.cs` construye la URL por core según las claves `appSettings.solr.solrCore.*` de `appsettings.json` (p. ej. `theme: "Theme"`, `news: "News"`).
- La app dispara `delta-import`/`full-import` desde la API (ej.: `SolrHelper.DataImport(...)` en `AccountController`, `KeywordController`) y `InternalSyncSolrByNode` para layouts.

## Skyline de todo lo que la app necesita de Solr

`/api/<Controller>/GetAllSolr` (o equivalentes) por core en `Rino.FrontendApi/Controllers/*.cs`. `SitemapHelper` y `NewsController` leen `NODE`, `NEWS` y `MEDIA` para sitemaps y "últimas noticias/videos".

## Gaps conocidos

- **Core `Theme`**: el appsettings lo pide (`solrCore.theme → Theme`) y el SP `GetAllSolrTheme` existe en SQL, pero **el backup no incluye su `conf/`**. Decisión pendiente: clonar la config de un core parecido y crear el core, o avisar que el tema se sirva sin Solr.
- `assetviewscounts` (Mongo) no restaurado → `GetMostRead` sigue en 204 aunque se indexe News.