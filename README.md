# API de ventas de autos · Prueba técnica .NET COTO

[![CI](https://github.com/MartuPe/coto-car-sales-api/actions/workflows/ci.yml/badge.svg)](https://github.com/MartuPe/coto-car-sales-api/actions/workflows/ci.yml)

API REST en **.NET 10** para una fábrica de autos con 4 modelos y 4 centros de distribución. Permite
registrar ventas y consultar el volumen de ventas total, el volumen por centro y el porcentaje de
unidades de cada modelo vendido en cada centro sobre el total. Los datos están mockeados en memoria.

- **Arquitectura:** Clean Architecture en 4 proyectos (Domain, Application, Infrastructure, Api).
- **Patrones:** Repository, CQRS liviano, Decorator (tiempos de ejecución), Strategy (impuestos) y
  Factory method.
- **Calidad:** 84 tests (xUnit), cobertura de 99 % con un mínimo exigido de 80 %, reglas de SonarQube
  en cada build y CI en GitHub Actions.

## Índice

1. [Cómo ejecutar y probar](#cómo-ejecutar-y-probar)
2. [Cómo usar los servicios](#cómo-usar-los-servicios)
3. [Estructura del proyecto](#estructura-del-proyecto)
4. [Decisiones técnicas](#decisiones-técnicas)
5. [Qué quedó afuera y cómo lo resolvería](#qué-quedó-afuera-y-cómo-lo-resolvería)
6. [Pruebas](#pruebas)
7. [Calidad de código y convenciones](#calidad-de-código-y-convenciones)
8. [Estimación y tiempo real](#estimación-y-tiempo-real)
9. [Uso de IA](#uso-de-ia)

---

## Cómo ejecutar y probar

**Requisito:** [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0).

```bash
# Levantar la API en http://localhost:5080 (abre Swagger UI)
dotnet run --project src/CarSales.Api

# Correr los 84 tests con cobertura (falla si baja de 80 %)
dotnet test
```

Con la API levantada hay tres formas de probar los servicios:

- **Swagger UI** en <http://localhost:5080/swagger> (la raíz `/` redirige ahí), con la descripción de
  cada servicio y un botón *Try it out*.
- El archivo [`src/CarSales.Api/CarSales.Api.http`](src/CarSales.Api/CarSales.Api.http), con un pedido
  de ejemplo por servicio, para Visual Studio, Rider o la extensión REST Client de VS Code.
- `curl`, con los ejemplos de la sección siguiente.

Los datos viven en memoria: al reiniciar la API vuelven a los datos mockeados originales.

---

## Cómo usar los servicios

| Método | Ruta                                  | Qué hace                                                    |
| ------ | ------------------------------------- | ----------------------------------------------------------- |
| POST   | `/api/sales`                          | Registra una venta.                                         |
| GET    | `/api/sales/volume`                   | Volumen de ventas total.                                    |
| GET    | `/api/sales/volume/by-center`         | Volumen de ventas de cada centro.                           |
| GET    | `/api/sales/volume/by-center/{id}`    | Volumen de ventas de un centro.                             |
| GET    | `/api/sales/model-share-by-center`    | Porcentaje de unidades de cada modelo en cada centro.       |

### Datos mockeados

| Modelo  | Precio de lista (u$s) | Impuesto extra | Total por unidad (u$s) |
| ------- | --------------------: | -------------- | ---------------------: |
| Sedan   |              8.000,00 | —              |               8.000,00 |
| SUV     |              9.500,00 | —              |               9.500,00 |
| Offroad |             12.500,00 | —              |              12.500,00 |
| Sport   |             18.200,00 | 7 % (1.274,00) |              19.474,00 |

Centros de distribución: **1** Buenos Aires, **2** Córdoba, **3** Rosario y **4** Mendoza. La API
arranca con 19 ventas de septiembre de 2026 que suman **50 unidades**, así los reportes tienen datos
desde la primera consulta.

### Registrar una venta

```bash
curl -X POST http://localhost:5080/api/sales \
  -H "Content-Type: application/json" \
  -d '{ "distributionCenterId": 1, "model": "Sport", "quantity": 2 }'
```

`201 Created`:

```json
{
  "id": "01a0d40c-b2cf-799a-ad8e-0218f86d19c9",
  "distributionCenterId": 1,
  "distributionCenterName": "Centro Buenos Aires",
  "model": "Sport",
  "quantity": 2,
  "unitPrice": 18200.00,
  "netAmount": 36400.00,
  "taxAmount": 2548.00,
  "totalAmount": 38948.00,
  "soldAt": "2026-09-24T15:33:17.1358208+00:00"
}
```

- `model` no distingue mayúsculas (`sport` también sirve) y `quantity` va de 1 a 1000.
- La fecha de la venta (`soldAt`) se guarda y se informa en UTC.
- La respuesta detalla el monto neto, el impuesto y el total.

### Volumen de ventas

```bash
curl http://localhost:5080/api/sales/volume
curl http://localhost:5080/api/sales/volume/by-center
curl http://localhost:5080/api/sales/volume/by-center/1
```

El volumen se informa en **unidades y en dólares**, con el impuesto separado:

```json
{ "units": 50, "netAmount": 542200.00, "taxAmount": 7644.00, "totalAmount": 549844.00 }
```

Por centro, cada elemento trae el centro y su volumen. Los centros sin ventas aparecen en cero:

```json
[
  {
    "distributionCenterId": 1,
    "distributionCenterName": "Centro Buenos Aires",
    "volume": { "units": 15, "netAmount": 145200.00, "taxAmount": 1274.00, "totalAmount": 146474.00 }
  }
]
```

### Porcentaje de cada modelo por centro

```bash
curl http://localhost:5080/api/sales/model-share-by-center
```

```json
{
  "totalUnits": 50,
  "centers": [
    {
      "distributionCenterId": 1,
      "distributionCenterName": "Centro Buenos Aires",
      "models": [
        { "model": "Sedan", "units": 8, "percentage": 16 },
        { "model": "SUV", "units": 4, "percentage": 8 },
        { "model": "Offroad", "units": 2, "percentage": 4 },
        { "model": "Sport", "units": 1, "percentage": 2 }
      ]
    }
  ]
}
```

Devuelve la matriz completa de 4 centros × 4 modelos, incluidas las combinaciones sin ventas (0 %).

### Errores

Los errores siguen el formato **ProblemDetails** (RFC 9457):

| Caso                                      | Código | Ejemplo de `detail` / `errors`                                           |
| ----------------------------------------- | :----: | ------------------------------------------------------------------------ |
| Campos inválidos o faltantes              |  400   | `"Quantity": ["La cantidad tiene que estar entre 1 y 1000."]`            |
| Centro o modelo inexistente en la venta   |  400   | `"No existe el modelo 'Coupe'. Modelos válidos: Sedan, SUV, Offroad, Sport."` |
| Centro inexistente en la URL              |  404   | `"No existe el centro de distribución 9."`                              |
| Ruta inexistente / método no permitido    | 404 / 405 | Título `"Recurso no encontrado"` / `"Método no permitido"`            |

El título (`title`) de cada error está en español. Si el JSON no se puede leer (por ejemplo, un
texto en un campo numérico), el detalle técnico lo genera .NET en inglés e indica el campo con
error (`$.quantity`).

### Tiempo de ejecución

Cada llamada imprime en la consola de la API **dos tiempos**: el del caso de uso (la lógica de
negocio) y el del request completo (incluye routing, validación y serialización):

```text
12:31:35 info: CarSales.Application.Diagnostics.ExecutionTimer[1] RegisterSaleCommandHandler.HandleAsync se ejecutó en 2.020 ms
12:31:35 info: CarSales.Api.Diagnostics.RequestTimingMiddleware[2] HTTP POST /api/sales respondió 201 en 22.641 ms
12:31:35 info: CarSales.Application.Diagnostics.ExecutionTimer[1] GetTotalSalesVolumeQueryHandler.HandleAsync se ejecutó en 0.073 ms
12:31:35 info: CarSales.Api.Diagnostics.RequestTimingMiddleware[2] HTTP GET /api/sales/volume respondió 200 en 0.889 ms
```

La primera llamada a cada servicio tarda más porque .NET compila el código en ese momento (JIT).

---

## Estructura del proyecto

```text
coto-car-sales-api/
├── src/
│   ├── CarSales.Domain/            Reglas de negocio. No depende de nada.
│   │   ├── Catalog/                CarModel y DistributionCenter
│   │   ├── Taxes/                  ITaxPolicy, NoExtraTax y PercentageTax (Strategy)
│   │   ├── Sales/                  Sale (factory method) y SalesVolume
│   │   └── Common/                 DomainException, Money (redondeo) y Percentage
│   ├── CarSales.Application/       Casos de uso. Depende solo de Domain.
│   │   ├── Abstractions/           ICommandHandler, IQueryHandler e interfaces de repositorios
│   │   ├── Sales/RegisterSale/     Comando «Insertar una venta»
│   │   ├── Reports/                Una carpeta por consulta (total, por centro, un centro, % por modelo)
│   │   ├── Diagnostics/            ExecutionTimer y los decorators que miden los tiempos
│   │   └── Common/                 DTOs compartidos y ResourceNotFoundException
│   ├── CarSales.Infrastructure/    Repositorios en memoria y datos mockeados
│   └── CarSales.Api/               Controller, contrato de entrada, errores, middleware y Program.cs
├── tests/
│   ├── CarSales.UnitTests/         Dominio, casos de uso (con repositorios falsos) e infraestructura
│   └── CarSales.IntegrationTests/  Servicios REST de punta a punta y reglas de arquitectura
├── .github/workflows/ci.yml        Build, tests y cobertura en cada push
├── Directory.Build.props           Configuración común: .NET 10, nullable, warnings como errores
└── Directory.Packages.props        Versiones de NuGet centralizadas y analizador de SonarQube
```

Las dependencias apuntan hacia adentro. `Api` es el único proyecto que conoce a todos, porque arma
la aplicación:

```text
CarSales.Api ──────────────► CarSales.Application ──► CarSales.Domain
      │                              ▲
      └──► CarSales.Infrastructure ──┘  (implementa las interfaces de Application)
```

Recorrido de un `POST /api/sales`:

```text
RequestTimingMiddleware → SalesController → TimedCommandHandler (decorator)
  → RegisterSaleCommandHandler → repositorios + Sale.Create (usa la ITaxPolicy del modelo)
  → InMemorySaleRepository
```

---

## Decisiones técnicas

### Arquitectura: Clean Architecture en 4 proyectos

Elegí separar las capas en proyectos, y no en carpetas de un único proyecto, porque así **el
compilador impide saltarse capas**: Domain no puede usar nada de Application porque no tiene la
referencia. `ArchitectureTests` deja la regla escrita en un test.

Adentro de Application, los casos de uso se agrupan por funcionalidad (`Sales/RegisterSale`,
`Reports/GetTotalSalesVolume`, …) y no por tipo de clase: todo lo de un caso de uso queda junto.

Alternativas que evalué:

- **Vertical Slice:** un proyecto con un archivo por caso de uso. Es muy cohesiva, pero no separa las
  reglas de negocio de la infraestructura, que es lo que más importa si mañana los datos vienen de
  una base real.
- **N-capas clásico** (Controllers → Services → Repositories): más simple, pero la lógica de
  negocio tiende a quedar repartida en servicios con muchas responsabilidades.
- **Monolito modular:** tiene sentido con varios contextos de negocio (ventas, stock, logística). Acá
  hay uno solo, así que dividirlo en módulos habría sido forzarlo.

### Patrones utilizados

| Patrón | Dónde | Por qué |
| ------ | ----- | ------- |
| **Repository** | Interfaces en `Application/Abstractions/Persistence`, implementaciones `InMemory*` en Infrastructure | Los casos de uso no saben de dónde salen los datos. Pasar a una base real es cambiar Infrastructure, sin tocar la lógica. También permite testear los casos de uso con repositorios falsos. |
| **CQRS liviano** | `ICommandHandler` (escribe) e `IQueryHandler` (lee) | Cada caso de uso es una clase chica con una sola responsabilidad. Que todos compartan una interfaz es lo que permite aplicarles un único decorator. Sin MediatR ni bases separadas: a esta escala no aportan. |
| **Decorator** | `TimedCommandHandler` y `TimedQueryHandler` | Imprimen el tiempo de ejecución envolviendo al caso de uso, sin escribir código de medición en cada método (responsabilidad única, abierto/cerrado). Se aplican a todos los casos de uso con dos líneas en `AddApplication`. |
| **Strategy** | `ITaxPolicy` → `NoExtraTax` y `PercentageTax` | Cada modelo tiene asignada su política de impuesto y la venta la usa sin saber cuál es. No hay un `if (modelo == "Sport")`: una regla nueva (por escalas, por provincia) es una clase nueva. |
| **Factory method** | `Sale.Create` | Es la única forma de crear una venta: valida la cantidad y calcula los montos, así no puede existir una venta inconsistente. |
| **Inyección de dependencias** | `AddApplication()` y `AddInfrastructure()` | Cada capa registra lo suyo y `Program.cs` solo las compone. |

### Cómo se imprime el tiempo de ejecución

La consigna pide imprimir el tiempo de cada método. Poner un `Stopwatch` adentro de cada método
repite código y lo mezcla con la lógica de negocio. En su lugar:

- **Decorator sobre los casos de uso:** mide la lógica de negocio. `ExecutionTimer` usa
  `Stopwatch.GetTimestamp()` y un log generado en compilación (`LoggerMessage`). El tiempo se
  imprime también si el caso de uso falla.
- **Middleware (`RequestTimingMiddleware`):** mide el request HTTP completo. Va primero en el
  pipeline para registrar el código de respuesta final, incluidos los errores.

La diferencia entre los dos tiempos muestra cuánto se va en la infraestructura web y cuánto en la
lógica.

### Interpretación de la consigna

- **«Volumen de ventas»** puede leerse como unidades o como dinero, así que la API informa las dos
  cosas: unidades y montos en dólares, con el neto, el impuesto y el total por separado.
- **«Porcentaje de unidades de cada modelo en cada centro sobre el total de ventas»:** el
  denominador es el **total de unidades de la empresa** (lectura literal de «sobre el total»). Las
  16 celdas centro × modelo suman 100 %. Por el redondeo a 2 decimales la suma puede diferir en
  centésimos: con 53 unidades da 99,97 %.
- **Una venta puede tener varias unidades** (`quantity`), porque la consigna habla de unidades.
- **El precio queda congelado en la venta:** cada venta guarda el precio y el impuesto vigentes al
  momento de venderse, así un cambio de precio posterior no altera las ventas ni los reportes ya
  registrados.
- **Volumen por centro:** se ofrece la lista de todos los centros y también la consulta de uno solo.

### Montos y redondeo

- Los montos usan `decimal` (nunca `double`, que acumula errores de representación con el dinero).
- El impuesto se redondea a 2 decimales con **redondeo comercial**
  (`MidpointRounding.AwayFromZero`: 0,125 → 0,13). `Math.Round` usa por defecto el redondeo
  bancario (0,125 → 0,12), que no corresponde para montos facturados.
- Todas las respuestas informan los montos con 2 decimales.

### Validaciones y errores

- **Entrada:** `RegisterSaleRequest` valida con DataAnnotations y mensajes en español. Sus
  propiedades son `required`, así un campo faltante da 400 en lugar de convertirse en un 0 silencioso.
- **Negocio:** el dominio lanza `DomainException` (400) y las consultas lanzan
  `ResourceNotFoundException` (404).
- **Un solo lugar para traducir errores:** `ApiExceptionHandler` (`IExceptionHandler` de ASP.NET
  Core) arma las respuestas ProblemDetails, así ni los controllers ni los casos de uso tienen
  try/catch. Cualquier otro error es un 500 sin detalles internos.
- **Títulos en español y un único formato:** `ProblemDetailsTitles` define los títulos por código de
  estado y se aplica a todas las respuestas de error, también a las que genera ASP.NET Core (validación,
  404, 405, 415). Con `UseStatusCodePages`, los errores sin cuerpo también salen como ProblemDetails.

### Concurrencia

Los repositorios son singleton (los datos tienen que sobrevivir entre requests) y ASP.NET Core
atiende requests en paralelo. Por eso `InMemorySaleRepository` protege su lista con un `lock` y
`GetAllAsync` devuelve una copia. Un test inserta 1.000 ventas en paralelo y verifica que no se pierda
ninguna.

### Herramientas

| Herramienta | Por qué |
| ----------- | ------- |
| **.NET 10** | Es la versión LTS actual (soporte hasta noviembre de 2028). |
| **Controllers** | Agrupan los servicios de ventas en una clase y documentan bien las respuestas en OpenAPI. |
| **OpenAPI nativo + Swagger UI** | Desde .NET 9 el documento OpenAPI lo genera ASP.NET Core; Swagger UI solo lo muestra. Las descripciones salen de los comentarios XML (`///`). |
| **Scrutor** | Registra los casos de uso por convención y aplica los decorators. |
| **`TimeProvider`** | Reloj inyectable de .NET: los tests fijan la fecha de las ventas. |
| **xUnit + coverlet** | Tests y cobertura con umbral mínimo en cada `dotnet test`. |
| **SonarAnalyzer.CSharp** | Reglas de SonarQube en cada build (ver [Calidad](#calidad-de-código-y-convenciones)). |
| **Central Package Management** | Todas las versiones de NuGet en `Directory.Packages.props`. |

**Descartados a propósito:** **MediatR** y **AutoMapper**. Para cinco casos de uso no hace falta un
mediador, y el mapeo manual (`SaleDto.From`) es explícito y fácil de seguir. Además, los dos pasaron
a licencia comercial en 2025.

**xUnit 2.9 y no xUnit v3:** xUnit v3 corre sobre Microsoft.Testing.Platform, la nueva plataforma
de tests de .NET. Ahí coverlet recolecta la cobertura pero no aplica un umbral, y la consigna pide un
mínimo de 80 %: con xUnit 2.9 y coverlet.msbuild, `dotnet test` falla si no se cumple.

---

## Qué quedó afuera y cómo lo resolvería

La consigna no pide estas cosas. Agregarlas habría sumado complejidad sin aportar a lo que se
evalúa, pero así las encararía en un sistema real:

- **Autenticación con JWT.** El token lo emite un proveedor de identidad (Cognito, Entra ID,
  Keycloak) y no esta API. La API lo valida con `AddAuthentication().AddJwtBearer(...)` configurando
  el `Authority` del emisor: el middleware verifica la **firma** con las claves públicas del emisor
  (JWKS), el emisor (`iss`), la audiencia (`aud`) y el vencimiento (`exp`) **antes** de llegar al
  controller. Recién después se leen los claims, por ejemplo un rol que permita registrar ventas.
- **Base de datos real** (Oracle o PostgreSQL con EF Core): se implementan los tres repositorios en
  Infrastructure y se cambia `AddInfrastructure`, sin tocar los casos de uso. Los reportes pasarían
  a agregar en la base (`GROUP BY`) con un repositorio de lectura, en lugar de traer todas las
  ventas a memoria como hoy.
- **Caché de reportes:** `HybridCache` (.NET 9+) con Redis como segundo nivel compartido entre
  instancias, invalidando la entrada al registrar una venta. Con los datos en memoria no aporta.
- **Eventos:** al registrar una venta se podría publicar un evento `SaleRegistered` (Kafka o
  RabbitMQ) para que otros sistemas (stock, facturación, logística) reaccionen sin acoplarse a
  esta API.
- **Despliegue:** Dockerfile multi-etapa (SDK para compilar, runtime de ASP.NET Core para correr)
  y health checks para las sondas de Kubernetes.
- **Versionado de la API** (`/api/v1/...`), cuando haya más de un consumidor.

---

## Pruebas

```bash
dotnet test
```

**84 tests** en dos proyectos:

| Proyecto | Tests | Qué prueba |
| -------- | :---: | ---------- |
| `CarSales.UnitTests` | 60 | Dominio (precios, impuesto, redondeo, validaciones), casos de uso con repositorios falsos, decorators de tiempos, registro de dependencias y repositorios en memoria (incluidas 1.000 inserciones en paralelo). |
| `CarSales.IntegrationTests` | 24 | Cada servicio REST de punta a punta con `WebApplicationFactory` (cada test levanta su propia API con los datos mockeados originales), errores 400, 404, 405 y 415, logs de tiempos y reglas de arquitectura. |

**Cobertura:** `dotnet test` mide con coverlet y **falla si las líneas o las ramas cubiertas
bajan de 80 %**.

| Proyecto de tests | Líneas | Ramas |
| ----------------- | :----: | :---: |
| Unitarios         | 99,6 % | 100 %  |
| Integración       | 99,7 % | 91,7 % |

Se excluye el código que genera el compilador (logs de `LoggerMessage` y comentarios XML de OpenAPI).
Coverlet deja los reportes en cada proyecto de tests: `coverage.opencover.xml`, para SonarQube, y
`coverage.cobertura.xml`. Para verlos en HTML:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:coverage-report
```

---

## Calidad de código y convenciones

- **Reglas de SonarQube en cada build:** `SonarAnalyzer.CSharp` trae al compilador las reglas de
  SonarQube (bugs, vulnerabilidades y *code smells*). Con `TreatWarningsAsErrors`, un hallazgo
  rompe la compilación, igual que el *quality gate* de Sonar en un pipeline. Detectó, por ejemplo,
  un `app.Run()` que debía ser `await app.RunAsync()`.
- **CI:** GitHub Actions compila en Release y corre los tests con el umbral de cobertura en cada
  push y pull request.
- **Estilo:** `.editorconfig` (namespaces por archivo, campos privados con `_`), nullable
  habilitado y finales de línea LF (`.gitattributes`).
- **Idioma:** código en inglés; comentarios, mensajes de error, README y commits en español.
- **Commits:** [Conventional Commits](https://www.conventionalcommits.org/es/) en español, con un
  cuerpo que explica el porqué (`feat`, `fix`, `test`, `build`, `ci`, `docs`, `chore`). La historia
  sigue el orden en que se construyó: estructura → dominio → casos de uso → infraestructura → API →
  tiempos → calidad.

---

## Estimación y tiempo real

|                                                                        | Horas                          |
| ---------------------------------------------------------------------- | ------------------------------ |
| **Estimación inicial**                                                 | **10 a 12 h** (1,5 a 2 días)   |
| Análisis de la consigna y evaluación de alternativas de arquitectura   | ~0,5 h                         |
| Desarrollo por capas con sus tests                                     | ~2 h                           |
| Verificación con la API corriendo, CI y README                         | ~1 h                           |
| **Total real**                                                         | **~3,5 h** (24/9/2026)         |

La estimación contemplaba hacer todo sin asistencia. El tiempo real fue menor por el uso de un
asistente de IA, que se detalla en la sección siguiente.

---

## Uso de IA

Desarrollé la prueba con **Claude Code** (asistente de programación con IA) como par de trabajo.
Antes de escribir código evalué con él las alternativas de arquitectura, de medición de tiempos y de
modelado del impuesto, y elegí las que se describen en este documento según lo que puedo sostener y
defender. Revisé cada etapa, verifiqué los servicios con la API corriendo y me hago responsable de
cada línea. El detalle de los montos con 2 decimales, por ejemplo, salió de probar la API real, no
de los tests.
