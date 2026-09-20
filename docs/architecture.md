# System Architecture & Layered Design

## 1. Architectural Overview
The application adheres strictly to the **Layered Architecture** pattern built on **ASP.NET Core MVC**. This architectural style enforces clean separation of concerns, high maintainability, testability, and adherence to SOLID principles.

```
┌────────────────────────────────────────────────────────┐
│                   Presentation Layer                   │
│          Razor Views (.cshtml), HTML5, CSS3,           │
│             Bootstrap 5, Vanilla JS, Chart.js          │
└───────────────────────────▲────────────────────────────┘
                            │ Strongly Typed ViewModels
┌───────────────────────────▼────────────────────────────┐
│                    Controller Layer                    │
│        (Routing, Model Binding, Identity Roles,        │
│          HTTP Request Lifecycle, View Dispatch)        │
└───────────────────────────▲────────────────────────────┘
                            │ Dependency Injected Interfaces
┌───────────────────────────▼────────────────────────────┐
│                  Business Service Layer                │
│    IEligibilityService, IStudentService,               │
│    IPlacementService, INotificationService,            │
│    IReportService                                      │
└───────────────────────────▲────────────────────────────┘
                            │ LINQ Queries / Operations
┌───────────────────────────▼────────────────────────────┐
│             Data Access & Persistence Layer            │
│       ApplicationDbContext, EF Core Code-First,        │
│       Fluent API Configurations, Database Seeder       │
└───────────────────────────▲────────────────────────────┘
                            │ ADO.NET / Connection Provider
┌───────────────────────────▼────────────────────────────┐
│                     Database Engine                    │
│      Microsoft SQL Server / LocalDB / SQLite Provider  │
└────────────────────────────────────────────────────────┘
```

---

## 2. Component Layer Responsibilities

### 2.1 Presentation Layer (Views & Assets)
- **Shared Layout (`_Layout.cshtml`):** Dynamically switches navigation trees based on Identity roles (`Admin` vs. `Student`). Incorporates topbar with live notification counter and user profile dropdown.
- **Design System (`dashboard.css`):** Employs an indigo/slate palette, glassmorphism cards, modern typography (Plus Jakarta Sans), and custom status pills.
- **Charting Components:** Embeds responsive Chart.js canvases driven by JSON payloads from the controller layer.

### 2.2 Controller Layer (`Controllers/`)
- Acts as the orchestrator of incoming HTTP requests.
- Enforces declarative security via `[Authorize(Roles = "Admin")]` and `[Authorize(Roles = "Student")]`.
- Contains minimal business logic; delegates complex processing to injected services.
- Translates service responses into strongly-typed ViewModels for rendering or redirects.

### 2.3 Business Service Layer (`Services/`)
- **`EligibilityService`:** Validates candidate academic criteria (CGPA, backlogs, branch, batch) and active application windows.
- **`StudentService`:** Computes profile completion metrics and manages secure resume uploads with GUID renaming.
- **`PlacementService`:** Manages state transitions during candidate hiring, creates placement records, updates student status to `Placed`, and fires notifications.
- **`NotificationService`:** Centralizes dispatch, retrieval, and read-receipt tracking of in-app alerts.
- **`ReportService`:** Performs database-level aggregations and UTF-8 encoded CSV stream generation.

### 2.4 Data Access Layer (`Data/` & `Models/`)
- **`ApplicationDbContext`:** Configures entity sets, precision for monetary CTC and CGPA decimals, unique composite indexes, and cascading constraints.
- **`DbSeeder`:** Seeds initial Identity roles, default administrator credentials, academic branches, partner companies, sample drives, and students.

---

## 3. Dependency Injection Configuration
The application registers all services with the ASP.NET Core Inversion of Control (IoC) container using scoped lifetimes (`AddScoped`), ensuring each HTTP request receives its own isolated context:

```csharp
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IPlacementService, PlacementService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
```
