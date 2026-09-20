# Student Placement Management System

A production-quality, responsive web application for managing the complete campus placement recruitment lifecycle, built using **ASP.NET Core MVC, C#, Entity Framework Core, SQL Server, and ASP.NET Core Identity**.

Designed specifically for college campus recruitment cells, placement officers, students, and corporate recruiters for final-year project evaluations and viva presentation.

---

## 🌟 Key Features

### 👨‍💼 Administrator / Placement Officer
- **Executive Analytics Dashboard:** Real-time metrics for total students, active drives, total applications, shortlisted count, placement percentage, and package CTC stats.
- **Visual Analytics (Chart.js):**
  - Department-wise placement comparison chart.
  - Placement status breakdown (Placed vs. Unplaced).
  - Top hiring companies and offer counts.
  - Monthly recruitment trend line chart.
- **Student Directory:** Search, filter by department, batch, or placement status, and view verified academic credentials and resumes.
- **Department Management:** Full CRUD management of university branches with safety checks to prevent orphan records.
- **Company Management:** Register recruiting partners, HR coordinators, corporate profiles, and logos, with active/inactive toggles.
- **Placement Drive Management:**
  - Create and configure job drives with minimum CGPA, max backlogs allowed, graduation year, application start/end windows, and salary CTC.
  - Multi-department eligibility mapping (`DriveDepartments`).
  - **Automated Eligibility Engine:** Automatically identifies and lists all students across the college meeting drive criteria.
- **Application Review:** Review candidate applications, filter by drive/company/status, record evaluation remarks, and manage stage progressions (`UnderReview`, `Shortlisted`, `Rejected`, `Selected`).
- **Interview Rounds & Grading Sheet:**
  - Define sequential interview rounds (`Aptitude Test`, `Technical Interview`, `HR Round`, `Group Discussion`).
  - Candidate evaluation matrix: Enter scores out of 100, pass/fail/absent grading, and feedback notes.
- **Final Selection & Placements:**
  - Selecting a student automatically creates official `Placement` records, marks `Student.PlacementStatus = 'Placed'`, and dispatches congratulatory notifications.
- **Auditable Reports & CSV Export:**
  - Overall placement metrics.
  - Department-wise placement performance.
  - Company-wise selection statistics.
  - One-click CSV downloads for Placements and Student rosters.

### 🎓 Student
- **Self-Registration & Secure Login:** Role-restricted student registration linking Identity accounts with verified enrollment numbers.
- **Student Dashboard:**
  - Personalized welcome banner with CGPA, backlog count, and Placement Status badge (`Placed` / `Not Placed`).
  - Profile readiness percentage indicator.
  - Quick statistics: Available Drives, Applications Submitted, Shortlisted, Offers Received.
  - Open placement drives filtered for the student's department.
- **Profile & Resume Manager:**
  - Update personal, contact, and academic details.
  - Secure resume upload supporting `.pdf`, `.doc`, `.docx` up to 5MB with unique GUID file safety.
  - View and download verified resumes.
- **Placement Drive Explorer & Real-Time Eligibility Engine:**
  - Explore active and upcoming drives.
  - Real-time eligibility evaluation giving clear explanations (e.g. *"Student satisfies all eligibility criteria"* vs *"CGPA (6.85) is below required minimum of 7.50"*).
  - One-click application submission with duplicate prevention.
- **Application & Interview Tracker:**
  - Real-time status badges (`Applied`, `Shortlisted`, `Rejected`, `Selected`).
  - Scheduled interview dates, times, venues, instructions, and evaluator scores.
- **Official Placement Offer:**
  - Dedicated offer card with company name, job role, package, joining date, and coordinator remarks.
- **In-App Notification Center:** Real-time alerts for application confirmations, interview schedules, test results, and final selections.

---

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| **Backend Framework** | ASP.NET Core MVC (Targeting .NET 10.0 / .NET LTS) |
| **Programming Language** | C# (Modern syntax, async/await, nullable reference types) |
| **ORM & Data Access** | Entity Framework Core 10 (Code First with Migrations) |
| **Database** | Microsoft SQL Server / LocalDB (with resilient SQLite fallback for offline portable demos) |
| **Authentication & Auth** | ASP.NET Core Identity (Role-based authorization: `Admin`, `Student`, `Recruiter`) |
| **Frontend UI** | HTML5, CSS3, Razor Views, Bootstrap 5.3.3 |
| **Icons** | Bootstrap Icons 1.11.3 |
| **Charts** | Chart.js |
| **Architecture** | Layered Architecture (Controllers, Services, Models, ViewModels, Data) |

---

## 🏗️ Project Architecture

