# File Storage Service API

File Storage is a .NET 8 web API, implementing clean architecture principles. This service provides secure file upload, download, preview, and management capabilities with JWT authentication, comprehensive logging, and health monitoring.

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Design Decisions](#design-decisions)
- [Setup Steps](#setup-steps)
- [API Endpoints](#api-endpoints)
- [Configuration](#configuration)
- [Known Limitations](#known-limitations)
- [Future Enhancements](#future-enhancements)

## Features

- **File Management**: Upload, download, preview, list, and delete files
- **Authentication & Authorization**: JWT-based authentication with role-based access control (Admin/User)
- **File Organization**: Files stored in date-based directory structure (_storage/YYYY/MM/DD/KeyId)
- **Soft & Hard Delete**: Soft delete for recovery, hard delete for permanent removal
- **Content Type Validation**: Configurable allowed file types (PDF, PNG, JPEG by default)
- **File Size Limits**: Configurable maximum file size (200 MB default)
- **Health Checks**: Database and filesystem health monitoring
- **Structured Logging**: Serilog with file and console sinks, correlation ID tracking
- **Error Handling**: Global exception middleware with detailed error responses
- **Docker Support**: Containerized deployment with Docker Compose
- **Auto Migrations**: Automatic database migrations on startup

## Architecture Overview

The application follows **Clean Architecture** principles with clear separation of concerns across four layers:

```
FileStorageAPIApp/
├── FileStorage.API/              # Presentation Layer
│   ├── Controllers/              # API endpoints
│   ├── Middleware/               # Custom middleware (Exception, CorrelationId)
│   └── Program.cs                # Application startup and configuration
│
├── FileStorage.Application/      # Application Layer
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Interfaces/               # Application service interfaces
│   └── Services/                 # Business logic implementation
│
├── FileStorage.Domain/           # Domain Layer
│   ├── Entities/                 # Domain entities
│   ├── Enums/                    # Domain enumerations
│   └── Interfaces/               # Repository interfaces
│
└── FileStorage.Infrastructure/   # Infrastructure Layer
    ├── AppDbContext/             # Entity Framework DbContext
    ├── Repositories/             # Data access implementations
    ├── LocalFileStorageService/  # File system storage implementation
    ├── Security/                 # JWT token service
    ├── HealthChecks/             # Health check implementations
    └── Migrations/               # EF Core database migrations
```


## Design Decisions

### 1. Clean Architecture
- **Rationale**: Ensures maintainability, testability, and independence from frameworks

### 2. Date-Based File Organization
- **Structure**: `_storage/YYYY/MM/DD/{guid}/content.bin`
- **Rationale**: 
  - Prevents directory bloat
  - Enables easy archival and cleanup
  - Improves filesystem performance
- **Metadata**: Stored separately in `metadata.json` for each file

### 3. Soft Delete Pattern
- **Implementation**: `DeletedAtUtc` timestamp instead of physical deletion
- **Rationale**: 
  - Enables recovery of accidentally deleted files
  - Maintains audit trail
  - Supports compliance requirements

### 4. JWT Authentication
- **Rationale**: Stateless, scalable authentication suitable for microservices
- **Implementation**: Bearer token in Authorization header
- **Roles**: Admin (full access), User (limited access)

### 5. Correlation ID Middleware
- **Purpose**: Track requests across the application for debugging and monitoring
- **Implementation**: Unique ID generated per request, included in all log entries

### 6. Health Checks
- **Database Check**: Verifies SQL Server connectivity
- **Filesystem Check**: Validates read/write permissions on storage directory

### 7. Structured Logging with Serilog
- **Sinks**: Console (development) and File (production)
- **Features**: 
  - Daily rolling logs
  - Correlation ID enrichment
  - 14-day retention
  - Structured JSON output

### 8. Entity Framework Core with Retry Logic
- **Connection Resilience**: Automatic retry on transient failures (5 retries, 30s max delay)
- **Migrations**: Automatic migration on startup for seamless deployment

## Setup Steps

### Prerequisites

- .NET 8.0 SDK
- SQL Server 2019+ (or use Docker Compose)
- Docker Desktop (for containerized deployment)
- Visual Studio 2022 / VS Code 

### Option 1: Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <https://github.com/Esha41/FileStorageServiceAPI.git>
   cd FileStorageAPIApp
   ```

2. **Configure SQL Server Connection**
   - Update `FileStorage.API/appsettings.json` or `appsettings.Development.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=FileStorageService;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Run database migrations**
   ```bash
   cd FileStorage.Infrastructure
   dotnet ef database update --project . --startup-project ../FileStorage.API
   ```

5. **Run the application**
   ```bash
   cd FileStorage.API
   dotnet run
   ```

7. **Access the API**
   - API: `https://localhost:44356`
   - Swagger UI: `https://localhost:44356/swagger`

### Option 2: Docker Compose Setup (Recommended)

1. **Update Docker Compose configuration** (if needed)
   - Edit `docker-compose.yml` to modify:
     - SQL Server password
     - Port mappings

2. **Build and start services**
   ```bash
   docker compose up --build
   ````

4. **Access the API**
   - API: `http://localhost:8080`
   - Swagger UI: `http://localhost:8080/swagger`
   - SQL Server: `localhost:1433`

5. **View logs**
   ```bash
   docker-compose logs -f api
   ```

### Default Users

The application includes default users for testing (configured in `appsettings.json`):

- **Admin**: `admin` / `admin123`
- **User**: `user` / `user123`

## API Endpoints

### Authentication

- `POST /api/auth/login` - Authenticate and receive JWT token
  ```json
  {
    "username": "admin",
    "password": "admin123"
  }
  ```

### File Operations

- `POST /api/files` - Upload a file
  - Content-Type: `multipart/form-data`
  - Body: `file` (IFormFile), `tags` (optional string array)
  - Returns: `StoredObjectDto`

- `GET /api/files` - List all files (paginated)
  - Query Parameters: `pageNumber`, `pageSize`, `contentType`, `tags`, `createdByUserId`
  - Returns: Paginated list of `StoredObjectDto`

- `GET /api/files/{id}/download` - Download a file
  - Returns: File stream with appropriate Content-Type

- `GET /api/files/{id}/preview` - Preview a file (same as download)
  - Returns: File stream with appropriate Content-Type

- `DELETE /api/files/{id}` - Soft delete a file
  - Sets `DeletedAtUtc` timestamp

- `DELETE /api/files/{id}/hard` - Hard delete a file
  - Permanently removes file and metadata

### Health Checks

- `GET /health` - Health check endpoint
  - Returns: Health status of database and filesystem


### Logging Configuration

Logs are written to:
- **Console**: All environments
- **File**: `Logs/DATE/log-YYYYMMDD.txt` (daily rolling, 14-day retention)


## Known Limitations

- Files are stored only on the local filesystem.

- Workaround: use load balancer with sticky sessions or implement distributed file storage.

- Basic authentication stores user credentials in appsettings.json, not production-ready.

- No password hashing or user management API.


## Future Enhancements

- Support cloud storage integration with Azure Blob Storage, AWS S3.

- Implement a user management API with registration, password reset, and profile management.

- Integrate user management with ASP.NET Core Identity.

- Add API endpoints for file versioning to list and restore previous versions.

- Implement rate limiting and throttling per user and per endpoint.

- Support public and private file visibility options.

- Add image processing features like automatic thumbnail generation.

- Provide audit logging and compliance reports.

