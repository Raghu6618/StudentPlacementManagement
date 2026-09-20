# Project Overview: Student Placement Management System

## 1. Introduction
The **Student Placement Management System** is an enterprise-grade academic web application developed using **ASP.NET Core MVC, C#, Entity Framework Core, SQL Server, and ASP.NET Core Identity**. The system is engineered to automate and optimize the end-to-end campus recruitment operations for engineering and postgraduate colleges.

## 2. Problem Statement
Traditional placement management in educational institutions relies on disparate spreadsheets, paper-based forms, manual email notifications, and ad-hoc physical record-keeping. This legacy approach suffers from several critical bottlenecks:
- **Inefficient Eligibility Verification:** Manually checking whether hundreds of students meet varying company requirements (e.g. branch, minimum CGPA, active backlog limits) is prone to calculation errors.
- **Communication Latency:** Students frequently miss drive deadlines, test schedules, or interview calls due to buried emails and unorganized notice boards.
- **Data Inconsistencies & Duplication:** Students inadvertently apply multiple times or submit outdated resumes.
- **Lack of Real-Time Analytics:** Placement officers spend days manually compiling placement percentages, department-wise recruitment ratios, and recruiter metrics for university accreditation (e.g., NAAC, NBA).

## 3. Proposed System
The proposed system addresses these challenges by introducing a centralized, web-based platform with role-based access for Administrators (Placement Officers) and Students:
- **Automated Eligibility Engine:** Dynamically calculates student qualification for any job drive using an algorithm based on branch mapping, CGPA, backlogs, and graduating batch.
- **Structured Recruitment Pipeline:** Manages the student journey across five distinct stages: Applied -> Shortlisted -> Interview Rounds -> Final Selection -> Official Placement.
- **In-App Notification Hub:** Dispatches real-time alerts upon application submission, shortlisting, round scheduling, and final job offers.
- **Analytical Dashboards & Charting:** Leverages Chart.js to render real-time graphical insights on placement percentages, department performances, top recruiting companies, and monthly recruitment trends.
- **Auditable Reporting:** Provides multi-dimensional tabular reports with instant CSV data streaming for institutional compliance.

## 4. System Objectives
1. Eliminate manual eligibility tracking through rule-driven validation.
2. Provide transparency to students regarding their application status, interview schedules, and evaluation scores.
3. Facilitate efficient candidate shortlisting and stage progression for placement officers.
4. Guarantee data security and confidentiality through ASP.NET Core Identity authentication and role authorization.
5. Provide auditable analytics for management and accreditation bodies.

## 5. Project Scope
The scope of this project encompasses:
- University-wide student profile and verified academic record maintenance.
- Partner employer and recruitment drive management.
- Multi-round interview workflow tracking and candidate evaluation.
- Institutional placement statistics generation.
