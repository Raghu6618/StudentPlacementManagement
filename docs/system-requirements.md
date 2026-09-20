# System Requirements Specification (SRS)

## 1. Functional Requirements

### 1.1 Authentication & Role-Based Authorization Module
- **FR-1.1:** The system shall support role-based authorization for `Admin`, `Student`, and optional `Recruiter` roles using ASP.NET Core Identity.
- **FR-1.2:** Unauthenticated users shall be automatically redirected to the login interface when attempting to access restricted resources.
- **FR-1.3:** The registration portal shall only permit student self-registration; administrator accounts cannot be created publicly.
- **FR-1.4:** Passwords shall be securely hashed using standard PBKDF2 with HMAC-SHA256.

### 1.2 Student Profile & Resume Module
- **FR-2.1:** Students shall maintain personal details (First Name, Last Name, Gender, DOB, Phone, Address, City, State).
- **FR-2.2:** Students shall record verified academic metrics (Enrollment Number, Department, Graduating Batch, CGPA, Backlogs).
- **FR-2.3:** Students shall upload resumes in `.pdf`, `.doc`, or `.docx` format up to 5 MB.
- **FR-2.4:** The system shall compute a profile readiness percentage based on required fields and verified resume upload.
- **FR-2.5:** Students shall only view and modify their own records; URL tampering must return HTTP 403 Forbidden.

### 1.3 Company & Placement Drive Module
- **FR-3.1:** Administrators shall manage partner companies with HR contact details, website URLs, and corporate logos.
- **FR-3.2:** Administrators shall publish placement drives defining job roles, CTC packages, locations, and application deadlines.
- **FR-3.3:** Placement drives shall support many-to-many relationships with eligible academic departments via `DriveDepartments`.

### 1.4 Eligibility Engine
- **FR-4.1:** The system shall evaluate a student's eligibility for any drive based on:
  1. Student's academic department is linked to the drive.
  2. Student's CGPA is greater than or equal to `Drive.MinCGPA`.
  3. Student's active backlogs are less than or equal to `Drive.MaxBacklogs`.
  4. Student's graduating batch matches `Drive.PassingYear`.
  5. The current UTC timestamp is between `ApplicationStartDate` and `ApplicationEndDate`.
  6. The student has not already applied to the drive.
- **FR-4.2:** The system shall provide an explicit natural language reason explaining eligibility or ineligibility.

### 1.5 Application & Recruitment Pipeline
- **FR-5.1:** Eligible students shall submit applications with duplicate prevention enforced via unique database constraints `(DriveId, StudentId)`.
- **FR-5.2:** Administrators shall review applications and update stages: `UnderReview`, `Shortlisted`, `Rejected`, and `Selected`.
- **FR-5.3:** Selecting a candidate shall create an official `Placement` record and update `Student.PlacementStatus = 'Placed'`.

### 1.6 Interview Management & Scoring
- **FR-6.1:** Administrators shall schedule sequential interview rounds (`Aptitude Test`, `Technical Interview`, `HR Round`).
- **FR-6.2:** Administrators shall record candidate scores (0-100) and evaluation status (`Passed`, `Failed`, `Absent`, `Pending`).

### 1.7 Notifications & Analytics
- **FR-7.1:** The system shall dispatch in-app notifications upon critical milestones (application submitted, shortlisted, interview scheduled, offer declared).
- **FR-7.2:** Administrators shall access real-time Chart.js dashboards and export reports to CSV format.

---

## 2. Non-Functional Requirements

### 2.1 Security & Data Integrity
- **NFR-1:** All forms shall enforce anti-forgery token verification (`[ValidateAntiForgeryToken]`).
- **NFR-2:** SQL Injection prevention shall be guaranteed through parameterized queries and Entity Framework Core.
- **NFR-3:** Uploaded file names shall be replaced with random GUIDs to prevent path traversal and arbitrary file execution.

### 2.2 Usability & Responsiveness
- **NFR-4:** The user interface shall be fully responsive across desktop (>=1200px), tablet (768px-1199px), and mobile (<=767px) devices using Bootstrap 5.
- **NFR-5:** Color contrasts and typography shall adhere to modern accessible web standards.

### 2.3 Performance & Reliability
- **NFR-6:** Average page response times under standard institutional load shall not exceed 1.5 seconds.
- **NFR-7:** Resilient database connection handling shall ensure seamless fallback for local evaluation.

---

## 3. Hardware & Software Requirements

| Component | Minimum Specification | Recommended Specification |
|---|---|---|
| **Operating System** | Windows 10 / 11, Linux, macOS | Windows 11 / Linux |
| **Processor** | Dual-core 2.0 GHz (x64 / ARM64) | Quad-core 2.5 GHz or higher |
| **RAM** | 4 GB | 8 GB or 16 GB |
| **Disk Space** | 500 MB free space | 2 GB free SSD space |
| **Runtime / SDK** | .NET SDK 10.0 (or .NET 8.0/9.0) | .NET 10.0 SDK |
| **Database Engine** | SQL Server 2019+ / LocalDB / SQLite | SQL Server 2022 Developer Edition |
| **Browser** | Chrome 100+, Edge 100+, Firefox 100+ | Latest Google Chrome / Microsoft Edge |
