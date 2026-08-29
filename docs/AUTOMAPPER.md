# Migración AutoMapper 10.1.1 → 13.0.1

## Por qué

El proyecto saltó a .NET 10 y las APIs en runtime devolvían **HTTP 500** en los endpoints que usaban perfiles de mapeo. Al actualizar AutoMapper a **13.0.1**, el método encadenable de las versiones viejas dejó de compilar:

```csharp
// ANTES (AutoMapper ≤ 10): devolvía el MappingExpression → permitía encadenar
CreateMap<News, NewsDto>().ForMember(x => x.Id, o => o.Ignore())
                          .ForAllMembers(x => x.Ignore());
```

```csharp
// AHORA (AutoMapper 13.0.1): ForAllMembers es void → No retorna la expresión
CreateMap<News, NewsDto>().ForMember(x => x.Id, o => o.Ignore())
                          .ForAllMembers(x => x.Ignore());   // ❌ ya no compila
```

Además solo había un `.ForAllMembers(x => x.Ignore())` (no compilaba porque `CreateMap` volvía a devolverse y `ForMember` seguía encadenado) y se usaron variables `m1` repetidas → **CS0128** (variable ya declarada) y cascada de **CS1061**.

## La forma que quedó (equivalente exacto)

Cada mapa se declara en una variable propia y se configuran las opciones **en llamadas separadas**, jamás encadenadas:

```csharp
var m1 = CreateMap<News, NewsDto>();
m1.ForAllMembers(x => x.Ignore());      // todo se ignora
m1.ForMember(x => x.Id, o => o.Ignore());
m1.ForMember(x => x.PublicationDate, o => o.MapFrom(src => src.PublicationDate));
```

Reglas de oro:

1. `ForAllMembers(x => x.Ignore())` **siempre solo** en su propia línea, después del `CreateMap(...)`.
2. Cada mapa usa una variable **única** (`m1, m2, m3, ...`) por archivo → se renumeran por archivo para evitar CS0128.
3. `CreateMap<A, B>().ForAllMembers(...)` encadenado queda **prohibido**.

## Archivos tocados (17 perfiles en `Ray.Managers/MapperProfiles/`)

`AuthorProfile, CategoryProfile, GalleryProfile, KeywordProfile, LayoutInstanceProfile, LayoutProfile, MediaProfile, NewsProfile, NewsSourceProfile, NodeProfile, PageProfile, ProgrammingGuideProfile, RoleProfile, TemplateProfile, ThemeProfile, URLRedirectProfile, UserProfile`

## Verificación local

El estado correcto se valida así (PowerShell):

```powershell
# 1) No debe quedar ninguna cadena encadenada rota:
#    ningún "CreateMap<T,U>().ForAllMembers(..." ni ".ForAllMembers(...).ForMember(...".
rg -n "CreateMap\(<[^)]+\)>?\s*\.\s*ForAllMembers|ForAllMembers\([^)]*\)\s*\.[A-Za-z]" Ray.Managers/MapperProfiles
#    resultado esperado: sin matches (los ForAllMembers van en línea propia).

# 2) En cada archivo, las variables `m#` deben ser únicas y el recuento
#    de `var m# = CreateMap(...)` debe coincidir con el de archivo (1+ mapas).
```

Referencia rápida de contadores por perfil (resultado del arreglo `renumber_mapper.ps1`):
`Author=1, Category=1, Gallery=1, Keyword=1, Layout=1, Media=2, News=2, NewsSource=1, Node=2, Page=2, ProgrammingGuide=2, Role=1, Template=1, Theme=2, URLRedirect=2, User=1`, `LayoutInstance=0`.

## Despliegue y prueba

```powershell
scp "Ray.Managers\MapperProfiles\*.cs" root@159.223.183.214:/root/api-fe/Ray.Managers/MapperProfiles/
ssh root@159.223.183.214 "cd /root/api-fe && docker compose up -d --build frontendapi backendapi"
```

Comprobación:

- `/api/Account/UserInfo` en apibe → **401** (JWT sano; el 401 es solo falta de token, no un crash).
- Endpoints de datos: `/api/Author/GetById/<id>`, `/api/News/GetById/<id>`, etc. → **200/404 esperados** (ya no 500).
- Logs de `frontendapi`/`backendapi` limpios (sin stack traces de AutoMapper).

## Herramientas usadas en la migración

- `fix_automapper.ps1` — transforma las cadenas rotas a la forma `var m# = CreateMap(...)` + llamadas separadas.
- `renumber_mapper.ps1` — renombra `m1,m2,...` por archivo de forma secuencial (elimina duplicados).
- `verify_mapper.ps1` — cuenta `CreateMap`s vs. `var m#` por archivo y detecta cadenas encadenadas residuales.
- Transferencia: con `scp`; builds únicamente en el droplet (SDK local es 9, `global.json` pide 10.0.301).