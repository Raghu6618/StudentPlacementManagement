# Database Design & Entity Relationship

## 1. Database Specifications
- **Database Engine:** Microsoft SQL Server (Compatible with LocalDB and SQL Server 2019/2022)
- **Database Name:** `StudentPlacementManagementDb`
- **ORM:** Entity Framework Core 10 (Code First)

---

## 2. Entity Relationship Diagram (ERD) Description

```
  ┌─────────────────┐
  │   AspNetUsers   │
  └────────┬────────┘
           │ 1:1
           ▼
  ┌─────────────────┐       1:N       ┌───────────────────┐
  │     Students    │◄────────────────┤    Departments    │
  └────────┬────────┘                 └─────────┬─────────┘
           │                                    │
           │ 1:N                                │ 1:N
           ▼                                    ▼
  ┌─────────────────┐       N:1       ┌───────────────────┐
  │   Applications  ├────────────────►│  DriveDepartments │
  └────────┬────────┘                 └─────────▲─────────┘
           │                                    │
           │ N:1                                │ N:1
           ▼                                    │
  ┌─────────────────┐◄──────────────────────────┘
  │ PlacementDrives │
  └────────┬────────┘
           │
           ├────────────► 1:N ──► ┌───────────────────┐
           │                      │  InterviewRounds  │
           │                      └─────────┬─────────┘
           │                                │ 1:N
           │                                ▼
           │                      ┌───────────────────┐
           │                      │   RoundResults    │
           │                      └───────────────────┘
           │
           │ 1:N
           ▼
  ┌─────────────────┐       N:1       ┌───────────────────┐
  │    Placements   ├────────────────►│     Companies     │
  └─────────────────┘                 └───────────────────┘
```

---

## 3. Database Schema & Tables

### 3.1 `Students` Table
Stores student demographic, academic, and placement status information.
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `StudentId` | INT | NO (PK, Identity) | Primary Key |
| `UserId` | NVARCHAR(450) | NO (FK) | Foreign Key to `AspNetUsers.Id` |
| `EnrollmentNo` | NVARCHAR(50) | NO | Unique Enrollment Number (Unique Index) |
| `FirstName` | NVARCHAR(50) | NO | Candidate first name |
| `LastName` | NVARCHAR(50) | NO | Candidate last name |
| `Gender` | NVARCHAR(10) | YES | Male, Female, Other |
| `DateOfBirth` | DATE | YES | Candidate birthdate |
| `Phone` | NVARCHAR(20) | YES | Contact telephone |
| `Email` | NVARCHAR(100) | NO | Institutional / personal email |
| `Address` | NVARCHAR(250) | YES | Residential street address |
| `City` | NVARCHAR(50) | YES | City of residence |
| `State` | NVARCHAR(50) | YES | State of residence |
| `DepartmentId` | INT | NO (FK) | Foreign key to `Departments.DepartmentId` |
| `PassingYear` | INT | NO | Year of graduation (e.g. 2026) |
| `CGPA` | DECIMAL(4,2) | NO | Cumulative GPA (0.00 - 10.00) |
| `Backlogs` | INT | NO | Active backlog count |
| `ResumeFileName` | NVARCHAR(255) | YES | Original uploaded file name |
| `ResumePath` | NVARCHAR(500) | YES | Relative server disk path |
| `ProfileImagePath` | NVARCHAR(500) | YES | Profile photo file path |
| `PlacementStatus` | NVARCHAR(20) | NO | Status: `NotPlaced` or `Placed` |
| `CreatedAt` | DATETIME2 | NO | Creation timestamp (UTC) |
| `UpdatedAt` | DATETIME2 | YES | Last modified timestamp |

### 3.2 `Departments` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `DepartmentId` | INT | NO (PK) | Primary Key |
| `DepartmentName`| NVARCHAR(100)| NO | e.g. Computer Science & Engineering |
| `DepartmentCode`| NVARCHAR(20) | NO | Unique acronym (e.g. CSE) |
| `CreatedAt` | DATETIME2 | NO | Creation timestamp |

### 3.3 `Companies` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `CompanyId` | INT | NO (PK) | Primary Key |
| `CompanyName` | NVARCHAR(100)| NO | Corporate partner name |
| `Website` | NVARCHAR(200)| YES | Company URL |
| `Industry` | NVARCHAR(100)| YES | e.g. Software Development, FinTech |
| `Description` | NVARCHAR(2000)| YES | Company profile description |
| `Location` | NVARCHAR(100)| YES | Primary hiring location |
| `HRName` | NVARCHAR(100)| YES | HR coordinator name |
| `HREmail` | NVARCHAR(100)| YES | HR coordinator email |
| `HRPhone` | NVARCHAR(20) | YES | HR coordinator telephone |
| `LogoPath` | NVARCHAR(500)| YES | Relative path to logo image |
| `IsActive` | BIT | NO | Active status toggle |
| `CreatedAt` | DATETIME2 | NO | Creation timestamp |
| `UpdatedAt` | DATETIME2 | YES | Last modified timestamp |

