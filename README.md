# MicroservicesDemo

A small .NET-based microservices demo that demonstrates an API gateway and several service projects (order, payment, product). Intended as a reference project for experimenting with service boundaries, layering (Api / Application / Domain / Infrastructure), and containerization.

### Stack
- **Language(s):** C# (+ Dockerfiles)
- **Framework / runtime:** .NET (projects are .NET C# projects — see each .csproj's TargetFramework)
- **Notable technologies:** ASP.NET Core Web API, Docker (containerization). See each project's .csproj for exact package dependencies.

## How it's organized
Top-level layout:
```
ApiGateway/                         - API gateway Web API project (Dockerfile + Program.cs, controllers/)
OrderService.Api/                   - Order service API project (Dockerfile + Program.cs)
OrderService.Application/           - Order service application layer (business logic / DTOs)
OrderService.Domain/                - Order domain models / entities
OrderService.Infrastructure/        - Order data access / infra
PaymentService.Api/                 - Payment service API project (Dockerfile + Program.cs)
PaymentService.Application/         - Payment application layer
PaymentService.Domain/              - Payment domain models
PaymentService.Infrastructure/      - Payment infra
ProductService.Application/         - Product application layer, features, DTOs, mappings
ProductService.Domain/              - Product domain models
ProductsService.Api/                - Another product API project
MicroservicesDemo.sln               - Solution file containing the projects
README.md                           - This file
.github/                            - repo CI / workflow configuration (if present)
```

How it fits together:
- The ApiGateway routes incoming client requests to the individual microservice APIs (Order, Payment, Product). Each service follows a layered pattern with separate Application / Domain / Infrastructure projects, enabling clear separation of API surface, business logic, domain models, and persistence/infra concerns. The solution file composes the projects for local development.

## How to run it

Prerequisites:
- .NET SDK installed (check `dotnet --info` and each project .csproj TargetFramework for exact version)
- Docker (optional, if you want to run containers)

Quick local run (single project at a time)
1. Restore and build the whole solution:
   ```bash
   dotnet restore
   dotnet build MicroservicesDemo.sln
   ```
2. Run a single API project (open separate terminals for multiple services):
   ```bash
   dotnet run --project ./ApiGateway/ApiGateway.csproj
   dotnet run --project ./OrderService.Api/OrderService.Api.csproj
   dotnet run --project ./PaymentService.Api/PaymentService.Api.csproj
   dotnet run --project ./ProductsService.Api/ProductsService.Api.csproj
   ```
3. The projects include `appsettings.json` and `appsettings.Development.json`. Configure any connection strings or endpoints in those files or via environment variables before running.

Build and run with Docker (per-service)
1. Build a service image:
   ```bash
   docker build -f ApiGateway/Dockerfile -t microservice-apigateway ./ApiGateway
   docker build -f OrderService.Api/Dockerfile -t microservice-order ./OrderService.Api
   docker build -f PaymentService.Api/Dockerfile -t microservice-payment ./PaymentService.Api
   ```
2. Run each image mapping appropriate ports (adjust as needed):
   ```bash
   docker run -p 5000:80 microservice-apigateway
   docker run -p 5001:80 microservice-order
   docker run -p 5002:80 microservice-payment
   ```
Note: There is no docker-compose.yml included — creating one is recommended for running the full set together with consistent network and environment variables.

Tests
- I did not find an explicit test project in the top-level directories. Add tests under a Tests/ directory or add project-specific test projects and include them in the solution.

## Notes & TODOs (observations)
- The solution file `MicroservicesDemo.sln` is present and ties projects together for development.
- Each API project includes a Dockerfile; consider adding a top-level docker-compose.yml to simplify running the system locally.
- Confirm the TargetFrameworks and package references inside each .csproj to lock the required .NET SDK version and list any important NuGet dependencies (EF Core, messaging libraries, MediatR, etc.) if you want this README to list them explicitly.
- Some domain projects (e.g., OrderService.Domain) currently contain placeholder files — review domain model completeness before using as production reference.

## Try asking
- Can you add a docker-compose.yml that runs ApiGateway, OrderService.Api and PaymentService.Api with mapped ports and environment variables?
- What TargetFramework and critical NuGet packages are used in `OrderService.Api/OrderService.Api.csproj` and `ProductService.Application/ProductService.Application.csproj`?
- Where are the persistence implementations (repositories/EF Core contexts) in OrderService.Infrastructure and PaymentService.Infrastructure, and can you add short usage examples?
