# Deploy y operación

Todo el build/despliegue se hace en el droplet. Desde Windows (PowerShell):

```powershell
$S = "root@159.223.183.214"

# Subir algún archivo del repo
scp "Ruta\local\archivo.csproj" $S:/root/api-fe/...  # o al path que corresponda

# Build + recrear un servicio (se ejecuta en el droplet)
ssh $S "cd /root/api-fe && docker compose up -d --build frontendapi"

# Recrear todo
ssh $S "cd /root/api-fe && docker compose up -d --build"
```

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
curl -s "https://apife.fixiocode.com/api/Keyword/GetAllSolr?q=*:*&rows=5"
curl -s "https://apife.fixiocode.com/api/Category/GetAllSolr?q=*:*&rows=5"
curl -s "https://apife.fixiocode.com/api/Channel/GetChannels?q=*:*&rows=5"
curl -s "https://apife.fixiocode.com/api/Author/GetAllSolr?q=IsEnabled:true&rows=5"

# Backend protegido por JWT (401 = JWT OK, solo falla la auth)
curl -s -o /dev/null -w "%{http_code}" "https://apibe.fixiocode.com/api/Account/UserInfo"

# Solr directo (solo en el droplet)
curl -s "http://127.0.0.1:8983/solr/Channel/select?q=*:*&rows=0"
```

## Lo que YA está operativo

- AutoMapper migrado y desplegado → las APIs compilan en .NET 10.
- Solr 8.11.4 corriendo con los **20 cores** configurados (ver [SOLR.md](SOLR.md)).
- Cores poblados vía `full-import`: Author, Category, Channel, Keywords (484.576), Menu, SettingsCore.
- Endpoints de búsqueda respondiendo con datos reales por HTTPS.

## Pendientes

- Importar los cores restantes: Node, Layout, LayoutInstance, LayoutInstanceByNode, URLRedirect, User, Widget, Pages, PrintEdition, ProgrammingGuide, NewsVersion (chicos) y **Gallery / Media / News** (pesados, en background).
- Restaurar `assetviewscounts` en Mongo para que `GetMostRead` deje de dar 204.
- Core `Theme` sin config en el backup de Solr (solo existe el SP en SQL) — decidir si se clona de otro core.

## Lecciones aprendidas (evitar repetir errores)

- **PowerShell ↔ bash**: escribir scripts `.ps1`/`.sh` en archivo y transferirlos (scp). Las comillas anidadas en `ssh "..." -Command "..."` comen `$variables`. En el droplet NO existe PowerShell (todo bash).
- **sqlcmd contra el contenedor**: no usar `-Q` con comillas anidadas. Escribir el `.sql`, `scp` al droplet, `docker cp` dentro de `sqlserver` y ejecutarlo con `-i archivo.sql`.
- **AutoMapper encadenado** (`CreateMap().ForAllMembers(...)`) ya no compila: ver [AUTOMAPPER.md](AUTOMAPPER.md). No reintroducir el patrón.
- Si un archivo aparece a medias en el droplet tras un `scp` con caracteres extraños, re-transferirlo completo.