# Gestão de Pedidos — Avaliação Técnica (Clean Architecture + CQRS + DDD + SQL Server + MongoDB + Angular)

Implementação completa do teste descrito em `prova-técnica-gestão-pedidos.pdf`:
- **Back-end**: Web API RESTful em **.NET 10** (".NET Core 5 ou superior"), com **Clean Architecture**, **CQRS**, **DDD**, **SOLID**, **KISS/DRY**.
- **Write Model**: **SQL Server** (EF Core).
- **Read Model**: **MongoDB** (MongoDB.Driver).
- **Sincronização SQL → Mongo**: **Outbox Pattern** + worker (HostedService) com **Dapper**.
- **Front-end**: **Angular 21** (SPA) com rotas simples para listar/criar/editar/detalhar pedidos.
- Tudo sobe via **Docker Compose**.

## Como rodar (Docker)

Pré-requisitos:
- Docker + Docker Compose

Subir tudo:
```bash
docker compose up --build
```

Acessos:
- Frontend: http://localhost:4200
- Swagger (API): http://localhost:8080/swagger
- Health: http://localhost:8080/health

> Observação: o read-model (Mongo) é atualizado quase imediatamente via worker do Outbox (polling a cada ~1s).

## Regras do teste (aplicadas)

- **Customer fixo** (todos os pedidos são do mesmo cliente).
- **Tabela de produtos com apenas 3 opções**.

IDs seedados:
- CustomerId fixo: `11111111-1111-1111-1111-111111111111`
- Produtos:
  - 22222222-2222-2222-2222-222222222222 — Camiseta — R$ 59.90
  - 33333333-3333-3333-3333-333333333333 — Calça — R$ 129.90
  - 44444444-4444-4444-4444-444444444444 — Tênis — R$ 249.90

## Endpoints principais

- `GET /api/orders` — lista pedidos (Mongo)
- `GET /api/orders/{id}` — detalha pedido (Mongo)
- `POST /api/orders` — cria pedido (SQL + Outbox)
- `PUT /api/orders/{id}` — atualiza pedido (SQL + Outbox)
- `DELETE /api/orders/{id}` — remove pedido (SQL + Outbox)
- `GET /api/products` — lista produtos (SQL)

## Estrutura (Clean Architecture)

```
backend/
  src/
    Orders.Domain
    Orders.Application
    Orders.Infrastructure
    Orders.Api
    Orders.Tests
frontend/
docker-compose.yml
```

## Notas de qualidade

- **DDD**: `Order` como Aggregate Root + regras/invariantes no domínio.
- **CQRS**: Commands escrevem no SQL; Queries leem apenas do Mongo.
- **Validation Pipeline**: FluentValidation via MediatR Behavior (Controllers limpos).
- **Error Handling**: Middleware global com ProblemDetails.
- **Outbox**: garante consistência eventual e robustez entre write/read models.

