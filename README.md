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

Atuo como Engenheiro de Software Full-Stack Sênior, com mais de 10 anos de experiência em desenvolvimento de software, participando da análise de requisitos, definição de soluções técnicas, arquitetura, desenvolvimento e sustentação de aplicações corporativas de médio e grande porte, especialmente nos segmentos financeiro, bancário, seguros e meios de pagamento.

Atualmente, na GFT, atuo em projetos do Banco Itaú, participando da arquitetura e desenvolvimento de aplicações relacionadas ao gerenciamento de regras de negócio, com abrangência nacional. Minha atuação envolve o desenvolvimento e evolução de soluções utilizando .NET Core/.NET 8, C#, Angular (versões 18, 20 e 21), SQL Server, MongoDB, AWS, microsserviços, mensageria e GitHub Actions, aplicando conceitos de arquitetura distribuída, DDD, SOLID, Design Patterns e boas práticas de engenharia de software.

Possuo experiência na construção e manutenção de APIs REST, desenvolvimento de serviços de backend, implementação de regras de negócio, integração entre sistemas, manipulação de dados e desenvolvimento de interfaces web. Participo da análise e definição de soluções técnicas, considerando requisitos funcionais e não funcionais, organização dos componentes, escalabilidade, desempenho, segurança, manutenibilidade e aderência aos padrões arquiteturais estabelecidos.

No desenvolvimento backend, tenho sólida experiência com C#, ASP.NET, ASP.NET Core, Web APIs, Entity Framework e Dapper, utilizando bancos de dados relacionais e não relacionais, principalmente SQL Server e MongoDB, além de experiência com Oracle e MySQL. Trabalho com modelagem de dados, implementação de regras de domínio, consultas e otimização de acesso aos dados, buscando soluções robustas e de fácil manutenção.

No frontend, possuo experiência com Angular, React e Vue.js, além de JavaScript, TypeScript, HTML, CSS e Bootstrap. Atuo no desenvolvimento de aplicações web corporativas, integração com APIs, implementação de componentes, formulários, validações e funcionalidades orientadas às necessidades de negócio, contribuindo tanto para a construção de novas interfaces quanto para a evolução de aplicações existentes.

Também possuo experiência com arquitetura de software e padrões de desenvolvimento, aplicando conceitos como Domain-Driven Design (DDD), CQRS, MVC, SOLID, Design Patterns, TDD e BDD, buscando promover baixo acoplamento, separação de responsabilidades, reutilização de componentes, testabilidade e qualidade de código. Participo de discussões e definições técnicas, análise de impactos, estimativas de desenvolvimento e identificação de melhorias arquiteturais.

Na área de qualidade de software, atuo na implementação e manutenção de testes unitários e na aplicação de práticas de desenvolvimento que contribuem para a prevenção de defeitos, redução de débito técnico, melhoria da manutenibilidade e confiabilidade das aplicações. Tenho experiência em ambientes que utilizam processos ágeis, especialmente Scrum, participando de refinamentos, planejamentos, estimativas, desenvolvimento, revisão e entrega de funcionalidades.

Possuo conhecimentos em serviços e soluções de computação em nuvem, especialmente AWS, associados ao desenvolvimento de aplicações distribuídas, microsserviços e mensageria. Também tenho familiaridade com tecnologias e ferramentas do ecossistema Azure e com práticas de integração e entrega contínua, utilizando ferramentas como GitHub Actions.

Ao longo da minha trajetória profissional, atuei em diferentes frentes do Banco Itaú, incluindo sistemas de gerenciamento de regras de negócio, aplicações para o setor de seguros e soluções corporativas voltadas à gestão de desempenho de colaboradores e executivos. Também participei de projetos relacionados a meios de pagamento, automação de processos, desenvolvimento de novos produtos e modernização de aplicações, adquirindo experiência em diferentes contextos de negócio e arquiteturas tecnológicas.

Tenho experiência anterior de aproximadamente cinco anos com análise de requisitos, o que contribui para minha capacidade de compreender necessidades de negócio, identificar impactos entre sistemas, apoiar decisões técnicas e estabelecer uma comunicação eficiente entre equipes de desenvolvimento, arquitetura, áreas de negócio e demais envolvidos nos projetos.

Sou graduado em Sistemas de Informação pela FIAP, possuo MBA em Arquitetura de Soluções pela FIAP e pós-graduação em Gerenciamento de Projetos pela Fundação Getulio Vargas (FGV). Também realizei formações complementares em arquitetura de software, modelagem de domínios ricos, microsserviços e testes de software.

Meu perfil combina uma forte atuação técnica em desenvolvimento backend e frontend com visão arquitetural e entendimento de negócio. Tenho facilidade para atuar em diferentes etapas do ciclo de desenvolvimento de software, desde o levantamento e análise de requisitos até a implementação, integração, evolução e sustentação de soluções, contribuindo para equipes multidisciplinares e projetos que demandem experiência técnica, qualidade, colaboração e alinhamento às necessidades dos clientes.

