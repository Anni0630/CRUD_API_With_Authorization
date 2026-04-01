# Product CRUD API — Eastencher Technical Assessment

A secure, production-ready **ASP.NET Core 8 Web API** implementing JWT authentication, role-based authorization, and Product CRUD operations following a clean layered architecture.

---

## 📋 Table of Contents
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Default Seeded Users](#default-seeded-users)
- [API Endpoints](#api-endpoints)
- [Authorization Rules](#authorization-rules)
- [Swagger UI](#swagger-ui)
- [Bonus Features](#bonus-features)

---

## Tech Stack

| Concern | Technology |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| Language | C# 12 |
| Authentication | JWT Bearer Tokens & Refresh Tokens |
| Database | EF Core In-Memory (zero setup) |
| ORM | Entity Framework Core + LINQ |
| Password Hashing | BCrypt.Net-Next |
| Documentation | Swagger / OpenAPI (Swashbuckle) |
| Logging | Serilog (Console sink) |
| Frontend UI | Premium Vanilla JS SPA (Glassmorphism) |
| Unit Testing | xUnit, Moq, FluentAssertions |

---

## Project Structure

```
ProductApi/
├── ProductApi.sln
└── ProductApi/
    ├── Controllers/
    │   ├── AuthController.cs        # Register / Login endpoints
    │   └── ProductController.cs     # CRUD endpoints
    ├── Models/
    │   ├── User.cs                  # User entity
    │   └── Product.cs               # Product entity
    ├── DTOs/
    │   ├── LoginDto.cs
    │   ├── RegisterDto.cs
    │   └── ProductDto.cs            # Create / Update / Read / Paged DTOs
    ├── Repositories/
    │   ├── IProductRepository.cs
    │   └── ProductRepository.cs
    ├── Services/
    │   ├── IAuthService.cs / AuthService.cs
    │   └── IProductService.cs / ProductService.cs
    ├── Data/
    │   └── ApplicationDbContext.cs  # EF Core DbContext (seeded data)
    ├── Helpers/
    │   └── JwtHelper.cs             # Token generation
    ├── Middleware/
    │   └── GlobalExceptionMiddleware.cs
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Run the API

```bash
# Navigate to the project folder
cd CRUD_API_With_Authorization/ProductApi

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

The application starts at:
- **HTTP**:  `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`

**Premium UI Dashboard** is served at the root → open `http://localhost:5000` in your browser.
**Swagger API Documentation** is served at → `http://localhost:5000/swagger`.

---

## Default Seeded Users

The In-Memory database is pre-seeded with these accounts on every startup:

| Role  | Email               | Password   |
|-------|---------------------|------------|
| Admin | admin@example.com   | Admin@123  |
| User  | user1@example.com   | User@123   |

---

## API Endpoints

### Authentication

| Method | Endpoint              | Description              | Auth Required |
|--------|-----------------------|--------------------------|---------------|
| POST   | `/api/auth/register`  | Register a new user       | ❌ No         |
| POST   | `/api/auth/login`     | Login & receive JWT pair  | ❌ No         |
| POST   | `/api/auth/refresh`   | Rotate expired access JWT | ❌ No (Requires tokens in body) |

**Register request body:**
```json
{
  "username": "john",
  "email": "john@example.com",
  "password": "Secret@123",
  "role": "User"
}
```

**Login request body:**
```json
{
  "email": "admin@example.com",
  "password": "Admin@123"
}
```

**Login / Register response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn...",
  "refreshToken": "xYz123...",
  "username": "admin",
  "email": "admin@example.com",
  "role": "Admin"
}
```

---

### Products

| Method | Endpoint              | Description                   | Auth Required | Role       |
|--------|-----------------------|-------------------------------|---------------|------------|
| GET    | `/api/product`        | Get all products (paginated)  | ✅ JWT        | Any        |
| GET    | `/api/product/{id}`   | Get product by ID             | ✅ JWT        | Any        |
| POST   | `/api/product`        | Create product                | ✅ JWT        | Admin only |
| PUT    | `/api/product/{id}`   | Update product                | ✅ JWT        | Admin only |
| DELETE | `/api/product/{id}`   | Delete product                | ✅ JWT        | Admin only |

#### Query Parameters for GET `/api/product`

| Param      | Type   | Default | Description                          |
|------------|--------|---------|--------------------------------------|
| `page`     | int    | 1       | Page number                          |
| `pageSize` | int    | 10      | Items per page (max 100)             |
| `search`   | string | —       | Search keyword for name/description  |

Example: `GET /api/product?page=1&pageSize=5&search=keyboard`

#### Create / Update product body:
```json
{
  "name": "Mechanical Keyboard",
  "description": "RGB backlit mechanical keyboard",
  "price": 79.99
}
```

---

## Authorization Rules

| Operation         | Required Role |
|-------------------|---------------|
| GET all products  | Any authenticated user |
| GET product by ID | Any authenticated user |
| Create product    | Admin |
| Update product    | Admin |
| Delete product    | Admin |

To authenticate requests, include the JWT in the `Authorization` header:
```
Authorization: Bearer <your-jwt-token>
```

---

## Swagger UI

1. Run the API (`dotnet run`)
2. Open `http://localhost:5000` in your browser
3. Click **Authorize** (🔒 button, top right)
4. Enter your JWT token (obtained from `/api/auth/login`)
5. Click **Authorize** → test all endpoints directly in the browser

---

## Bonus Features

| Feature | Status |
| JWT & Refresh Token Rotation | ✅ Implemented |
| Unit Testing (xUnit + Moq) | ✅ Implemented (100% service coverage) |
| Swagger / OpenAPI Documentation | ✅ Implemented |
| Global Exception Middleware | ✅ Implemented |
| Serilog structured logging | ✅ Implemented |
| Pagination & keyword filtering | ✅ Implemented |
| Data Annotations validation | ✅ Implemented |
| Seeded test data | ✅ Implemented |
| Premium SPA Frontend | ✅ Implemented |

---

## Architecture Notes

- **SOLID Principles**: Each class has a single responsibility; services depend on interfaces, not concrete types.
- **Dependency Injection**: All services, repositories, and helpers are registered in `Program.cs` and injected via constructors.
- **DTO Pattern**: API surfaces never expose domain models directly — all I/O goes through DTOs.
- **Repository Pattern**: Data access is abstracted behind `IProductRepository`, keeping services database-agnostic.
- **No configuration hardcoding**: JWT secret, issuer, and audience are driven by `appsettings.json`.


