# Solr incident checklist

Objetivo: que un recreate/deploy de Solr no borre indices, que los delta-import no fallen por columnas inexistentes, y que el flujo sea repetible.

## Estado preparado en repo

- [x] Mantener Solr en `8.4.0`.
- [x] Corregir la doc de `deploy-solr.ps1` que mencionaba `8.11.4`.
- [x] Declarar volumen Docker `solr-data` con nombre real `api-fe-solr-data`.
- [x] Montar `api-fe-solr-data` en `/var/solr`.
- [x] Apuntar cada core a `/var/solr/data/<core>` mediante `solr.data.dir` en `core.properties`.
- [x] Agregar backup best-effort de indices actuales antes de recrear Solr.
- [x] Migrar indices actuales desde el backup al volumen persistente `api-fe-solr-data`.
- [x] Validar en deploy que `/var/solr` quede montado.
- [x] Corregir delta de `Menu`, que usaba `Menu.CacheSolr` aunque la tabla no tiene esa columna.

## Antes de tocar produccion

- [ ] Hacer backup manual de los indices actuales:
  - Origen actual: `/opt/solr/server/solr/*/data`
  - Destino sugerido: `/root/api-fe/solr-backups/`
- [ ] Hacer backup del `docker-compose.yml` actual del droplet.
- [ ] Confirmar ventana de reindexado.
- [ ] Confirmar que se quiere conservar el indice actual y no hacer full-import inicial.

## Deploy seguro

- [ ] Subir cambios de Solr al repo.
- [ ] Ejecutar `deploy/deploy-solr.ps1`.
- [ ] Confirmar que el contenedor `solr` esta `Up`.
- [ ] Confirmar que `/var/solr` esta montado:
  - `docker inspect solr --format '{{json .Mounts}}'`
- [ ] Confirmar que los cores escriben data en `/var/solr/data/<core>`.
- [ ] Confirmar que los docs actuales siguen disponibles sin reindexar.

## Reindexado opcional

- [ ] Solo si la migracion de indices falla o si se decide reconstruir desde SQL: ejecutar full-import para cores principales:
  - `News`
  - `Pages`
  - `Media`
  - `Node`
  - `Author`
  - `Category`
  - `Keywords`
  - `Gallery`
  - `LayoutInstance`
  - `User`
  - `Menu`
- [ ] Verificar busquedas en frontend/admin.
- [ ] Publicar o editar una nota de prueba.
- [ ] Confirmar que el delta-import de `News` no falla.

## Pendientes de fondo

- [ ] Resolver `NewsVersion`: parece legacy/no usado por el codigo actual, pero su `db-data-config.xml` no coincide con la tabla real `News`.
- [ ] Sacar credenciales SQL hardcodeadas de `db-data-config.xml`.
- [ ] Crear usuario SQL especifico para Solr con permisos minimos.
- [ ] Rotar password de `sa` despues de sacar credenciales del repo.
