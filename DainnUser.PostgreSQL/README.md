# DainnUser.PostgreSQL

A comprehensive, extensible user management library for ASP.NET Core APIs with support for multiple database providers, JWT authentication, two-factor authentication, role-based authorization, audit logging, and full observability.

## Features

### Core Functionality
- **User Management**: Registration, authentication, profile management, password reset
- **JWT Authentication**: Access tokens and refresh tokens with configurable expiration
- **Role-Based Authorization**: Roles and permissions system with flexible assignment
- **Two-Factor Authentication (2FA)**: TOTP-based 2FA using QR codes for authenticator apps
- **Account Security**: Login lockout, failed login attempt tracking, password complexity
- **Audit Logging**: Comprehensive audit trail for all user actions
- **Email Services**: Extensible email service interface for notifications (password reset, email confirmation, etc.)

### Database Support
- **SQLite**: Lightweight, file-based database (great for development)
- **SQL Server**: Full support for Microsoft SQL Server
- **PostgreSQL**: Native UUID support and advanced features
- **MySQL**: Full support via Pomelo provider
- **InMemory**: In-memory database for testing

### Security Features
- **Rate Limiting**: Configurable rate limiting for authentication endpoints
- **Login Lockout**: Automatic account lockout after failed login attempts
- **Password Validation**: Built-in password complexity requirements
- **CORS Support**: Configurable Cross-Origin Resource Sharing
- **HTTPS/HSTS**: Optional enforcement of HTTPS and HSTS headers
- **Antiforgery Tokens**: CSRF protection support
- **OAuth2 Authentication**: External authentication with Google, Apple, and other providers

### Observability
- **OpenTelemetry**: Full instrumentation for traces, metrics, and logs
- **Serilog**: Structured logging with OpenTelemetry integration
- **Health Checks**: Built-in health checks for database, JWT, and custom endpoints
- **Metrics**: Authentication metrics (login attempts, success/failure rates)
- **Distributed Tracing**: Request tracing across services

### Developer Experience
- **API Versioning**: Built-in API versioning support with URL segment-based routing
- **FluentValidation**: Request validation using FluentValidation
- **Event-Driven Architecture**: Domain events for extensibility
- **Exception Handling**: Global exception handling middleware
- **Extensibility**: Service interfaces for custom implementations
- **AutoMapper**: DTO mapping support

## Installation

Install via NuGet:

```bash
dotnet add package DainnUser.PostgreSQL
```

Or install locally:

```bash
cd DainnUser.PostgreSQL
dotnet pack
dotnet add ../YourProject/YourProject.csproj reference DainnUser.PostgreSQL --package-source ../DainnUser.PostgreSQL/bin/Debug
```

## Quick Start

### 1. Configure Services

In your `Program.cs`:

```csharp
using DainnUser.PostgreSQL.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add user management with configuration
builder.AddDainnUserManagement(options =>
{
    // Optional: Configure additional options
    options.CustomUserServiceType = typeof(MyCustomUserService); // Optional: override default service
});

var app = builder.Build();

// Configure middleware pipeline
app.UseDainnUserManagementMiddleware();
app.MapControllers();

app.Run();
```

### 2. Configure appsettings.json

