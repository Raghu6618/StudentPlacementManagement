using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services;
using Xunit;

namespace StudentPlacementManagement.Tests
{
    public class EligibilityServiceTests
    {
        private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void EvaluateEligibility_StudentMeetsAllCriteria_ReturnsEligible()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_Eligible");
            var service = new EligibilityService(context);

            var student = new Student
            {
                StudentId = 1,
                FirstName = "Test",
                LastName = "User",
                DepartmentId = 10,
                CGPA = 8.5m,
                Backlogs = 0,
                PlacementStatus = "Not Placed"
            };

            var drive = new PlacementDrive
            {
                DriveId = 1,
                Status = "Open",
                ApplicationStartDate = DateTime.UtcNow.AddDays(-2),
                ApplicationEndDate = DateTime.UtcNow.AddDays(5),
                MinCGPA = 7.0m,
                MaxBacklogs = 1,
                SalaryPackage = 6.5m,
                DriveDepartments = new List<DriveDepartment>
                {
                    new DriveDepartment { DriveId = 1, DepartmentId = 10 }
                }
            };

            // Act
            var result = service.EvaluateEligibility(student, drive, alreadyApplied: false);

            // Assert
            Assert.True(result.IsEligible);
            Assert.Contains("criteria", result.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void EvaluateEligibility_StudentCGPABelowMinimum_ReturnsNotEligible()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_LowCGPA");
            var service = new EligibilityService(context);

            var student = new Student
            {
                StudentId = 2,
                DepartmentId = 10,
                CGPA = 6.2m,
                Backlogs = 0,
                PlacementStatus = "Not Placed"
            };

            var drive = new PlacementDrive
            {
                DriveId = 2,
                Status = "Open",
                ApplicationStartDate = DateTime.UtcNow.AddDays(-1),
                ApplicationEndDate = DateTime.UtcNow.AddDays(3),
                MinCGPA = 7.5m,
                MaxBacklogs = 0,
                DriveDepartments = new List<DriveDepartment>
                {
                    new DriveDepartment { DriveId = 2, DepartmentId = 10 }
                }
            };

            // Act
            var result = service.EvaluateEligibility(student, drive, alreadyApplied: false);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains("CGPA", result.Reason);
        }

        [Fact]
        public void EvaluateEligibility_StudentExceedsBacklogs_ReturnsNotEligible()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_Backlogs");
            var service = new EligibilityService(context);

            var student = new Student
            {
                StudentId = 3,
                DepartmentId = 10,
                CGPA = 8.0m,
                Backlogs = 2,
                PlacementStatus = "Not Placed"
            };

            var drive = new PlacementDrive
            {
                DriveId = 3,
                Status = "Open",
                ApplicationStartDate = DateTime.UtcNow.AddDays(-1),
                ApplicationEndDate = DateTime.UtcNow.AddDays(3),
                MinCGPA = 7.0m,
                MaxBacklogs = 1,
                DriveDepartments = new List<DriveDepartment>
                {
                    new DriveDepartment { DriveId = 3, DepartmentId = 10 }
                }
            };

            // Act
            var result = service.EvaluateEligibility(student, drive, alreadyApplied: false);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains("backlog", result.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void EvaluateEligibility_DepartmentNotEligible_ReturnsNotEligible()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_WrongDept");
            var service = new EligibilityService(context);

            var student = new Student
            {
                StudentId = 4,
                DepartmentId = 99, // Mechanical
                CGPA = 9.0m,
                Backlogs = 0,
                PlacementStatus = "Not Placed"
            };

            var drive = new PlacementDrive
            {
                DriveId = 4,
                Status = "Open",
                ApplicationStartDate = DateTime.UtcNow.AddDays(-1),
                ApplicationEndDate = DateTime.UtcNow.AddDays(3),
                MinCGPA = 6.0m,
                MaxBacklogs = 2,
                DriveDepartments = new List<DriveDepartment>
                {
                    new DriveDepartment { DriveId = 4, DepartmentId = 10 } // CSE only
                }
            };

            // Act
            var result = service.EvaluateEligibility(student, drive, alreadyApplied: false);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains("department", result.Reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void EvaluateEligibility_AlreadyApplied_ReturnsNotEligible()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_AlreadyApplied");
            var service = new EligibilityService(context);

            var student = new Student
            {
                StudentId = 5,
                DepartmentId = 10,
                CGPA = 8.5m,
                Backlogs = 0,
                PlacementStatus = "Not Placed"
            };

            var drive = new PlacementDrive
            {
                DriveId = 5,
                Status = "Open",
                ApplicationStartDate = DateTime.UtcNow.AddDays(-1),
                ApplicationEndDate = DateTime.UtcNow.AddDays(3),
                MinCGPA = 6.0m,
                MaxBacklogs = 1,
                DriveDepartments = new List<DriveDepartment>
                {
                    new DriveDepartment { DriveId = 5, DepartmentId = 10 }
                }
            };

            // Act
            var result = service.EvaluateEligibility(student, drive, alreadyApplied: true);

            // Assert
            Assert.False(result.IsEligible);
            Assert.Contains("already applied", result.Reason, StringComparison.OrdinalIgnoreCase);
        }
    }
}
