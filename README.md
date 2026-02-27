# 🖨️ Inkdrop API

**Backend** for a Toner Control system — REST API for managing toners, printers, locations, and movements. The frontend is a separate repository.

---

## 📖 About the project

This repository contains only the **backend** (ASP.NET Core REST API). It exposes endpoints consumed by the [Inkdrop App](https://github.com/codebyallan/inkdrop.app) frontend.

### ✨ Features

- **Locations** — Create, list, get by ID, update, and soft-delete locations (sectors/rooms)
- **Printers** — Manage printers with IP address validation and location assignment
- **Toners** — Manage toners with stock control via IN/OUT movements and low stock alerts
- **Movements** — Record stock IN (replenishment) and OUT (printer assignment) movements, filter by printer or toner
- **Dashboard-ready** — Dedicated low stock endpoint and movement filters for KPI cards

### 🏗 Architecture highlights

- **Rich Domain Model** — Entities encapsulate their own validation and business rules (`In()`, `Out()`, `MarkAsDeleted()`, `Update()`)
- **Notification pattern** — Business errors are collected and returned as a list instead of throwing exceptions, avoiding the performance cost of exception-based flow control
- **Soft delete** — Records are never physically deleted; a global `QueryFilter` filters them automatically
- **Global exception handler** — Unhandled exceptions are caught and returned as RFC 7807 `ProblemDetails`
- **CORS via configuration** — Allowed origins are read from `appsettings.json`, not hardcoded

---

## 🛠 Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| Language | C# |
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL |
| API Docs | Swagger / OpenAPI |
| Validation | Custom Notification Pattern |

---

## 🚀 Clone and run locally

### 📋 Prerequisites

- **.NET 10 SDK**
- **PostgreSQL**

### 1️⃣ Clone the repository

```bash
git clone https://github.com/codebyallan/inkdrop.api.git
cd inkdrop.api
```

### 2️⃣ Configure the database

Edit `Inkdrop.Api/appsettings.Development.json` and set your PostgreSQL connection string and allowed origins:

```json
{
  "DbConfig": {
    "ConnectionString": "Host=localhost;Port=5432;Database=inkdrop;Username=postgres;Password=yourpassword"
  },
  "AllowedOrigins": [
    "http://localhost:4200"
  ]
}
```

### 3️⃣ Apply migrations

```bash
cd Inkdrop.Api
dotnet ef database update
```

### 4️⃣ Run the API

```bash
dotnet run
```

The Swagger UI will be available at **http://localhost:5109** (or the port shown in the terminal).

---

## 📡 Endpoints

### 📍 Locations — `/api/location`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/location` | List all locations |
| `GET` | `/api/location/{id}` | Get a location by ID |
| `POST` | `/api/location` | Create a new location |
| `PUT` | `/api/location/{id}` | Update a location |
| `DELETE` | `/api/location/{id}` | Soft-delete a location |

### 🖨️ Printers — `/api/printer`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/printer` | List all printers |
| `GET` | `/api/printer/{id}` | Get a printer by ID |
| `POST` | `/api/printer` | Create a new printer |
| `PUT` | `/api/printer/{id}` | Update a printer |
| `DELETE` | `/api/printer/{id}` | Soft-delete a printer |

### 🟦 Toners — `/api/toner`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/toner` | List all toners |
| `GET` | `/api/toner/{id}` | Get a toner by ID |
| `GET` | `/api/toner/low?threshold=3` | List toners below stock threshold (default: 3) |
| `POST` | `/api/toner` | Create a new toner |
| `PUT` | `/api/toner/{id}` | Update a toner |
| `DELETE` | `/api/toner/{id}` | Soft-delete a toner |

### 🔄 Movements — `/api/movements`

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/movements` | List all movements |
| `GET` | `/api/movements/{id}` | Get a movement by ID |
| `GET` | `/api/movements/printer/{id}` | List all OUT movements for a specific printer |
| `GET` | `/api/movements/toner/{id}` | List all IN/OUT movements for a specific toner |
| `POST` | `/api/movements` | Record a new movement (IN or OUT) |

> Full request/response schemas are available in the Swagger UI when running locally.

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

- ✅ You may **use** and **modify** the software.
- ✅ You may distribute original or modified versions.
- ❌ You **may not** make the project or derivatives **proprietary/closed source** — derivative works must be released under the same GPL-3.0 and remain open source.

See the [LICENSE](LICENSE) file for the full text.