```json
{
  "UserManagement": {
    "Provider": "postgresql",
    "ConnectionString": "Host=localhost;Database=userdb;Username=postgres;Password=postgres;Port=5432",
    "JwtSecret": "YourSuperSecretKeyForJWTTokenGeneration12345678901234567890",
    "ApplicationName": "MyApp",
    "Enable2FA": true,
    "AutoMigrateDatabase": true,
    "SeedDefaultAdmin": true,
    "DefaultAdminEmail": "admin@example.com",
    "DefaultAdminPassword": "Admin@123!",
    // When SeedDefaultAdmin is true, the following accounts are created:
    // - Admin account: admin@example.com / Admin@123! (Admin role)
    // - User account: user@example.com / User@123! (User role)
    "JwtSettings": {
      "AccessTokenHours": 1,
      "RefreshTokenDays": 7
    },
    "SecuritySettings": {
      "MaxFailedLoginAttempts": 5,
      "LockoutDurationMinutes": 30,
      "EnableRateLimiting": true,
      "RateLimitRequestsPerMinute": 10
    },
    "CorsSettings": {
      "Enabled": true,
      "AllowedOrigins": ["http://localhost:3000", "https://myapp.com"],
      "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS"],
      "AllowedHeaders": ["Content-Type", "Authorization"],
      "AllowCredentials": true
    },
    "JwtValidation": {
      "ValidateIssuer": true,
      "ValidateAudience": true,
      "ValidateLifetime": true,
      "ValidateIssuerSigningKey": true,
      "ValidIssuer": "MyApp",
      "ValidAudience": "MyAppUsers",
      "ClockSkew": "00:00:05"
    },
    "OAuth2Settings": {
      "Google": {
        "Enabled": true,
        "ClientId": "YOUR_GOOGLE_CLIENT_ID",
        "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
      },
      "Microsoft": {
        "Enabled": false,
        "ClientId": "YOUR_MICROSOFT_APPLICATION_ID",
        "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET"
      },
      "Facebook": {
        "Enabled": false,
        "AppId": "YOUR_FACEBOOK_APP_ID",
        "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
      },
      "GitHub": {
        "Enabled": false,
        "ClientId": "YOUR_GITHUB_CLIENT_ID",
        "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
      },
      "Apple": {
        "Enabled": false,
        "ClientId": "YOUR_APPLE_SERVICE_ID",
        "KeyId": "YOUR_APPLE_KEY_ID",
        "TeamId": "YOUR_APPLE_TEAM_ID",
        "PrivateKeyPath": "path/to/AuthKey_XXXXXXXXXX.p8"
      }
    }
  },
  "OpenTelemetry": {
    "Enabled": true,
    "OtlpTracesEndpoint": "http://localhost:4317",
    "ServiceName": "DainnUser.PostgreSQL",
    "ServiceVersion": "2.0.0"
  },
  "Serilog": {
    "Enabled": true,
    "OtlpLogsEndpoint": "http://localhost:4318/v1/logs",
    "MinimumLevel": {
      "Default": "Information"
    }
  },
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "AssumeDefaultVersionWhenUnspecified": true,
    "ReportApiVersions": true
  }
}
```

### 3. Create Controllers

Create controllers in your application project (not in the library). Example:

```csharp
using Microsoft.AspNetCore.Mvc;
using DainnUser.PostgreSQL.Application.Dtos;
using DainnUser.PostgreSQL.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    
    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<UserProfileDto>> Register([FromBody] RegisterDto dto)
    {
        var user = await _userService.RegisterAsync(dto);
        return Ok(user);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
    {
        var token = await _userService.LoginAsync(dto);
        return Ok(token);
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }
}
```

### 4. Register Controllers

In `Program.cs`, register controllers from your project:

```csharp
builder.Services.AddControllers()
    .AddApplicationPart(typeof(YourApp.Controllers.AuthController).Assembly);
```

## Database Migrations

### Creating Migrations

Migrations work with any configured database provider. To create and apply migrations:

```bash
# Navigate to your startup project (e.g., DainnUserManagement.Sample)
cd DainnUser.PostgreSQL.Sample

# Create a new migration
dotnet ef migrations add <MigrationName> --project ../DainnUserManagement --startup-project .

# Apply migrations
dotnet ef database update --project ../DainnUserManagement --startup-project .
```

**Note:** Migrations automatically adapt to the configured database provider. Simply update `UserManagement.Provider` in `appsettings.json` to switch between providers.

### Provider-Specific Migrations

For most providers, standard migrations work automatically. Migrations adapt to the configured database provider based on the `UserManagement.Provider` setting in `appsettings.json`.

## Database Seeding

When `SeedDefaultAdmin` is set to `true` in your configuration, the application will automatically seed the database with:

1. **Admin Role**: Creates an "Admin" role with full access
2. **User Role**: Creates a "User" role for standard users
3. **Admin Account**: 
   - Email: `admin@example.com` (configurable via `DefaultAdminEmail`)
   - Password: `Admin@123!` (configurable via `DefaultAdminPassword`)
   - Role: Admin
   - Email confirmed and active
4. **User Account**:
   - Email: `user@example.com`
   - Password: `User@123!`
   - Role: User
   - Email confirmed and active

Both accounts are created only if they don't already exist. If accounts exist, the seeding process ensures they have the correct roles assigned.

## Domain Model

### Entities

