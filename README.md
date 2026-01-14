# Venice Orders API

API para gerenciamento de pedidos - Teste Técnico para Desenvolvedor Backend .NET Sênior.

## Requisitos

- Docker e Docker Compose

## Como Executar

```bash
docker compose up --build
```

A API estará disponível em: http://localhost:5000

Swagger UI: http://localhost:5000

---

## Design Pattern Adotado

### Padrão Arquitetural: Clean Architecture + CQRS

**Escolhi a combinação de Clean Architecture com CQRS** como padrão principal do projeto.

#### Justificativa da Escolha

1. **Clean Architecture** foi escolhida porque:
   - **Testabilidade**: As regras de negócio no Domain não dependem de frameworks externos, facilitando testes unitários
   - **Independência de tecnologia**: Posso trocar SQL Server por PostgreSQL ou RabbitMQ por Kafka sem alterar o domínio
   - **Manutenibilidade**: Código organizado em camadas com responsabilidades claras
   - **Escalabilidade**: Cada camada pode evoluir independentemente

2. **CQRS** foi escolhido porque:
   - **Otimização separada**: Leituras (com cache Redis) e escritas (com eventos RabbitMQ) têm necessidades diferentes
   - **Clareza de intenção**: Commands (`CreateOrderCommand`) e Queries (`GetOrderByIdQuery`) deixam explícito o que cada operação faz
   - **Facilita Event Sourcing futuro**: A estrutura já está preparada para evoluir se necessário

#### Outros Patterns Complementares

| Pattern | Aplicação | Benefício |
|---------|-----------|-----------|
| **Repository** | `IOrderRepository`, `IOrderItemRepository` | Abstrai persistência, permite trocar banco sem impacto |
| **Domain Events** | `OrderCreatedEvent` | Desacoplamento entre criação e notificação |
| **Cache-Aside** | `GetOrderByIdQueryHandler` | Performance em leituras frequentes |
| **Dependency Injection** | Todas as camadas | Baixo acoplamento, alta coesão |

---

## Arquitetura

### Clean Architecture

O projeto segue os princípios da **Clean Architecture**, organizando o código em camadas com dependências apontando para o centro:

```
┌─────────────────────────────────────────────┐
│                    API                       │
│  (Controllers, Configuration, Middleware)    │
├─────────────────────────────────────────────┤
│               Infrastructure                 │
│  (EF Core, MongoDB, RabbitMQ, Redis)        │
├─────────────────────────────────────────────┤
│                Application                   │
│  (Commands, Queries, DTOs, Interfaces)      │
├─────────────────────────────────────────────┤
│                  Domain                      │
│  (Entities, Events, Interfaces)             │
└─────────────────────────────────────────────┘
```

**Justificativa**: Clean Architecture permite alta testabilidade, independência de frameworks e facilita a manutenção. As regras de negócio ficam isoladas no Domain, sem dependências externas.

### CQRS (Command Query Responsibility Segregation)

Implementado com **MediatR** para separar operações de escrita (Commands) e leitura (Queries):

- `CreateOrderCommand` - Criação de pedidos
- `GetOrderByIdQuery` - Consulta de pedidos

**Justificativa**: CQRS permite otimizar separadamente as operações de leitura e escrita, facilita a escalabilidade e torna o código mais organizado e testável.

## Decisões Técnicas

### 1. Armazenamento Híbrido

| Dado | Banco | Justificativa |
|------|-------|---------------|
| Order (Pedido) | SQL Server | Dados transacionais que requerem ACID e relacionamentos |
| OrderItems | MongoDB | Flexibilidade para itens variados, schema-less, alta performance em leitura |

### 2. Mensageria com MassTransit + RabbitMQ

- **MassTransit**: Abstração sobre RabbitMQ que facilita a configuração e oferece retry policies, circuit breaker e dead-letter queues out-of-the-box
- **RabbitMQ**: Message broker robusto, com alta disponibilidade e ampla adoção no mercado

O evento `PedidoCriado` é publicado após a criação do pedido, permitindo que outros microsserviços (ex: faturamento, estoque) reajam de forma assíncrona.

### 3. Cache com Redis

- Cache de 2 minutos no endpoint GET /pedidos/{id}
- Estratégia **Cache-Aside**: Verifica cache primeiro, se não existir busca nos bancos e cacheia
- Redis escolhido por ser distribuído, permitindo escalabilidade horizontal da API

### 4. Autenticação JWT

