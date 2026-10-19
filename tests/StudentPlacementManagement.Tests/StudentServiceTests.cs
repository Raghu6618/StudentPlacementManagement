using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentPlacementManagement.Data;
using StudentPlacementManagement.Models;
using StudentPlacementManagement.Services;
using Xunit;

namespace StudentPlacementManagement.Tests
{
    public class StudentServiceTests
    {
        private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public void CalculateProfileCompletion_CompleteProfile_Returns100Percent()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_CompleteProfile");
            var service = new StudentService(context, null!, null!);

            var student = new Student
            {
                FirstName = "Raghu",
                LastName = "Joshi",
                Email = "raghu.joshi@student.edu",
                Phone = "+91 9876543210",
                Gender = "Male",
                DateOfBirth = new DateTime(2002, 5, 15),
                City = "Pune",
                DepartmentId = 1,
                CGPA = 8.5m,
                ResumePath = "/uploads/resumes/sample.pdf"
            };

            // Act
            int completion = service.CalculateProfileCompletion(student);

            // Assert
            Assert.Equal(100, completion);
        }

        [Fact]
        public void CalculateProfileCompletion_MissingResumeAndCity_Returns80Percent()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_PartialProfile");
            var service = new StudentService(context, null!, null!);

            var student = new Student
            {
                FirstName = "Raghu",
                LastName = "Joshi",
                Email = "raghu.joshi@student.edu",
                Phone = "+91 9876543210",
                Gender = "Male",
                DateOfBirth = new DateTime(2002, 5, 15),
                City = null,
                DepartmentId = 1,
                CGPA = 8.5m,
                ResumePath = null // Missing resume
            };

            // Act
            int completion = service.CalculateProfileCompletion(student);

            // Assert (8 of 10 earned points = 80%)
            Assert.Equal(80, completion);
        }

        [Fact]
        public void CalculateProfileCompletion_NullStudent_ReturnsZero()
        {
            // Arrange
            using var context = CreateInMemoryDbContext("TestDb_NullStudent");
            var service = new StudentService(context, null!, null!);

            // Act
            int completion = service.CalculateProfileCompletion(null!);

            // Assert
            Assert.Equal(0, completion);
        }
    }
}
