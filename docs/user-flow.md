# Complete User Workflow & Recruitment Lifecycle

## 1. End-to-End Placement Workflow Diagram

```
[ Placement Officer ]                     [ Student ]
        │                                      │
        ├─► 1. Registers Partner Company       ├─► 2. Registers Student Account
        │      (e.g., TCS, Infosys)            │      (Enters Dept, Batch, Roll No)
        │                                      │
        ├─► 3. Creates Placement Drive         ├─► 4. Logs into Student Portal
        │      - Sets Salary CTC (LPA)         │
        │      - Sets Min CGPA Cutoff          ├─► 5. Updates Profile & CGPA
        │      - Sets Max Backlog Cutoff       │
        │      - Selects Eligible Branches     ├─► 6. Uploads Verified Resume
        │      - Sets Application Deadline     │      (PDF/Word, Max 5MB)
        │                                      │
        │             [ Automated Eligibility Engine ]
        │             Evaluates student criteria against drive
        │             Displays Qualified / Ineligible Badge
        │                                      │
        │                                      ├─► 7. Clicks "Apply for Drive"
        │                                      │      (Status: Applied)
        │                                      │
        ├─► 8. Reviews Applications Table      ◄──────┤
        │      (Filters by Branch/CGPA)        │
        │                                      │
        ├─► 9. Shortlists Qualified Candidates │
        │      (Status: Shortlisted)           │
        │      - Triggers Notification ───────►├─► 10. Receives Shortlist Alert
        │                                      │
        ├─► 11. Schedules Interview Rounds     │
        │       - Round 1: Aptitude Test       ├─► 12. Views Interview Schedule
        │       - Round 2: Technical Interview │       (Date, Time, Link/Venue)
        │       - Round 3: HR Round            │
        │                                      ├─► 13. Attends Interview Rounds
        │                                      │
        ├─► 14. Enters Candidate Evaluation    │
        │       (Scores, Pass / Fail / Absent) ├─► 15. Views Evaluation Results
        │                                      │
        ├─► 16. Declares Final Selection       │
        │       (Status: Selected)             │
        │       - Generates Placement Record   │
        │       - Updates Student: Placed      │
        │       - Dispatches Alert ───────────►├─► 17. Receives Offer Notice
        │                                      │       Views Official Offer Card
        │                                      │       (Package, Company, Joining)
        │                                      │
        ├─► 18. Executive Dashboard Updated    │
        │       (Placement % Recalculated)     │
        │                                      │
        └─► 19. Generates Accredited Reports   │
                (Exports CSV for NAAC/NBA)     ▼
```

---

## 2. Step-by-Step Scenario Walkthrough

### Scenario: TCS Campus Recruitment Drive
1. **Drive Creation:**
   - The Placement Officer creates a drive for **Tata Consultancy Services (TCS)** for the role of **"Graduate Engineer Trainee"** with CTC **₹4.50 LPA**.
   - Eligibility rules are configured: Minimum CGPA: **6.50**, Maximum Backlogs: **1**, Target Passing Year: **2026**, Eligible Branches: **CSE, IT, ECE**.
   - Application deadline set for 7 days ahead.

2. **Student Discovery & Application:**
   - **Student Raghu (CSE, CGPA 8.65, 0 Backlogs):**
     - Navigates to Placement Drives.
     - The Eligibility Engine displays a **Green Banner**: *"You satisfy all eligibility criteria."*
     - Raghu clicks **"Apply for Drive"**. Application status becomes `Applied`.
   - **Student Vikram (CSE, CGPA 5.90, 3 Backlogs):**
     - Navigates to the same drive.
     - The Eligibility Engine displays a **Red Banner**: *"CGPA (5.90) is below required minimum of 6.50. Your active backlog count (3) exceeds the allowed limit (1)."*
     - Application submission is locked.

3. **Shortlisting & Interview Management:**
   - The Placement Officer reviews applicants, verifies Raghu's resume, and clicks **"Shortlist"**.
   - The officer creates **Round 1 (Online Aptitude & Coding)** scheduled for Friday at 10:00 AM UTC.
   - Raghu receives an automated in-app notification: *"Interview Scheduled: Online Aptitude & Coding"*.

4. **Grading & Final Selection:**
   - After the test, the Placement Officer enters the Grading sheet, marks Raghu as **"Passed"** with score **88.5/100**.
   - Technical and HR rounds are similarly conducted and cleared.
   - The officer marks the application status as **"Selected"** with confirmed CTC of ₹4.50 LPA.
   - The system automatically:
     1. Creates a row in `Placements`.
     2. Updates Raghu's `PlacementStatus` to `Placed`.
     3. Sends a congratulatory notification.
     4. Updates the campus placement percentage on the Admin Dashboard.

5. **Reporting & Accreditation:**
   - The officer navigates to `Analytics & Reports`, reviews the updated CSE department placement ratio, and clicks **"Export Placements (CSV)"** to download the official hiring audit sheet.