- **AppUser**: Extended user entity with properties for full name, avatar, 2FA, login tracking, and timestamps
- **AppRole**: Role entity with permissions and timestamps
- **Permission**: Granular permissions that can be assigned to roles
- **RolePermission**: Many-to-many relationship between roles and permissions
- **RefreshToken**: Refresh tokens for JWT token renewal
- **AuditLog**: Comprehensive audit trail for all actions

### Value Objects

- Custom value objects can be added to `Domain/ValueObjects/`

## Services

### Core Services

- **IUserService**: User registration, authentication, profile management, password changes
- **IAuthService**: JWT token generation and validation
- **IRoleService**: Role and permission management
- **IPermissionService**: Permission operations
- **ITwoFactorService**: 2FA setup, verification, and QR code generation
- **IAuditService**: Audit logging
- **IEmailService**: Email notifications (implement your own or use the default)

### Custom Services

All services implement interfaces, allowing you to provide custom implementations:

```csharp
builder.AddDainnUserManagement(options =>
{
    options.CustomUserServiceType = typeof(MyCustomUserService);
});
```

Your custom service must implement `IUserService`:

```csharp
public class MyCustomUserService : IUserService
{
    // Implement all interface methods
}
```

## Events System

The library uses an event-driven architecture for extensibility. Domain events are published automatically:

- **UserRegisteredEvent**: Published when a new user registers
- **UserLoggedInEvent**: Published when a user logs in
- **PasswordChangedEvent**: Published when a user changes their password
- **ProfileUpdatedEvent**: Published when a user updates their profile
- **RoleAssignedEvent**: Published when a role is assigned to a user
- **UserLockedEvent**: Published when a user account is locked

### Handling Events

Implement `IEventHandler<T>` to handle events:

```csharp
public class UserRegisteredEventHandler : IEventHandler<UserRegisteredEvent>
{
    public Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        // Your custom logic here
        return Task.CompletedTask;
    }
}

// Register in DI
builder.Services.AddScoped<IEventHandler<UserRegisteredEvent>, UserRegisteredEventHandler>();
```

## DTOs (Data Transfer Objects)

### Authentication DTOs
- **RegisterDto**: User registration
- **LoginDto**: User login
- **RefreshTokenDto**: Token refresh
- **TokenResponseDto**: Authentication response with tokens

### User DTOs
- **UserProfileDto**: User profile information
- **UpdateProfileDto**: Profile update request
- **ChangePasswordDto**: Password change request
- **UserAdminDto**: Admin view of user

### Role & Permission DTOs
- **RoleDto**: Role information
- **CreateRoleDto**: Role creation
- **AssignRoleDto**: Role assignment

### Two-Factor Authentication DTOs
- **TwoFactorSetupDto**: 2FA setup response
- **Enable2FADto**: Enable 2FA request
- **Verify2FADto**: 2FA verification

### Other DTOs
- **ForgotPasswordDto**: Password reset request
- **ResetPasswordDto**: Password reset confirmation
- **ConfirmEmailDto**: Email confirmation
- **LockUserDto**: User lockout

## Validation

All DTOs are validated using FluentValidation. Validators are automatically registered:

- **RegisterDtoValidator**: Validates registration requests
- **LoginDtoValidator**: Validates login requests
- **ChangePasswordDtoValidator**: Validates password changes

## Security

### Login Lockout

Automatic account lockout after failed login attempts:
- Configure via `SecuritySettings.MaxFailedLoginAttempts`
- Lockout duration: `SecuritySettings.LockoutDurationMinutes`

### Rate Limiting

Rate limiting is enabled by default for authentication endpoints:
- Default: 10 requests per minute
- Configurable via `SecuritySettings.RateLimitRequestsPerMinute`
- Applied to `/login` and `/register` endpoints

### Password Requirements

Passwords must meet ASP.NET Core Identity requirements (configurable via Identity options).

### CORS

Configure CORS settings in `UserManagement.CorsSettings`:
```json
{
  "CorsSettings": {
    "Enabled": true,
    "AllowedOrigins": ["http://localhost:3000"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["Content-Type", "Authorization"],
    "AllowCredentials": true
  }
}
```

### OAuth2 External Authentication

The library supports OAuth2 authentication with external providers: **Google, Microsoft, Facebook, GitHub, and Apple**.

#### Setup Google Sign-In

