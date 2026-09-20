# Module Breakdown & Functional Specifications

The Student Placement Management System is architectured into 11 interconnected subsystems:

---

## 1. Authentication & Role Authorization Module
- **Purpose:** Manages user identity, encrypted password hashing, session tokens, and role-based permissions.
- **Roles Implemented:**
  - `Admin`: Unrestricted privileges across all institutional records, drives, and reports.
  - `Student`: Confined to the student portal, profile maintenance, and personal applications.
  - `Recruiter`: Optional company-specific self-service role.
- **Key Controller:** `AccountController.cs`

---

## 2. Student Profile & Resume Subsystem
- **Purpose:** Records comprehensive academic metrics and securely manages curriculum vitae uploads.
- **Key Features:**
  - Academic metrics validation: CGPA (0.00-10.00), Active Backlogs, Graduating Batch.
  - Profile readiness percentage calculator.
  - Resume upload with MIME validation, maximum 5 MB constraint, and cryptographically secure GUID file renaming.
  - Verification download endpoint with ownership protection.
- **Key Controller & Service:** `StudentController.cs`, `StudentService.cs`

---

## 3. Academic Department Subsystem
- **Purpose:** Maintains university academic branches and programs.
- **Key Features:**
  - Department code uniqueness checks (e.g. `CSE`, `IT`, `ECE`, `ME`).
  - Referential integrity guard preventing accidental deletion of departments containing registered students or active placement drives.
- **Key Controller:** `DepartmentController.cs`

---

## 4. Corporate Partner & Company Subsystem
- **Purpose:** Organizes partner employer directories, corporate hiring profiles, and HR coordinator details.
- **Key Features:**
  - Company directory with search and industry filtering.
  - Active/Inactive toggle for seasonal hiring partners.
  - Company logo uploads and comprehensive placement history.
- **Key Controller:** `CompanyController.cs`

---

## 5. Placement Drive Management Subsystem
- **Purpose:** Manages the publication of campus recruitment drives and job openings.
- **Key Features:**
  - Multi-department eligibility binding (`DriveDepartments`).
  - Cutoff criteria definition (Min CGPA, Max Backlogs, Passing Year).
  - Time-bounded application windows (`ApplicationStartDate` to `ApplicationEndDate`).
- **Key Controller:** `PlacementDriveController.cs`

---

## 6. Automated Eligibility Engine Subsystem
- **Purpose:** Evaluates student qualifications in real time for any placement drive.
- **Algorithm Rules:**
  1. Department Check: Confirms student's branch is in `DriveDepartments`.
  2. CGPA Cutoff: `Student.CGPA >= Drive.MinCGPA`.
  3. Backlog Cutoff: `Student.Backlogs <= Drive.MaxBacklogs`.
  4. Batch Year: `Student.PassingYear == Drive.PassingYear`.
  5. Application Window: `Now >= ApplicationStartDate && Now <= ApplicationEndDate`.
  6. Duplicate Prevention: `Student hasn't already applied`.
- **Output:** Returns `{ bool IsEligible, string Reason }` with detailed user explanations.
- **Key Service:** `EligibilityService.cs`

---

## 7. Application Management & Pipeline Subsystem
- **Purpose:** Handles application submissions, duplicate blocking, and progression through evaluation stages.
- **Status Lifecycle:** `Applied` -> `UnderReview` -> `Shortlisted` -> `Rejected` or `Selected`.
- **Key Features:**
  - Student application portal with instant confirmation.
  - Administrator review table with multi-criteria filtering and status update modal.
- **Key Controller & Service:** `ApplicationController.cs`, `PlacementService.cs`

---

## 8. Interview Assessment & Scoring Subsystem
- **Purpose:** Configures sequential interview stages and candidate evaluation sheets.
- **Key Features:**
  - Multi-round scheduling (Aptitude, Technical, HR).
  - Online venue or meeting link communication.
  - Candidate evaluation matrix: Enter scores out of 100, pass/fail/absent grading, and feedback notes.
- **Key Controller:** `InterviewController.cs`

---

## 9. Final Selection & Placement Record Subsystem
- **Purpose:** Finalizes job offers and links successful candidates to employers.
- **Key Features:**
  - Declaring a candidate `Selected` automatically generates a `Placement` record.
  - Updates `Student.PlacementStatus` to `Placed`.
  - Dispatches congratulations notifications to the candidate.
  - Offer letter tracking and joining status maintenance.
- **Key Controller & Service:** `PlacementController.cs`, `PlacementService.cs`

---

## 10. In-App Notification Subsystem
- **Purpose:** Delivers contextual alerts to users for recruitment milestones.
- **Categories:** `Placement`, `Interview`, `Application`, `Result`, `General`.
- **Features:**
  - Real-time topbar counter badge with live AJAX polling.
  - Mark individual or all notifications as read.
- **Key Controller & Service:** `NotificationController.cs`, `NotificationService.cs`

---

## 11. Institutional Analytics & Reporting Subsystem
- **Purpose:** Generates executive dashboards and auditable tabular reports for university accreditation.
- **Key Features:**
  - Chart.js visual analytics (Department comparisons, status doughnuts, monthly recruitment trends).
  - Tabbed breakdown by Department, Company, and Selected/Unplaced candidates.
  - In-memory UTF-8 CSV data streaming.
- **Key Controller & Service:** `ReportController.cs`, `ReportService.cs`
