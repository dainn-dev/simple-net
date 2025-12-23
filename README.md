# Simple-Net Solution

A comprehensive .NET solution providing user management and product catalog functionality with full observability support. Built with ASP.NET Core, Entity Framework Core, and modern microservices patterns.

## 🏗️ Solution Overview

This solution consists of multiple projects working together to provide:

- **User Management**: Complete authentication, authorization, and user administration system
- **Product Catalog**: Flexible EAV (Entity-Attribute-Value) based product management system
- **Observability**: Full OpenTelemetry integration with Prometheus, Grafana, Loki, and Jaeger
- **API Layer**: RESTful API with Swagger documentation and API versioning

## 📦 Projects

### Core Libraries

#### [DainnUserManagement](./DainnUserManagement/README.md)
A comprehensive user management library featuring:
- JWT authentication with refresh tokens
- Role-based authorization (RBAC)
- Two-factor authentication (2FA) with TOTP
- OAuth2 external authentication (Google, Microsoft, Facebook, GitHub, Apple)
- Account security (lockout, rate limiting, password complexity)
- Audit logging
- Multi-database provider support (SQLite, SQL Server, PostgreSQL, MySQL, InMemory)
- Full OpenTelemetry instrumentation

#### [DainnProductEAVManagement](./DainnProductEAVManagement/README.md)
A flexible product catalog library using the EAV pattern:
- Dynamic product attributes without schema changes
- Multi-store support with store-specific values
- Category management with hierarchical structure
- Inventory management
- Tier pricing
- Media gallery support
- Product relations (related, upsell, crosssell)
- Multi-database provider support

### Application Projects

#### [DainnUserManagement.API](./DainnUserManagement.API/)
The main API application that combines both libraries:
- RESTful API endpoints
- Swagger/OpenAPI documentation
- API versioning support
- Health checks and metrics endpoints
- Docker-ready configuration