```
StudentPlacementManagement/
│
├── Controllers/                  # MVC Controllers with [Authorize] role checks
│   ├── AccountController.cs      # Login, Student self-registration, Logout, AccessDenied
│   ├── AdminController.cs        # Admin Dashboard KPIs and Chart.js datasets
│   ├── StudentController.cs      # Student Dashboard, Profile, Edit, Resume Upload/Download
│   ├── DepartmentController.cs   # Academic department management CRUD
│   ├── CompanyController.cs      # Corporate recruiting partner directory and details
│   ├── PlacementDriveController.cs # Placement drives, criteria, and automated eligibility
│   ├── ApplicationController.cs  # Application submissions, duplicate prevention, status updates
│   ├── InterviewController.cs    # Interview round scheduling and candidate score grading
│   ├── PlacementController.cs    # Confirmed placements and offer tracking
│   ├── NotificationController.cs # In-app notification center and live counter badge
│   └── ReportController.cs       # Comprehensive placement reports and CSV exports
│
├── Data/                         # EF Core Data Context and Seeder
│   ├── ApplicationDbContext.cs   # DbSets, Fluent API mappings, indexes, cascading rules
│   └── DbSeeder.cs               # Seeds default Admin, roles, sample drives, students, and companies
│
├── Models/                       # Domain Entities
│   ├── ApplicationUser.cs        # Identity user extension
│   ├── Student.cs                # Student profile and placement status
│   ├── Department.cs             # Academic departments
│   ├── Company.cs                # Recruiting companies
│   ├── PlacementDrive.cs         # Job drive requirements and criteria
│   ├── DriveDepartment.cs        # Many-to-many drive & department mapping
│   ├── Application.cs            # Student drive applications
│   ├── InterviewRound.cs         # Sequential interview rounds
│   ├── RoundResult.cs            # Candidate round evaluation and scores
│   ├── Placement.cs              # Finalized placement offers
│   ├── Notification.cs           # User notifications
│   └── AuditLog.cs               # Administrator audit trails
│
├── ViewModels/                   # Strongly-typed models for Views and Dashboards
├── Services/                     # Business Logic Layer (Separation of Concerns)
│   ├── Interfaces/
│   │   ├── IEligibilityService.cs
│   │   ├── IStudentService.cs
│   │   ├── IPlacementService.cs
│   │   ├── INotificationService.cs
│   │   └── IReportService.cs
│   ├── EligibilityService.cs
│   ├── StudentService.cs
│   ├── PlacementService.cs
│   ├── NotificationService.cs
│   └── ReportService.cs
│
├── Views/                        # Responsive Razor Views
│   ├── Account/                  # Login, Register, AccessDenied
│   ├── Admin/                    # Admin Dashboard
│   ├── Student/                  # Student Dashboard, Profile, Edit, Directory
│   ├── Department/               # Department CRUD views
│   ├── Company/                  # Company Directory, Details, Create, Edit
│   ├── PlacementDrive/           # Drive List, Details, Create, Edit, Eligible Students
│   ├── Application/              # Applications list, MyApplications
│   ├── Interview/                # Rounds, CreateRound, Grading, MyInterviews
│   ├── Placement/                # Placements list, Details, MyOffer
│   ├── Notification/             # Notification Center
│   ├── Report/                   # Analytics and CSV export hub
│   └── Shared/                   # _Layout, _ValidationScriptsPartial, Error
│
├── wwwroot/                      # Static Assets
│   ├── css/
│   │   └── dashboard.css         # Modern design system and custom styling
│   ├── js/
│   │   └── site.js               # Sidebar toggle, live notification polling
│   └── uploads/                  # Secure uploads folder
│       ├── resumes/              # Uploaded PDF/Word student resumes
│       └── company-logos/        # Corporate partner logos
│
├── Migrations/                   # EF Core Code First Migrations
├── docs/                         # Final-year project viva & academic documentation
├── appsettings.json              # Connection strings and configurable parameters
└── Program.cs                    # Application pipeline, DI configuration, and startup seeding
```

---

## 🚀 Setup & Execution Guide

### 1. Prerequisites
- **.NET SDK 10.0** (or .NET 8.0/9.0 LTS). Verify with:
  ```bash
  dotnet --version
  ```
- **Microsoft SQL Server** (LocalDB, SQL Server Express, or Full SQL Server instance).
- Optional: `dotnet-ef` CLI tool:
  ```bash
  dotnet tool install --global dotnet-ef
  ```
  *(A local tool manifest is already pre-configured in this repository, so you can also run `dotnet dotnet-ef`).*

---

### 2. Database & Connection String Setup
Open `appsettings.json` and configure your SQL Server connection string under `ConnectionStrings:DefaultConnection`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StudentPlacementManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
    "SqliteConnection": "Data Source=StudentPlacementManagement.db"
  },
  "DatabaseProvider": "SqlServer"
}
```

> **Note on Portability:** The application includes resilient database initialization. If local SQL Server is not currently installed or running on your evaluation machine, it will automatically fall back to SQLite (`StudentPlacementManagement.db`) so the system **never crashes and works out-of-the-box on any machine**.

---

### 3. Build and Run Commands

1. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

2. **Compile the solution:**
   ```bash
   dotnet build
   ```

3. **(Optional) Apply EF Core Migrations manually:**
   ```bash
   dotnet dotnet-ef database update
   ```
   *(Migrations and seeding are also automatically verified and executed on first application launch via `Program.cs`).*

4. **Launch the Web Application:**
   ```bash
   dotnet run
   ```
   By default, the server will start listening at:
   - **`http://localhost:5050`** (or `https://localhost:7000`)

