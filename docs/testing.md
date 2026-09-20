# Quality Assurance & Testing Strategy

## 1. Testing Methodology
The Student Placement Management System underwent comprehensive verification across multiple testing tiers:
1. **Unit & Service Layer Verification:** Testing business logic algorithms (e.g. eligibility calculation, profile completion, duplicate application checks).
2. **Integration & Database Verification:** Testing Entity Framework Core migrations, composite index constraints, cascading rules, and seed pipelines.
3. **Security & Authorization Auditing:** Testing role-based boundaries, horizontal privilege escalation, and anti-forgery tokens.
4. **End-to-End User Flow Verification:** Validating complete workflows from drive publication to candidate selection.

---

## 2. Test Case Matrix

| Test ID | Module | Test Description | Expected Result | Status |
|---|---|---|---|---|
| **TC-01** | Authentication | Admin login with valid credentials (`admin@placement.edu` / `Admin@123`). | Authentication succeeds; user is redirected to `/Admin/Dashboard`. | **PASS** |
| **TC-02** | Authentication | Student login with valid credentials (`raghu.joshi@student.edu` / `Student@123`). | Authentication succeeds; user is redirected to `/Student/Dashboard`. | **PASS** |
| **TC-03** | Authorization | Logged-in student attempts to access `/Admin/Dashboard` or `/Company/Create`. | Access is blocked; system returns HTTP 403 Forbidden / redirects to `/Account/AccessDenied`. | **PASS** |
| **TC-04** | Security | Student attempts to view or edit another student's profile via URL tampering (`/Student/Profile/99`). | System checks user ownership and returns HTTP 403 Forbidden. | **PASS** |
| **TC-05** | Registration | Student registers with duplicate enrollment number. | Validation error displayed: "This enrollment number is already registered." | **PASS** |
| **TC-06** | File Upload | Student uploads `.exe` or executable script instead of resume. | System rejects file with error: "Invalid file format. Allowed extensions: .pdf, .doc, .docx". | **PASS** |
| **TC-07** | File Upload | Student uploads valid `.pdf` resume under 5 MB. | Resume saved with GUID filename in `wwwroot/uploads/resumes/`; path recorded in DB. | **PASS** |
| **TC-08** | Eligibility | Student with CGPA 8.0 applies for drive requiring Min CGPA 7.5. | Eligibility Engine flags `IsEligible = true` and enables the "Apply" button. | **PASS** |
| **TC-09** | Eligibility | Student with 2 backlogs applies for drive allowing max 0 backlogs. | Eligibility Engine flags `IsEligible = false` with explicit explanation: "Your active backlog count exceeds allowed limit." | **PASS** |
| **TC-10** | Applications | Eligible student submits application to open drive. | Application row created with status `Applied`; confirmation notification sent. | **PASS** |
| **TC-11** | Duplicate Prevention | Student attempts to submit second application to same drive. | Request blocked by application validation and DB unique constraint `(DriveId, StudentId)`. | **PASS** |
| **TC-12** | Selection | Admin updates application status to `Selected`. | `Placement` record created, `Student.PlacementStatus` updated to `Placed`, notification dispatched. | **PASS** |
| **TC-13** | Interviews | Admin creates interview round and inputs candidate scores. | Round results saved; student receives interview alert with score and remarks. | **PASS** |
| **TC-14** | Reporting | Admin exports placements report to CSV. | Server streams valid UTF-8 formatted CSV file containing placement headers and data. | **PASS** |
| **TC-15** | Dashboards | Admin Dashboard loads with active drives and student records. | Chart.js renders department bar charts, status doughnut, and monthly trend line. | **PASS** |

---

## 3. Database Constraints Validation

- **Composite Key Constraint (`DriveId, StudentId`):** Successfully verified that duplicate applications generate an EF Core concurrency/unique constraint exception if application-layer validation is bypassed.
- **Foreign Key Cascade Protection:** Successfully verified that deleting a department with enrolled students is blocked to preserve data integrity.
- **Decimal Precision:** Verified that CGPA values like `8.65` and CTC values like `9.50` are stored accurately without floating-point truncation.
