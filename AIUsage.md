# AI-Usage Report: Order Processing Service

## Development Approach

I followed a three-phase workflow to develop this application:

### Phase 1: Ask (Context Gathering)
Used **GPT-mini** to clarify requirements and scope:
- Functional requirements: order creation, inventory validation, status transitions
- Technology stack: .NET 8.0, MongoDB, Redis, RabbitMQ, Docker
- Architecture constraints: clean layering, atomic operations, event-driven design
- Testing requirements: unit tests for business logic and edge cases

### Phase 2: Plan (Design & Breakdown)
Created a structured development plan:
1. Project scaffolding and dependency setup
2. Domain entities (Order, Product) and state machine
3. API controllers (Orders, Products)
4. Data layer (MongoDB repositories, seeding)
5. Services (OrderService, ProductService, InventoryService, CacheService, EventPublisher)
6. Validation and error handling
7. Unit tests (4 test files covering core logic)
8. Docker configuration

### Phase 3: Develop (Agent-Driven Implementation)
Used **Claude Sonnet 4.5** to accelerate implementation:
- Generated service structures and dependency injection setup
- Implemented MongoDB atomic updates for stock reservation
- Created Redis caching layer with invalidation logic
- Generated RabbitMQ event publishing
- Scaffolded unit tests and mocks

---

## Key Architectural Decisions

### 1. MongoDB Atomic Updates (Accepted from AI)
Used atomic `FindOneAndUpdateAsync` with filter-based stock decrement to prevent race conditions:
```csharp
var filter = Builders<Product>.Filter.And(
    Builders<Product>.Filter.Eq(p => p.Id, productId),
    Builders<Product>.Filter.Gte(p => p.Stock, quantity)
);
var update = Builders<Product>.Update.Inc(p => p.Stock, -quantity);
```
**Why**: Simple, performant, prevents overselling under concurrent load.

### 2. Redis Caching (Accepted with refinement)
AI generated caching wrapper; enhanced with proper cache invalidation on all mutations (create order, update status).
- Products: 30-minute TTL (low-churn data)
- Orders: 1-hour TTL (user queries)

### 3. RabbitMQ Event Publishing (Accepted with modification)
AI suggested multiple messaging patterns; selected simple event publishing approach:
- OrderCreatedEvent, OrderStatusChangedEvent, OrderCancelledEvent
- Enables downstream services to react asynchronously

### 4. Clean Service Layering (My decision)
Strict layering: Controllers → Services → Domain Logic → Repositories → Infrastructure
- Ensures testability, maintainability, and loose coupling

### 5. Order State Machine (My decision)
Explicit state transitions: `Pending → Confirmed → Processing → Shipped → Delivered` with `Cancelled` from non-terminal states.
- Validated in StatusTransitionValidator to prevent invalid transitions

---

## AI Suggestions Rejected

| Suggestion | Reason Rejected |
|-----------|-----------------|
| MongoDB Transactions | Unnecessary complexity; atomic updates sufficient and more performant |
| Caching POST Responses | POST is write-heavy; caching introduces consistency issues |
| Direct Service Invocation | Violates service independence; required event-driven decoupling |

---

## What AI Generated Successfully

✅ Service scaffolding and DI configuration  
✅ MongoDB atomic update patterns  
✅ Redis caching wrapper (refined for invalidation)  
✅ RabbitMQ event classes and publisher skeleton  
✅ Unit test scaffolding (OrderValidationTests, StatusTransitionValidatorTests, StockReservationTests, BusinessLogicTests)  
✅ Docker configuration (docker-compose.yml with all services)  

---

## Verification Process

1. All AI-generated code was manually reviewed for correctness and architecture alignment
2. Unit tests validated business logic and edge cases
3. Code refactored to match clean code principles and project standards
4. Architecture validated against requirements

---

## Summary

AI accelerated development by handling boilerplate, suggesting proven patterns, and scaffolding tests. However, critical architectural decisions—concurrency strategy, caching approach, event-driven design, and state machine logic—were my decisions and intentionally implemented to ensure production-grade quality.

The Ask → Plan → Develop workflow kept AI aligned with human intentions throughout.