---

## 🔑 Default Development Credentials

On first run, the database seeder automatically creates roles and sample accounts:

| Role | Email | Password | Description |
|---|---|---|---|
| **Placement Officer (Admin)** | `admin@placement.edu` | `Admin@123` | Complete system configuration, student management, drives, placements, reports. |
| **Student 1 (Placed)** | `raghu.joshi@student.edu` | `Student@123` | CSE Student, CGPA 8.65, Placed at TCS (₹4.5 LPA). |
| **Student 2 (Shortlisted)** | `priya.patel@student.edu` | `Student@123` | IT Student, CGPA 7.80, 0 Backlogs, Shortlisted for drives. |
| **Student 3 (Active)** | `amit.verma@student.edu` | `Student@123` | CSE Student, CGPA 6.85, 1 Backlog, Active applicant. |

> *Tip: The Login page includes convenient **Quick Demo Credentials buttons** that fill in credentials with a single click.*

---

## 📋 Comprehensive Recruitment Workflow

```
Administrator                        Eligibility Engine                      Student
     │                                       │                                  │
     ├─► Creates Company & Placement Drive ──┼──────────────────────────────────┤
     │   (Role, Package, Min CGPA, Batch)    │                                  │
     │                                       │                                  │
     │                                       ├─► Evaluates Branch, CGPA, ───────┼─► Browses Drives & Eligibility
     │                                       │   Backlogs, Passing Year         │   (Sees Green / Red status badge)
     │                                       │                                  │
     │                                       │                                  ├─► Clicks "Apply for Drive"
     │   ◄── Dispatches Real-time Alert ─────┼── Checks duplicates & deadline ──┤   (Status = Applied)
     │                                       │                                  │
     ├─► Reviews Candidate Applications ─────┼──────────────────────────────────┤
     │   (Shortlist / Reject candidate)      │                                  │
     │                                       ├─► Dispatches Notification ───────┼─► Receives Shortlist Alert
     │                                       │                                  │
     │                                       │                                  ├─► Checks Interview Schedule
     ├─► Creates Interview Rounds ───────────┼──────────────────────────────────┤   (Date, Venue, Round details)
     │   (Aptitude -> Technical -> HR)       │                                  │
     │                                       │                                  │
     ├─► Evaluates Round Results ────────────┼──────────────────────────────────┼─► Receives Score & Remarks
     │   (Enters Score, Pass / Fail)         │                                  │
     │                                       │                                  │
     ├─► Declares Final Selection ───────────┼──────────────────────────────────┤
     │   (Application.Status = Selected)     │                                  │
     │   (Placement Record Created)          ├─► Updates Placement Status ──────┼─► Placement Status: Placed
     │   (Student.PlacementStatus = Placed)  │   Dispatches Congratulations     │   Views Official Offer Card
     │                                       │                                  │
     ▼                                       ▼                                  ▼
Admin Dashboard & Reports Updated                                     Student Placement Complete
```

---

## 🔒 Security & Validation Highlights

1. **Role-Based Authorization:** Powered by ASP.NET Core Identity. Access is secured using `[Authorize(Roles = "Admin")]` and `[Authorize(Roles = "Student")]`.
2. **Horizontal Privilege Escalation Protection:** Students can only view and update their own profiles and applications. Attempting to change an ID in the URL is verified against the logged-in user's identity.
3. **Anti-Forgery Protection:** All state-modifying requests enforce `@Html.AntiForgeryToken()` and `[ValidateAntiForgeryToken]`.
4. **Secure File Uploads:** Uploaded resumes are strictly validated for allowed extensions (`.pdf`, `.doc`, `.docx`), MIME types, and file size limits (configurable in `appsettings.json`). Files are assigned cryptographically random GUID filenames to prevent path traversal or shell execution.
5. **Duplicate Prevention:** Unique composite database constraints on `(DriveId, StudentId)` ensure students cannot apply twice to the same recruitment drive.

---

## 📁 Academic Viva Documentation

Comprehensive final-year engineering viva documentation is located in the `docs/` folder:
- **`docs/project-overview.md`:** Project introduction, problem statement, objectives, and scope.
- **`docs/system-requirements.md`:** Hardware, software, functional, and non-functional specifications.
- **`docs/architecture.md`:** Layered MVC architecture, dependency injection, and data flow.
- **`docs/database-design.md`:** Entity-Relationship diagram description, table schemas, and keys.
- **`docs/modules.md`:** Deep dive into all 11 subsystems of the placement portal.
- **`docs/user-flow.md`:** End-to-end recruitment process workflow from drive to hiring.
- **`docs/testing.md`:** Test cases, validation matrices, and authorization security tests.
- **`docs/future-enhancements.md`:** Documented roadmap for AI resume parsing, automated aptitude testing, and mobile notifications.

---

## 📜 License & Academic Integrity
This project was developed for academic viva evaluation, demonstration, and college placement management. Free for educational and institutional adaptation.
