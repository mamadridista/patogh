# Patogh

A production-oriented restaurant reservation platform built with **.NET 8** following Clean Architecture principles.

## Architecture

```
src/
├── Patogh.API             # Presentation layer
├── Patogh.Application     # Use cases, CQRS, business workflows
├── Patogh.Domain          # Core business entities and rules
├── Patogh.Infrastructure  # External services
└── Patogh.Persistance     # Database and EF Core
```

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Redis
- Docker
- GitHub Actions CI/CD

## Running locally

```bash
cp .env.example .env
docker compose up -d
dotnet run --project src/Patogh.API
```

## Development workflow

- Feature branches from `develop`
- Pull requests required
- Automated build/test pipeline
- Conventional commit messages recommended

## License

MIT
