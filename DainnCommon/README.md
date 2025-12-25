# DainnCommon

A shared library containing common functionality used across Dainn projects.

## Overview

This library provides shared infrastructure components that are used by multiple Dainn projects, including:
- Exception handling classes
- Middleware components
- Database provider configuration helpers
- Entity Framework Core extensions

## Components

### Exceptions

Common exception classes for consistent error handling:
- `BusinessRuleException` - Thrown when business rules are violated
- `NotFoundException` - Thrown when requested resources are not found
- `ValidationException` - Thrown when input validation fails

### Middleware

- `ExceptionMiddleware` - Global exception handling middleware that formats exceptions into consistent JSON responses

### Extensions

- `DbContextProviderExtensions` - Extension methods for configuring database providers (SQLite, SQL Server, PostgreSQL, MySQL, InMemory)
- PostgreSQL UUID configuration helpers

### Data

- `DbContextFactoryHelper` - Helper methods for creating DbContext instances at design time

## Usage

### Adding to Your Project

Add a project reference to DainnCommon:

```xml
<ProjectReference Include="..\DainnCommon\DainnCommon.csproj" />
```

### Using Exceptions

```csharp
using DainnCommon.Exceptions;

throw new NotFoundException("User", userId);
throw new BusinessRuleException("Cannot delete active user");
throw new ValidationException(validationErrors);
```

### Using Exception Middleware

```csharp
using DainnCommon.Middleware;

app.UseMiddleware<ExceptionMiddleware>();
```

### Configuring Database Providers

```csharp
using DainnCommon.Extensions;

services.AddDbContext<MyDbContext>(options =>
{
    options.ConfigureDatabaseProvider(provider, connectionString);
});
```

### Configuring PostgreSQL UUIDs

```csharp
using DainnCommon.Extensions;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    if (Database.IsNpgsql())
    {
        modelBuilder.ConfigurePostgreSqlUuids(setDefaultValue: true);
    }
}
```

### Using DbContext Factory Helper

```csharp
using DainnCommon.Data;

public class MyDbContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>();
        var configuration = DbContextFactoryHelper.BuildConfiguration();
        var (provider, connectionString) = DbContextFactoryHelper.GetDatabaseConfiguration(configuration);
        DbContextFactoryHelper.ConfigureDbContext(optionsBuilder, provider, connectionString);
        return new MyDbContext(optionsBuilder.Options);
    }
}
```

## Supported Database Providers

- SQLite
- SQL Server
- PostgreSQL / Npgsql
- MySQL / MariaDB
- InMemory

## Requirements

- .NET 8.0 or later
- Entity Framework Core 8.0.11 or later

