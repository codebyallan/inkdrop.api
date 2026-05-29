# 🖨️ Inkdrop API

**Enterprise-grade Backend** for a Toner Control system. A robust REST API designed with Domain-Driven Design (DDD) and Clean Architecture principles to manage toners, printers, locations, and inventory movements.

---

## 📖 About the Project

Inkdrop API is not just a CRUD application; it is built to be scalable, secure, and maintainable. It implements a **Rich Domain Model** where business rules are encapsulated within entities, avoiding anemic models and ensuring data integrity from the moment of instantiation.

### ✨ Key Features

- **🛡️ Advanced Security**
  - **Authentication**: Secure Cookie-based authentication (HttpOnly, Secure, SameSite=None) to support cross-domain SPAs.
  - **Authorization**: Role-Based Access Control (RBAC) with `Admin` and `Technician` roles.
  - **CSRF Protection**: Double-Submit Cookie pattern to prevent Cross-Site Request Forgery.
  - **Password Security**: Native PBKDF2 hashing with unique salts for every user and strict complexity requirements (minimum 6 characters, including uppercase, lowercase, number, and special character).
- **📦 Inventory Management**
  - **Locations** $\rightarrow$ **Printers** $\rightarrow$ **Toners**.
  - **Movements**: Full traceability of stock IN/OUT movements.
  - **Active Monitoring**: Integration with local agents via SNMP to automate page counters and toner level reporting.
  - **Low Stock Alerts**: Dedicated endpoints for KPI dashboards.
  - **Concurrent Access Protection**: High-resilience concurrency control using Row-Level Versioning (Optimistic Concurrency) and Atomic Constraint Handling to prevent race conditions in stock updates.
- **⚙️ Architectural Highlights**
  - **Notification Pattern**: Business errors are collected in a `NotificationContext` instead of throwing costly exceptions, significantly improving performance.
  - **Soft Delete**: Global query filters ensure that deleted records are ignored across the system while maintaining database integrity.
  - **Resilience**: Implementation of `CancellationToken` across all async layers to optimize resource usage.
  - **Keep-Alive Strategy**: Integrated Health Check endpoint to prevent "Cold Starts" on free-tier hosting.

---

## 🛠 Tech Stack

| Area | Technology |
|---|---|
| **Framework** | ASP.NET Core 10 |
| **Language** | C# 13 |
| **ORM** | Entity Framework Core 10 |
| **Database** | PostgreSQL |
| **Containerization** | Docker (Multi-stage Build) |
| **API Docs** | Swagger / OpenAPI |
| **Validation** | Custom Notification Pattern |

---

## 🚀 Getting Started

### 📋 Prerequisites
- **.NET 10 SDK**
- **PostgreSQL**
- **Docker** (Optional)

### 1️⃣ Local Setup (Standard)
```bash
git clone https://github.com/codebyallan/inkdrop.api.git
cd inkdrop.api
cd Inkdrop.Api
dotnet ef database update
dotnet run
```

### 2️⃣ Local Setup (Docker)
```bash
docker build -t inkdrop-api .
docker run -d -p 5109:8080 --name inkdrop-api-container inkdrop-api
```

### 3️⃣ Configuration
Edit `appsettings.json` and set your connection string and allowed origins:
```json
{
  "DbConfig": {
    "ConnectionString": "Host=your_host;Port=5432;Database=inkdrop;Username=postgres;Password=yourpassword"
  },
  "AllowedOrigins": [ "http://localhost:4200" ]
}
```

---

## 📡 API Endpoints

### 🔑 Auth & Security
| Method | Route | Description | Access |
|--------|-------|-------------|---------|
| `GET` | `/api/auth/csrf` | Get Anti-Forgery Token | Public |
| `POST` | `/api/auth/login` | Authenticate & create session | Public |
| `POST` | `/api/auth/logout` | Terminate session | Authenticated |
| `POST` | `/api/apikey` | Create API Key for Agents | Admin |

### 🤖 Bot & Integration (API Key Auth)
| Method | Route | Description | Access |
|--------|-------|-------------|---------|
| `GET` | `/api/bot/printers` | List printers for monitoring | ApiKey |
| `POST`| `/api/bot/telemetry` | Report SNMP telemetry data | ApiKey |

### 👥 User Management
| Method | Route | Description | Access |
|--------|-------|-------------|---------|
| `GET` | `/api/user` | List all users | Admin |
| `POST` | `/api/user` | Create new user (Default: Technician) | Admin |
| `PUT` | `/api/user/{id}` | Update user profile/role | Admin |
| `PATCH`| `/api/user/{id}/password` | Change user password | Admin |
| `DELETE`| `/api/user/{id}` | Soft-delete user | Admin |

### 📍 Locations, 🖨️ Printers & 🟦 Toners
*(Standard CRUD operations)* $\rightarrow$ **Admin Only**

### 🔄 Movements
| Method | Route | Description | Access |
|--------|-------|-------------|---------|
| `POST` | `/api/movements` | Record stock IN or OUT | Admin, Technician |
| `GET` | `/api/movements` | List all movements | Admin, Technician |

### 🩺 System Health
| Method | Route | Description | Access |
|--------|-------|-------------|---------|
| `GET` | `/api/health` | Heartbeat for hosting services | Public |

---

## 📄 License
This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.