#### [DainnUserManagement.Tests](./DainnUserManagement.Tests/)
Test project for unit and integration testing.

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [Docker](https://www.docker.com/get-started) and Docker Compose (for observability stack)
- Database provider of choice (SQLite, SQL Server, PostgreSQL, or MySQL)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd simple-net
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Configure the application**
   
   Edit `DainnUserManagement.API/appsettings.json`:
   ```json
   {
     "DainnApplication": {
       "Provider": "sqlite",
       "ConnectionString": "Data Source=app.db",
       "JwtSecret": "YourSuperSecretKeyForJWTTokenGeneration12345678901234567890",
       "AutoMigrateDatabase": true,
       "SeedDefaultAdmin": true,
       "SeedDefaultAttributes": true
     }
   }
   ```

4. **Run the application**
   ```bash
   cd DainnUserManagement.API
   dotnet run
   ```

5. **Access the API**
   - API: http://localhost:5000
   - Swagger UI: http://localhost:5000/swagger
   - Health Check: http://localhost:5000/health/live

### Docker Setup (with Observability Stack)

1. **Start all services**
   ```bash
   docker-compose up -d
   ```

2. **Access services**
   - **API**: http://localhost:5000
   - **Swagger**: http://localhost:5000/swagger
   - **Grafana**: http://localhost:3000 (admin/admin)
   - **Prometheus**: http://localhost:9090
   - **Jaeger**: http://localhost:16686
   - **Loki**: http://localhost:3100

3. **Verify observability**
   - Make some API calls to generate traces
   - Check Jaeger for distributed traces
   - View metrics in Prometheus
   - Explore logs in Grafana (Loki data source)

For detailed Docker setup instructions, see [docker-README.md](./docker-README.md).

## 🏛️ Architecture

### Solution Structure

```
simple-net/
├── DainnUserManagement/          # User management library
│   ├── Application/              # DTOs, services, events, validators
│   ├── Domain/                    # Domain entities
│   ├── Infrastructure/           # Persistence, auth, middleware
│   └── Extensions/               # Service registration extensions
│
├── DainnProductEAVManagement/    # Product catalog library
│   ├── Entities/                 # Product, category, attribute entities
│   ├── ValueEntities/            # EAV value tables
│   ├── Repositories/             # Data access layer
│   ├── Services/                 # Business logic layer
│   └── Extensions/               # Service registration extensions
│
├── DainnUserManagement.API/      # API application
│   ├── Controllers/              # API endpoints
│   ├── Dtos/                     # API-specific DTOs
│   └── Extensions/               # API configuration
│
└── DainnUserManagement.Tests/    # Test project
```

### Technology Stack

- **.NET 9.0**: Latest .NET runtime and SDK
- **ASP.NET Core**: Web framework
- **Entity Framework Core**: ORM with multi-provider support
- **JWT Bearer Authentication**: Token-based authentication
- **OpenTelemetry**: Observability instrumentation
- **Serilog**: Structured logging
- **FluentValidation**: Request validation
- **Swagger/OpenAPI**: API documentation
- **Docker**: Containerization

### Database Providers

Both libraries support multiple database providers:

- **SQLite**: File-based, perfect for development
- **SQL Server**: Enterprise-grade Microsoft database
- **PostgreSQL**: Advanced open-source database with native UUID support
- **MySQL**: Popular open-source database
- **InMemory**: For testing

## 📚 Documentation

- [User Management Library Documentation](./DainnUserManagement/README.md)
- [Product Catalog Library Documentation](./DainnProductEAVManagement/README.md)
- [Docker & Observability Setup](./docker-README.md)

## 🔑 Default Credentials

When `SeedDefaultAdmin` is enabled, the following accounts are created:

- **Admin Account**
  - Email: `admin@example.com`
  - Password: `Admin@123!`
  - Role: Admin

- **User Account**
  - Email: `user@example.com`
  - Password: `User@123!`
  - Role: User

⚠️ **Important**: Change these credentials in production!

## 🎯 Key Features

### User Management
- ✅ User registration and authentication
- ✅ JWT access tokens and refresh tokens
- ✅ Role-based access control (RBAC)
- ✅ Two-factor authentication (2FA)
- ✅ OAuth2 external authentication
- ✅ Account lockout and rate limiting
- ✅ Password reset and email confirmation
- ✅ Audit logging
- ✅ Profile management

### Product Catalog
- ✅ EAV pattern for flexible attributes
- ✅ Multi-store support
- ✅ Hierarchical categories
- ✅ Inventory management
- ✅ Tier pricing
- ✅ Product relations
- ✅ Media gallery
- ✅ Search functionality

### Observability
- ✅ Distributed tracing (Jaeger)
- ✅ Metrics collection (Prometheus)
- ✅ Log aggregation (Loki)
- ✅ Dashboard visualization (Grafana)
- ✅ Health checks
- ✅ Performance monitoring

## 🔧 Configuration

### User Management Configuration

See [User Management README](./DainnUserManagement/README.md#configuration-reference) for complete configuration options.

Key settings in `appsettings.json`:
```json
{
  "DainnApplication": {
    "Provider": "postgresql",
    "ConnectionString": "Host=localhost;Database=userdb;...",
    "JwtSecret": "your-secret-key",
    "Enable2FA": true,
    "AutoMigrateDatabase": true,
    "SeedDefaultAdmin": true,
    "SeedDefaultAttributes": true
  }
}
```

## 🧪 Testing

Run tests:
```bash
dotnet test
```

For integration testing, use the InMemory database provider:
```json
{
  "Provider": "inmemory",
  "ConnectionString": "TestDb"
}
```

## 🐳 Docker

The solution includes a complete Docker Compose setup with:

- Application container
- PostgreSQL database
- OpenTelemetry Collector
- Prometheus (metrics)
- Grafana (visualization)
- Loki (logs)
- Jaeger (tracing)

See [docker-README.md](./docker-README.md) for detailed instructions.

## 📊 API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login
- `POST /api/v1/auth/refresh` - Refresh token
- `GET /api/v1/auth/me` - Get current user profile

### User Management
- `GET /api/v1/users` - List users (Admin)
- `GET /api/v1/users/{id}` - Get user details
- `PUT /api/v1/users/{id}` - Update user
- `DELETE /api/v1/users/{id}` - Delete user

### Product Catalog
- `GET /api/v1/products` - List products
- `GET /api/v1/products/{id}` - Get product details
- `POST /api/v1/products` - Create product
- `PUT /api/v1/products/{id}` - Update product
- `DELETE /api/v1/products/{id}` - Delete product

### Health & Metrics
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe
- `GET /metrics` - Prometheus metrics

Full API documentation available at `/swagger` when running the application.

## 🔒 Security

- JWT token-based authentication
- Password hashing with ASP.NET Core Identity
- Account lockout after failed attempts
- Rate limiting on authentication endpoints
- CORS configuration support
- HTTPS/HSTS enforcement (optional)
- OAuth2 external authentication
- Two-factor authentication (2FA)

## 📈 Observability

The solution includes comprehensive observability:

- **Traces**: Distributed request tracing via OpenTelemetry → Jaeger
- **Metrics**: Application and infrastructure metrics via Prometheus
- **Logs**: Structured logging via Serilog → Loki
- **Dashboards**: Pre-configured Grafana dashboards

Access observability tools:
- Grafana: http://localhost:3000
- Prometheus: http://localhost:9090
- Jaeger: http://localhost:16686

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📝 License

[Specify your license here]

## 🆘 Support

For issues, questions, or contributions:
- Open an issue on GitHub
- Check the individual library READMEs for detailed documentation
- Review the [Docker README](./docker-README.md) for deployment help

## 🗺️ Roadmap

- [ ] Additional OAuth2 providers
- [ ] GraphQL API support
- [ ] Advanced product search
- [ ] Bulk operations
- [ ] Export/import functionality
- [ ] Multi-language support
- [ ] Advanced analytics

---

**Built with ❤️ using .NET 9.0**