1. **Get OAuth2 Credentials from Google:**
   - Go to [Google Cloud Console](https://console.cloud.google.com/)
   - Create a new project or select an existing one
   - Enable the Google+ API
   - Create OAuth2 credentials (Web application)
   - Add your callback URL: `https://yourdomain.com/api/v1/oauth2/google/callback`

2. **Configure in appsettings.json:**
```json
{
  "OAuth2Settings": {
    "Google": {
      "Enabled": true,
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

3. **Use the endpoints:**
   - Challenge: `GET /api/v1/oauth2/google/challenge` - Redirects to Google login
   - Callback: `GET /api/v1/oauth2/google/callback` - Handles the callback and returns JWT tokens

#### Setup Microsoft Sign-In

1. **Get OAuth2 Credentials from Microsoft:**
   - Go to [Microsoft Azure Portal](https://portal.azure.com/)
   - Navigate to Azure Active Directory > App registrations
   - Create a new registration or select an existing one
   - Generate a client secret
   - Add redirect URI: `https://yourdomain.com/api/v1/oauth2/microsoft/callback`

2. **Configure in appsettings.json:**
```json
{
  "OAuth2Settings": {
    "Microsoft": {
      "Enabled": true,
      "ClientId": "YOUR_MICROSOFT_APPLICATION_ID",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET"
    }
  }
}
```

3. **Use the endpoints:**
   - Challenge: `GET /api/v1/oauth2/microsoft/challenge`
   - Callback: `GET /api/v1/oauth2/microsoft/callback`

#### Setup Facebook Sign-In

1. **Get OAuth2 Credentials from Facebook:**
   - Go to [Facebook Developers](https://developers.facebook.com/)
   - Create a new app or select an existing one
   - Add Facebook Login product
   - Set Valid OAuth Redirect URIs: `https://yourdomain.com/api/v1/oauth2/facebook/callback`
   - Get App ID and App Secret

2. **Configure in appsettings.json:**
```json
{
  "OAuth2Settings": {
    "Facebook": {
      "Enabled": true,
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  }
}
```

3. **Use the endpoints:**
   - Challenge: `GET /api/v1/oauth2/facebook/challenge`
   - Callback: `GET /api/v1/oauth2/facebook/callback`

#### Setup GitHub Sign-In

1. **Get OAuth2 Credentials from GitHub:**
   - Go to GitHub Settings > Developer settings > OAuth Apps
   - Create a new OAuth App
   - Set Authorization callback URL: `https://yourdomain.com/api/v1/oauth2/github/callback`
   - Get Client ID and Client Secret

2. **Configure in appsettings.json:**
```json
{
  "OAuth2Settings": {
    "GitHub": {
      "Enabled": true,
      "ClientId": "YOUR_GITHUB_CLIENT_ID",
      "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
    }
  }
}
```

3. **Use the endpoints:**
   - Challenge: `GET /api/v1/oauth2/github/challenge`
   - Callback: `GET /api/v1/oauth2/github/callback`

#### Setup Apple Sign-In

**Note:** Apple Sign-In requires additional configuration and currently requires a third-party library like `AspNet.Security.OAuth.Apple`.

1. **Get credentials from Apple Developer:**
   - Enroll in the [Apple Developer Program](https://developer.apple.com/programs/)
   - Create an App ID with "Sign in with Apple" capability
   - Create a Services ID and configure return URLs
   - Generate a private key (download the `.p8` file)

2. **Configure in appsettings.json:**
```json
{
  "OAuth2Settings": {
    "Apple": {
      "Enabled": true,
      "ClientId": "com.example.app",  // Your Service ID
      "KeyId": "KEYID123",            // Your Key ID
      "TeamId": "TEAMID123",          // Your Team ID
      "PrivateKeyPath": "path/to/AuthKey.p8"
    }
  }
}
```

**Note:** For production Apple Sign-In, install the `AspNet.Security.OAuth.Apple` NuGet package and configure it in your `ServiceExtensions.cs`.

#### Using OAuth2 Authentication

The OAuth2 controller automatically handles:
- Creating new user accounts from external providers
- Linking external logins to existing accounts
- Generating JWT tokens for authenticated users

Response format:
```json
{
  "isNewUser": true,
  "tokens": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600
  },
  "user": {
    "id": "user-guid",
    "email": "user@example.com",
    "fullName": "John Doe",
    "avatarUrl": "https://...",
    "isActive": true,
    "twoFactorEnabled": false
  }
}
```

## Observability

### OpenTelemetry

Full OpenTelemetry instrumentation is included:
- **Traces**: Request tracing with ASP.NET Core and EF Core instrumentation
- **Metrics**: Custom metrics for authentication operations
- **Logs**: Structured logging via Serilog with OTLP export

Configure in `appsettings.json`:
```json
{
  "OpenTelemetry": {
    "Enabled": true,
    "OtlpTracesEndpoint": "http://localhost:4317",
    "ServiceName": "DainnUser.PostgreSQL",
    "ServiceVersion": "2.0.0",
    "EnableAspNetCoreInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableEntityFrameworkCoreInstrumentation": true,
    "EnablePrometheusExporter": true
  }
}
```

### Health Checks

Built-in health checks:
- **Database Health Check**: Checks database connectivity
- **JWT Health Check**: Validates JWT configuration
- **Custom Endpoints**: Add your own health checks

Access health checks:
- Ready: `/health/ready`
- Live: `/health/live`

### Metrics

Authentication metrics are automatically collected:
- Login attempts (success/failure)
- Registration attempts
- Token refresh operations

## API Versioning

The library includes built-in API versioning support:
- **URL Segment-based**: Routes use `v{version:apiVersion}` (e.g., `/api/v1/auth/login`)
- **Default Version**: Configurable default version
- **Version Discovery**: Automatic version reporting

Configure in `appsettings.json`:
```json
{
  "ApiVersioning": {
    "DefaultVersion": "1.0",
    "AssumeDefaultVersionWhenUnspecified": true,
    "ReportApiVersions": true,
    "EnableUrlSegmentReader": true
  }
}
```

## Configuration Reference

### UserManagementOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Provider` | string | Required | Database provider: `sqlite`, `sqlserver`, `postgresql`, `mysql`, `inmemory` |
| `ConnectionString` | string | Required | Database connection string |
| `JwtSecret` | string | Required | Secret key for JWT token signing |
| `ApplicationName` | string | `"DainnUserManagement"` | Application name for 2FA QR codes |
| `Enable2FA` | bool | `false` | Enable two-factor authentication |
| `AutoMigrateDatabase` | bool | `false` | Automatically apply migrations on startup |
| `SeedDefaultAdmin` | bool | `false` | Seed default admin and user accounts with roles |
| `DefaultAdminEmail` | string | `"admin@example.com"` | Default admin email |
| `DefaultAdminPassword` | string | `"Admin@123!"` | Default admin password |
| `EnforceHttps` | bool | `false` | Enforce HTTPS redirection |
| `EnableHsts` | bool | `false` | Enable HSTS headers |
| `JwtSettings` | JwtSettings | - | JWT token configuration |
| `SecuritySettings` | SecuritySettings | - | Security settings (lockout, rate limiting) |
| `CorsSettings` | CorsSettings | - | CORS configuration |
| `JwtValidation` | JwtValidationOptions | - | JWT validation options |
| `OAuth2Settings` | OAuth2Settings | - | OAuth2 external authentication configuration |
| `CustomUserServiceType` | Type? | `null` | Custom user service implementation |

### JwtSettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AccessTokenHours` | int | `1` | Access token expiration in hours |
| `RefreshTokenDays` | int | `7` | Refresh token expiration in days |

### SecuritySettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MaxFailedLoginAttempts` | int | `5` | Max failed login attempts before lockout |
| `LockoutDurationMinutes` | int | `30` | Account lockout duration in minutes |
| `EnableRateLimiting` | bool | `true` | Enable rate limiting |
| `RateLimitRequestsPerMinute` | int | `10` | Rate limit for auth endpoints |

### CorsSettings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable CORS |
| `AllowedOrigins` | List<string> | `[]` | Allowed origins |
| `AllowedMethods` | List<string> | `["GET","POST","PUT","DELETE","OPTIONS"]` | Allowed HTTP methods |
| `AllowedHeaders` | List<string> | `["Content-Type","Authorization","X-Requested-With"]` | Allowed headers |
| `AllowCredentials` | bool | `true` | Allow credentials |

### OAuth2Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Google` | GoogleOAuth2Settings | - | Google OAuth2 configuration |
| `Microsoft` | MicrosoftOAuth2Settings | - | Microsoft OAuth2 configuration |
| `Facebook` | FacebookOAuth2Settings | - | Facebook OAuth2 configuration |
| `GitHub` | GitHubOAuth2Settings | - | GitHub OAuth2 configuration |
| `Apple` | AppleOAuth2Settings | - | Apple OAuth2 configuration |

### GoogleOAuth2Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable Google authentication |
| `ClientId` | string | `""` | Google OAuth2 client ID |
| `ClientSecret` | string | `""` | Google OAuth2 client secret |

### MicrosoftOAuth2Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable Microsoft authentication |
| `ClientId` | string | `""` | Microsoft OAuth2 application ID |
| `ClientSecret` | string | `""` | Microsoft OAuth2 client secret |

### FacebookOAuth2Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable Facebook authentication |
| `AppId` | string | `""` | Facebook OAuth2 App ID |
| `AppSecret` | string | `""` | Facebook OAuth2 App Secret |

### GitHubOAuth2Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable GitHub authentication |
| `ClientId` | string | `""` | GitHub OAuth2 Client ID |
| `ClientSecret` | string | `""` | GitHub OAuth2 Client Secret |

### AppleOAuth2Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | bool | `false` | Enable Apple authentication |
| `ClientId` | string | `""` | Apple OAuth2 Service ID |
| `KeyId` | string | `""` | Apple OAuth2 key ID |
| `TeamId` | string | `""` | Apple OAuth2 team ID |
| `PrivateKeyPath` | string | `""` | Path to Apple private key file (.p8) |

## Database Provider Details

### SQLite
```json
{
  "Provider": "sqlite",
  "ConnectionString": "Data Source=app.db"
}
```

### SQL Server
```json
{
  "Provider": "sqlserver",
  "ConnectionString": "Server=localhost;Database=userdb;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
}
```

### PostgreSQL
```json
{
  "Provider": "postgresql",
  "ConnectionString": "Host=localhost;Database=userdb;Username=postgres;Password=postgres;Port=5432"
}
```
**Note:** PostgreSQL uses native UUID types for all `Guid` properties automatically.

### MySQL
```json
{
  "Provider": "mysql",
  "ConnectionString": "Server=localhost;Database=userdb;User Id=root;Password=password;"
}
```

### InMemory
```json
{
  "Provider": "inmemory",
  "ConnectionString": "InMemoryDb"
}
```

## Extensibility

### Custom User Service

Override the default `UserService` with your own implementation:

```csharp
public class MyCustomUserService : IUserService
{
    // Implement interface methods
}

builder.AddDainnUserManagement(options =>
{
    options.CustomUserServiceType = typeof(MyCustomUserService);
});
```

### Custom Email Service

Implement `IEmailService`:

```csharp
public class MyEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        // Your email sending logic
        return Task.CompletedTask;
    }
}

// Register in DI
builder.Services.AddScoped<IEmailService, MyEmailService>();
```

### Custom Event Handlers

Handle domain events for custom logic:

```csharp
public class MyEventHandler : IEventHandler<UserRegisteredEvent>
{
    public Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
    {
        // Custom logic
        return Task.CompletedTask;
    }
}

// Register in DI
builder.Services.AddScoped<IEventHandler<UserRegisteredEvent>, MyEventHandler>();
```

## Testing

The library includes test infrastructure. See `DainnUserManagement.Tests` for examples.

For unit testing, use the InMemory database provider:

```json
{
  "Provider": "inmemory",
  "ConnectionString": "TestDb"
}
```

## Docker Support

The library is fully compatible with Docker. See `docker-compose.yml` for examples of:
- PostgreSQL setup
- SQL Server setup
- Observability stack (Prometheus, Grafana, Loki, Jaeger)
- OpenTelemetry Collector

## Requirements

- .NET 9.0 or later
- ASP.NET Core 9.0 or later
- Entity Framework Core 9.0 or later

### Database Provider Requirements

- **SQLite**: No additional packages
- **SQL Server**: `Microsoft.EntityFrameworkCore.SqlServer` (included)
- **PostgreSQL**: `Npgsql.EntityFrameworkCore.PostgreSQL` (included)
- **MySQL**: `Pomelo.EntityFrameworkCore.MySql` (included)

## License

[Specify your license]

## Contributing

[Contributing guidelines]

## Support

[Support information]