- Tokens JWT com expiração de 60 minutos
- Validação de Issuer, Audience e SigningKey
- Todos os endpoints protegidos exceto /auth/login

### 5. Pacotes e Versões

| Pacote | Versão | Motivo |
|--------|--------|--------|
| .NET | 8.0 | LTS com melhor performance e novos recursos |
| EF Core | 8.0.x | Compatibilidade com .NET 8 |
| MassTransit | 8.2.x | Última versão gratuita (9.x requer licença) |
| MongoDB.Driver | 3.5.x | Driver oficial mais recente |
| Swashbuckle | 6.5.x | Compatibilidade com .NET 8 |

## Estrutura do Projeto

```
VeniceOrders/
├── src/
│   ├── VeniceOrders.API/           # Camada de apresentação
│   │   ├── Controllers/            # Endpoints REST
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   ├── Services/               # Serviços de infraestrutura (Token)
│   │   └── Configuration/          # Configurações
│   │
│   ├── VeniceOrders.Application/   # Camada de aplicação
│   │   ├── Commands/               # Handlers de escrita
│   │   ├── Queries/                # Handlers de leitura
│   │   ├── DTOs/                   # DTOs de resposta
│   │   └── Interfaces/             # Abstrações
│   │
│   ├── VeniceOrders.Domain/        # Camada de domínio
│   │   ├── Entities/               # Entidades de negócio
│   │   ├── Events/                 # Eventos de domínio
│   │   └── Interfaces/             # Contratos
│   │
│   └── VeniceOrders.Infrastructure/ # Camada de infraestrutura
│       ├── Persistence/
│       │   ├── SqlServer/          # EF Core + Repositórios SQL
│       │   └── MongoDB/            # MongoDB Driver + Repositórios
│       ├── Messaging/              # RabbitMQ Publisher
│       └── Caching/                # Redis Cache Service
│
└── tests/
    └── VeniceOrders.UnitTests/     # Testes unitários
```

## Endpoints

### Autenticação

```http
POST /api/Auth/login
Content-Type: application/json

{
    "username": "admin",
    "password": "admin123"
}
```

Resposta:
```json
{
    "token": "eyJhbGci...",
    "expiresIn": 3600,
    "tokenType": "Bearer"
}
```

### Criar Pedido

```http
POST /api/Pedidos
Authorization: Bearer {token}
Content-Type: application/json

{
    "clienteId": "b6966d6d-8eae-43f4-83b6-1ff844670271",
    "itens": [
        {
            "produto": "Notebook Dell",
            "quantidade": 2,
            "precoUnitario": 3500.00
        }
    ]
}
```

Resposta:
```json
{
    "orderId": "8717aa53-90dd-4444-b141-dbd16036b2ce",
    "message": "Pedido criado com sucesso"
}
```

### Consultar Pedido

```http
GET /api/Pedidos/{id}
Authorization: Bearer {token}
```

Resposta:
```json
{
    "id": "8717aa53-90dd-4444-b141-dbd16036b2ce",
    "clienteId": "b6966d6d-8eae-43f4-83b6-1ff844670271",
    "data": "2026-01-14T17:01:00",
    "status": "Pending",
    "total": 7000.00,
    "itens": [
        {
            "id": "b934bf0d-5ec4-449c-8301-b49121368620",
            "produto": "Notebook Dell",
            "quantidade": 2,
            "precoUnitario": 3500.00,
            "subtotal": 7000.00
        }
    ]
}
```

## Testes

Os testes unitários cobrem os handlers de commands e queries:

- `CreateOrderCommandHandlerTests`: Testa criação de pedidos e publicação de eventos
- `GetOrderByIdQueryHandlerTests`: Testa busca de pedidos com e sem cache

Para executar os testes:

```bash
dotnet test
```

## Serviços Docker

| Serviço | Porta | Descrição |
|---------|-------|-----------|
| API | 5000 | Venice Orders API |
| SQL Server | 1433 | Banco de dados relacional |
| MongoDB | 27017 | Banco de dados NoSQL |
| Redis | 6379 | Cache distribuído |
| RabbitMQ | 5672, 15672 | Message broker (Management UI na 15672) |

## Princípios Aplicados

- **SOLID**: Cada classe tem responsabilidade única, dependências são injetadas via interfaces
- **DDD**: Entidades ricas com comportamento, eventos de domínio, repositories
- **Clean Architecture**: Camadas bem definidas, dependências apontando para o centro
- **CQRS**: Separação clara entre operações de leitura e escrita