### 3.4 `PlacementDrives` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `DriveId` | INT | NO (PK) | Primary Key |
| `CompanyId` | INT | NO (FK) | Foreign Key to `Companies.CompanyId` |
| `JobRole` | NVARCHAR(100)| NO | Role designation |
| `JobDescription`| NVARCHAR(3000)| NO | Role details and responsibilities |
| `JobLocation` | NVARCHAR(100)| YES | Work location |
| `EmploymentType`| NVARCHAR(50)| NO | Full Time, Internship |
| `SalaryPackage` | DECIMAL(10,2)| NO | Annual CTC in Lakhs (₹ LPA) |
| `MinCGPA` | DECIMAL(4,2) | NO | Minimum CGPA cutoff |
| `MaxBacklogs` | INT | NO | Maximum active backlogs permitted |
| `PassingYear` | INT | NO | Target graduation year |
| `DriveDate` | DATE | NO | Scheduled date of recruitment drive |
| `ApplicationStartDate`| DATETIME2 | NO | Application opening window |
| `ApplicationEndDate` | DATETIME2 | NO | Application closing deadline |
| `RequiredSkills`| NVARCHAR(500)| YES | Comma-separated technical skills |
| `EligibilityDescription`| NVARCHAR(1000)| YES | Additional eligibility notes |
| `Status` | NVARCHAR(20) | NO | `Upcoming`, `Open`, `Closed`, `Completed`, `Cancelled` |

### 3.5 `DriveDepartments` Table
Allows a single drive to target multiple academic branches.
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `DriveDepartmentId` | INT | NO (PK) | Primary Key |
| `DriveId` | INT | NO (FK) | Foreign Key to `PlacementDrives` |
| `DepartmentId` | INT | NO (FK) | Foreign Key to `Departments` |
*(Unique composite index on `DriveId + DepartmentId`)*

### 3.6 `Applications` Table
Tracks student submissions for placement drives.
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `ApplicationId` | INT | NO (PK) | Primary Key |
| `DriveId` | INT | NO (FK) | Foreign Key to `PlacementDrives` |
| `StudentId` | INT | NO (FK) | Foreign Key to `Students` |
| `AppliedDate` | DATETIME2 | NO | Submission timestamp (UTC) |
| `Status` | NVARCHAR(30) | NO | `Applied`, `UnderReview`, `Shortlisted`, `Rejected`, `Selected`, `Withdrawn` |
| `Remarks` | NVARCHAR(500)| YES | Review notes / feedback |
| `UpdatedAt` | DATETIME2 | YES | Last status update timestamp |
*(Unique composite index on `DriveId + StudentId` prevents duplicate applications)*

### 3.7 `InterviewRounds` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `RoundId` | INT | NO (PK) | Primary Key |
| `DriveId` | INT | NO (FK) | Foreign Key to `PlacementDrives` |
| `RoundName` | NVARCHAR(100)| NO | e.g. Technical Interview |
| `RoundNumber` | INT | NO | Sequential round order (1, 2, 3...) |
| `RoundDate` | DATETIME2 | NO | Scheduled date & time |
| `Description` | NVARCHAR(1000)| YES | Venue or video meeting link |
| `IsRequired` | BIT | NO | Mandatory elimination round flag |
*(Unique composite index on `DriveId + RoundNumber`)*

### 3.8 `RoundResults` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `RoundResultId` | INT | NO (PK) | Primary Key |
| `RoundId` | INT | NO (FK) | Foreign Key to `InterviewRounds` |
| `StudentId` | INT | NO (FK) | Foreign Key to `Students` |
| `Status` | NVARCHAR(20) | NO | `Pending`, `Passed`, `Failed`, `Absent` |
| `Score` | DECIMAL(5,2) | YES | Score achieved (0-100) |
| `Remarks` | NVARCHAR(500)| YES | Evaluation feedback |

### 3.9 `Placements` Table
Stores officially finalized job offers.
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `PlacementId` | INT | NO (PK) | Primary Key |
| `StudentId` | INT | NO (FK) | Foreign Key to `Students` |
| `DriveId` | INT | NO (FK) | Foreign Key to `PlacementDrives` |
| `CompanyId` | INT | NO (FK) | Foreign Key to `Companies` |
| `JobRole` | NVARCHAR(100)| NO | Hired designation |
| `SalaryPackage`| DECIMAL(10,2)| NO | Final offered CTC (LPA) |
| `PlacementDate`| DATE | NO | Offer date |
| `JoiningDate` | DATE | YES | Tentative joining date |
| `Status` | NVARCHAR(30) | NO | `Selected`, `OfferReceived`, `Joined`, `NotJoined` |
| `Remarks` | NVARCHAR(500)| YES | Offer notes |

### 3.10 `Notifications` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `NotificationId`| INT | NO (PK) | Primary Key |
| `UserId` | NVARCHAR(450)| NO (FK) | Foreign Key to `AspNetUsers.Id` |
| `Title` | NVARCHAR(150)| NO | Alert heading |
| `Message` | NVARCHAR(1000)| NO | Alert body |
| `Type` | NVARCHAR(50) | NO | `General`, `Placement`, `Application`, `Interview`, `Result` |
| `IsRead` | BIT | NO | Read receipt toggle |
| `CreatedAt` | DATETIME2 | NO | Dispatch timestamp |

### 3.11 `AuditLogs` Table
| Column | Data Type | Nullable | Description |
|---|---|---|---|
| `AuditLogId` | INT | NO (PK) | Primary Key |
| `UserId` | NVARCHAR(450)| YES | Foreign Key to `AspNetUsers.Id` |
| `Action` | NVARCHAR(100)| NO | e.g. Application Shortlisted |
| `EntityName` | NVARCHAR(100)| NO | e.g. Placement |
| `EntityId` | NVARCHAR(100)| YES | Target record ID |
| `Description` | NVARCHAR(2000)| YES | Transaction details |
| `CreatedAt` | DATETIME2 | NO | Transaction timestamp |
