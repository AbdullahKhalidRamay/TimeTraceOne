# TimeFlow Backend API

A comprehensive ASP.NET Core 8.0 Web API for time tracking and project management, built with modern best practices and enterprise-grade architecture.

## 🚀 Features

- **Authentication & Authorization**: JWT-based authentication with role-based access control
- **User Management**: Complete CRUD operations for users with different roles (Employee, Manager, Owner)
- **Time Entry Management**: Individual and weekly bulk time entry operations
- **Project Management**: Full project lifecycle management with client information
- **Team Management**: Team creation, member management, and associations
- **Reporting & Analytics**: Comprehensive reporting with export capabilities
- **Approval Workflow**: Manager approval system for time entries
- **Real-time Notifications**: System for time entry approvals and rejections

## 🏗️ Architecture

- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: JWT with refresh tokens
- **Validation**: FluentValidation for input validation
- **Mapping**: AutoMapper for object mapping
- **Logging**: Serilog with structured logging
- **Caching**: Redis and in-memory caching
- **Documentation**: Swagger/OpenAPI

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, SQL Server Express, or Azure SQL)
- Visual Studio 2022 or VS Code
- Redis (optional, for caching)

## 🛠️ Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd TimeTraceOne
```

### 2. Configure Database Connection

Update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TimeFlow;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

### 3. Configure JWT Settings

Update the JWT configuration in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-with-at-least-32-characters-for-production-use",
    "Issuer": "https://timeflow.com",
    "Audience": "https://timeflow.com",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### 4. Install Dependencies

```bash
dotnet restore
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at:
- **API**: https://localhost:5001/api
- **Swagger UI**: https://localhost:5001

## 🔐 Authentication

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "john.doe@timeflow.com",
  "password": "password123"
}
```

### Using JWT Token

Include the JWT token in the Authorization header:

```http
Authorization: Bearer <your-jwt-token>
```

## 👥 Default Users

The application comes with pre-seeded users:

| Email | Password | Role | Job Title |
|-------|----------|------|-----------|
| john.doe@timeflow.com | password123 | Owner | CEO |
| jane.smith@timeflow.com | password123 | Manager | Project Manager |
| bob.johnson@timeflow.com | password123 | Employee | Developer |

## 📚 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - User logout
- `GET /api/auth/me` - Get current user

### Time Entries
- `GET /api/time-entries` - Get time entries with filtering
- `GET /api/time-entries/{id}` - Get time entry by ID
- `POST /api/time-entries` - Create new time entry
- `PUT /api/time-entries/{id}` - Update time entry
- `DELETE /api/time-entries/{id}` - Delete time entry
- `POST /api/time-entries/weekly-bulk` - Create weekly bulk time entries
- `GET /api/time-entries/weekly/{weekStart}` - Get weekly time entries
- `POST /api/time-entries/{id}/approve` - Approve time entry (Manager/Owner only)
- `POST /api/time-entries/{id}/reject` - Reject time entry (Manager/Owner only)

## 🔧 Development

### Project Structure

```
TimeTraceOne/
├── Controllers/          # API controllers
├── Data/                # DbContext and database seeder
├── DTOs/                # Data transfer objects
├── Extensions/          # Service collection extensions
├── Middleware/          # Custom middleware
├── Models/              # Entity models
├── Services/            # Business logic services
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

### Adding New Features

1. **Create Model**: Add entity in `Models/` folder
2. **Create DTOs**: Add DTOs in `DTOs/` folder
3. **Create Service**: Add service interface and implementation in `Services/` folder
4. **Create Controller**: Add controller in `Controllers/` folder
5. **Update DbContext**: Add DbSet in `Data/TimeFlowDbContext.cs`
6. **Add Validation**: Create FluentValidation validators

### Database Migrations

```bash
# Add migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Remove migration
dotnet ef migrations remove
```

## 🧪 Testing

### Unit Tests

```bash
dotnet test
```

### Integration Tests

```bash
dotnet test --filter Category=Integration
```

## 📊 Monitoring & Logging

- **Logs**: Stored in `Logs/` folder with daily rotation
- **Console Logging**: Real-time logging in development
- **Structured Logging**: JSON format for production

## 🚀 Deployment

### Production Configuration

1. Update connection strings for production database
2. Set secure JWT keys
3. Configure CORS for production domains
4. Enable HTTPS
5. Set up monitoring and alerting

### Docker Support

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TimeTraceOne.csproj", "./"]
RUN dotnet restore "TimeTraceOne.csproj"
COPY . .
RUN dotnet build "TimeTraceOne.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeTraceOne.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeTraceOne.dll"]
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the API documentation at `/swagger`

## 🎯 Roadmap

- [ ] Advanced reporting and analytics
- [ ] Mobile API endpoints
- [ ] Real-time notifications with SignalR
- [ ] Advanced search and filtering
- [ ] Export functionality (CSV, PDF, Excel)
- [ ] Integration with external time tracking tools
- [ ] Multi-tenant support
- [ ] Advanced approval workflows
- [ ] Performance monitoring and metrics
- [ ] Automated testing pipeline
