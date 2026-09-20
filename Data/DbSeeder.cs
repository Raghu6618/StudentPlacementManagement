using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlacementManagement.Models;

namespace StudentPlacementManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Student", "Recruiter" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default Admin
            var adminEmail = configuration["ApplicationSettings:DefaultAdminEmail"] ?? "admin@placement.edu";
            var adminPassword = configuration["ApplicationSettings:DefaultAdminPassword"] ?? "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Placement Officer (Admin)",
                    UserRole = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { DepartmentName = "Computer Science & Engineering", DepartmentCode = "CSE", CreatedAt = DateTime.UtcNow },
                    new Department { DepartmentName = "Information Technology", DepartmentCode = "IT", CreatedAt = DateTime.UtcNow },
                    new Department { DepartmentName = "Electronics & Communication", DepartmentCode = "ECE", CreatedAt = DateTime.UtcNow },
                    new Department { DepartmentName = "Mechanical Engineering", DepartmentCode = "ME", CreatedAt = DateTime.UtcNow },
                    new Department { DepartmentName = "Civil Engineering", DepartmentCode = "CE", CreatedAt = DateTime.UtcNow },
                    new Department { DepartmentName = "Electrical Engineering", DepartmentCode = "EE", CreatedAt = DateTime.UtcNow }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // 4. Seed Companies
            if (!await context.Companies.AnyAsync())
            {
                var companies = new List<Company>
                {
                    new Company
                    {
                        CompanyName = "Tata Consultancy Services (TCS)",
                        Website = "https://www.tcs.com",
                        Industry = "Information Technology",
                        Description = "Global IT services, consulting and business solutions organization.",
                        Location = "Mumbai / Pan-India",
                        HRName = "Mr. Rajesh Gopinathan",
                        HREmail = "careers@tcs.com",
                        HRPhone = "+91 22 6778 9999",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Company
                    {
                        CompanyName = "Infosys Limited",
                        Website = "https://www.infosys.com",
                        Industry = "Software Development",
                        Description = "Global leader in next-generation digital services and consulting.",
                        Location = "Bengaluru / Pune",
                        HRName = "Ms. Sunita Sharma",
                        HREmail = "campus@infosys.com",
                        HRPhone = "+91 80 2852 0261",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Company
                    {
                        CompanyName = "Wipro Technologies",
                        Website = "https://www.wipro.com",
                        Industry = "Consulting & Cloud",
                        Description = "Leading technology services and consulting company focused on innovative solutions.",
                        Location = "Bengaluru / Hyderabad",
                        HRName = "Mr. Anand Murthy",
                        HREmail = "talent@wipro.com",
                        HRPhone = "+91 80 2844 0011",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Company
                    {
                        CompanyName = "Accenture India",
                        Website = "https://www.accenture.com",
                        Industry = "Management & Tech Consulting",
                        Description = "Global professional services company with leading capabilities in digital, cloud and security.",
                        Location = "Gurugram / Bengaluru",
                        HRName = "Ms. Deepa Mathew",
                        HREmail = "india.hiring@accenture.com",
                        HRPhone = "+91 124 456 7000",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Company
                    {
                        CompanyName = "Deloitte USI",
                        Website = "https://www.deloitte.com",
                        Industry = "Advisory & Analytics",
                        Description = "Audit, consulting, advisory, and tax services for premier Fortune 500 enterprises.",
                        Location = "Hyderabad / Mumbai",
                        HRName = "Mr. Vikram Rao",
                        HREmail = "usi.campus@deloitte.com",
                        HRPhone = "+91 40 7125 3000",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await context.Companies.AddRangeAsync(companies);
                await context.SaveChangesAsync();
            }

            // 5. Seed Placement Drives
            if (!await context.PlacementDrives.AnyAsync())
            {
                var tcs = await context.Companies.FirstAsync(c => c.CompanyName.Contains("TCS"));
                var infosys = await context.Companies.FirstAsync(c => c.CompanyName.Contains("Infosys"));
                var deloitte = await context.Companies.FirstAsync(c => c.CompanyName.Contains("Deloitte"));
                var accenture = await context.Companies.FirstAsync(c => c.CompanyName.Contains("Accenture"));

                var cseDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "CSE");
                var itDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "IT");
                var eceDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "ECE");
                var meDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "ME");

                var drive1 = new PlacementDrive
                {
                    CompanyId = tcs.CompanyId,
                    JobRole = "Graduate Engineer Trainee (Ninja)",
                    JobDescription = "Exciting opportunity for fresh engineering graduates to work on Java, .NET, cloud solutions, and enterprise engineering projects.",
                    JobLocation = "Pan-India",
                    EmploymentType = "Full Time",
                    SalaryPackage = 4.50m,
                    MinCGPA = 6.50m,
                    MaxBacklogs = 1,
                    PassingYear = DateTime.UtcNow.Year,
                    DriveDate = DateTime.UtcNow.AddDays(14),
                    ApplicationStartDate = DateTime.UtcNow.AddDays(-5),
                    ApplicationEndDate = DateTime.UtcNow.AddDays(10),
                    RequiredSkills = "C#, Java, Python, SQL, Problem Solving",
                    EligibilityDescription = "Min 6.5 CGPA with max 1 active backlog. Open to CSE, IT, ECE.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                var drive2 = new PlacementDrive
                {
                    CompanyId = infosys.CompanyId,
                    JobRole = "Specialist Programmer (Power Programmer)",
                    JobDescription = "High-impact coding role requiring proficiency in data structures, algorithms, system architecture, and modern full-stack development.",
                    JobLocation = "Bengaluru / Pune",
                    EmploymentType = "Full Time",
                    SalaryPackage = 9.50m,
                    MinCGPA = 7.50m,
                    MaxBacklogs = 0,
                    PassingYear = DateTime.UtcNow.Year,
                    DriveDate = DateTime.UtcNow.AddDays(20),
                    ApplicationStartDate = DateTime.UtcNow.AddDays(-2),
                    ApplicationEndDate = DateTime.UtcNow.AddDays(12),
                    RequiredSkills = "Data Structures, Algorithms, C++, Java, System Design",
                    EligibilityDescription = "Min 7.5 CGPA, strictly zero backlogs. Open to CSE & IT.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                var drive3 = new PlacementDrive
                {
                    CompanyId = deloitte.CompanyId,
                    JobRole = "Analyst - Cloud & Digital Platforms",
                    JobDescription = "Consulting and business technology architecture role involving cloud migrations, API strategies, and client digital transformation.",
                    JobLocation = "Hyderabad",
                    EmploymentType = "Full Time",
                    SalaryPackage = 8.20m,
                    MinCGPA = 7.00m,
                    MaxBacklogs = 0,
                    PassingYear = DateTime.UtcNow.Year,
                    DriveDate = DateTime.UtcNow.AddDays(25),
                    ApplicationStartDate = DateTime.UtcNow.AddDays(5), // Upcoming
                    ApplicationEndDate = DateTime.UtcNow.AddDays(18),
                    RequiredSkills = "Cloud Architecture, SQL, Business Analysis, Agile",
                    EligibilityDescription = "Min 7.0 CGPA, 0 backlogs. Open to all engineering departments.",
                    Status = "Upcoming",
                    CreatedAt = DateTime.UtcNow
                };

                var drive4 = new PlacementDrive
                {
                    CompanyId = accenture.CompanyId,
                    JobRole = "Associate Software Engineer (ASE)",
                    JobDescription = "Entry-level development and testing position across diverse industry technologies and platforms.",
                    JobLocation = "Gurugram / Noida",
                    EmploymentType = "Full Time",
                    SalaryPackage = 5.00m,
                    MinCGPA = 6.00m,
                    MaxBacklogs = 2,
                    PassingYear = DateTime.UtcNow.Year,
                    DriveDate = DateTime.UtcNow.AddDays(10),
                    ApplicationStartDate = DateTime.UtcNow.AddDays(-7),
                    ApplicationEndDate = DateTime.UtcNow.AddDays(6),
                    RequiredSkills = "Basic Programming, Analytical Thinking, Communication",
                    EligibilityDescription = "Min 6.0 CGPA, max 2 backlogs.",
                    Status = "Open",
                    CreatedAt = DateTime.UtcNow
                };

                await context.PlacementDrives.AddRangeAsync(drive1, drive2, drive3, drive4);
                await context.SaveChangesAsync();

                // Add Drive-Department mappings
                context.DriveDepartments.AddRange(
                    new DriveDepartment { DriveId = drive1.DriveId, DepartmentId = cseDept.DepartmentId },
                    new DriveDepartment { DriveId = drive1.DriveId, DepartmentId = itDept.DepartmentId },
                    new DriveDepartment { DriveId = drive1.DriveId, DepartmentId = eceDept.DepartmentId },

                    new DriveDepartment { DriveId = drive2.DriveId, DepartmentId = cseDept.DepartmentId },
                    new DriveDepartment { DriveId = drive2.DriveId, DepartmentId = itDept.DepartmentId },

                    new DriveDepartment { DriveId = drive3.DriveId, DepartmentId = cseDept.DepartmentId },
                    new DriveDepartment { DriveId = drive3.DriveId, DepartmentId = itDept.DepartmentId },
                    new DriveDepartment { DriveId = drive3.DriveId, DepartmentId = eceDept.DepartmentId },
                    new DriveDepartment { DriveId = drive3.DriveId, DepartmentId = meDept.DepartmentId },

                    new DriveDepartment { DriveId = drive4.DriveId, DepartmentId = cseDept.DepartmentId },
                    new DriveDepartment { DriveId = drive4.DriveId, DepartmentId = itDept.DepartmentId },
                    new DriveDepartment { DriveId = drive4.DriveId, DepartmentId = eceDept.DepartmentId }
                );

                // Add Interview Rounds for Drive 1 (TCS)
                var round1 = new InterviewRound
                {
                    DriveId = drive1.DriveId,
                    RoundName = "Online Aptitude & Coding Assessment",
                    RoundNumber = 1,
                    RoundDate = DateTime.UtcNow.AddDays(14),
                    Description = "90 minutes test covering Quantitative, Logical Reasoning, Verbal and 2 Coding Questions.",
                    IsRequired = true,
                    CreatedAt = DateTime.UtcNow
                };
                var round2 = new InterviewRound
                {
                    DriveId = drive1.DriveId,
                    RoundName = "Technical Interview",
                    RoundNumber = 2,
                    RoundDate = DateTime.UtcNow.AddDays(16),
                    Description = "In-depth technical interview covering Core Subjects, Projects, and Live Coding.",
                    IsRequired = true,
                    CreatedAt = DateTime.UtcNow
                };
                var round3 = new InterviewRound
                {
                    DriveId = drive1.DriveId,
                    RoundName = "Managerial & HR Round",
                    RoundNumber = 3,
                    RoundDate = DateTime.UtcNow.AddDays(18),
                    Description = "Behavioral evaluation, career aspirations, relocation discussion.",
                    IsRequired = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.InterviewRounds.AddRange(round1, round2, round3);
                await context.SaveChangesAsync();
            }

            // 6. Seed Sample Students & Ensure Raghu Joshi is correctly updated
            // Check if existing database has previous Rahul Sharma record and update to Raghu Joshi
            var existingRahulUser = await userManager.FindByEmailAsync("rahul.sharma@student.edu");
            if (existingRahulUser != null)
            {
                existingRahulUser.Email = "raghu.joshi@student.edu";
                existingRahulUser.UserName = "raghu.joshi@student.edu";
                existingRahulUser.NormalizedEmail = "RAGHU.JOSHI@STUDENT.EDU";
                existingRahulUser.NormalizedUserName = "RAGHU.JOSHI@STUDENT.EDU";
                existingRahulUser.FullName = "Raghu Joshi";
                await userManager.UpdateAsync(existingRahulUser);
            }

            var existingRahulStudent = await context.Students.FirstOrDefaultAsync(s => s.Email == "rahul.sharma@student.edu" || s.FirstName == "Rahul");
            if (existingRahulStudent != null)
            {
                existingRahulStudent.FirstName = "Raghu";
                existingRahulStudent.LastName = "Joshi";
                existingRahulStudent.Email = "raghu.joshi@student.edu";
                existingRahulStudent.ResumeFileName = "Raghu_Resume.pdf";
                await context.SaveChangesAsync();
            }

            if (!await context.Students.AnyAsync())
            {
                var cseDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "CSE");
                var itDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "IT");
                var eceDept = await context.Departments.FirstAsync(d => d.DepartmentCode == "ECE");

                var sampleStudentData = new[]
                {
                    new { Email = "raghu.joshi@student.edu", First = "Raghu", Last = "Joshi", Dept = cseDept.DepartmentId, Enroll = "2022CSE0101", Cgpa = 8.65m, Backlogs = 0, Gender = "Male", Phone = "+91 9876543210" },
                    new { Email = "priya.patel@student.edu", First = "Priya", Last = "Patel", Dept = itDept.DepartmentId, Enroll = "2022IT0205", Cgpa = 7.80m, Backlogs = 0, Gender = "Female", Phone = "+91 9876543211" },
                    new { Email = "amit.verma@student.edu", First = "Amit", Last = "Verma", Dept = cseDept.DepartmentId, Enroll = "2022CSE0112", Cgpa = 6.85m, Backlogs = 1, Gender = "Male", Phone = "+91 9876543212" },
                    new { Email = "sneha.kulkarni@student.edu", First = "Sneha", Last = "Kulkarni", Dept = eceDept.DepartmentId, Enroll = "2022ECE0308", Cgpa = 8.10m, Backlogs = 0, Gender = "Female", Phone = "+91 9876543213" },
                    new { Email = "vikram.singh@student.edu", First = "Vikram", Last = "Singh", Dept = cseDept.DepartmentId, Enroll = "2022CSE0145", Cgpa = 5.90m, Backlogs = 3, Gender = "Male", Phone = "+91 9876543214" }
                };

                foreach (var item in sampleStudentData)
                {
                    var user = await userManager.FindByEmailAsync(item.Email);
                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = item.Email,
                            Email = item.Email,
                            EmailConfirmed = true,
                            FullName = $"{item.First} {item.Last}",
                            UserRole = "Student",
                            CreatedAt = DateTime.UtcNow
                        };

                        var result = await userManager.CreateAsync(user, "Student@123");
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, "Student");

                            var student = new Student
                            {
                                UserId = user.Id,
                                EnrollmentNo = item.Enroll,
                                FirstName = item.First,
                                LastName = item.Last,
                                Gender = item.Gender,
                                DateOfBirth = new DateTime(2003, 5, 15),
                                Phone = item.Phone,
                                Email = item.Email,
                                Address = "Campus Hostel Block B",
                                City = "Pune",
                                State = "Maharashtra",
                                DepartmentId = item.Dept,
                                PassingYear = DateTime.UtcNow.Year,
                                CGPA = item.Cgpa,
                                Backlogs = item.Backlogs,
                                ResumeFileName = $"{item.First}_Resume.pdf",
                                ResumePath = "/uploads/resumes/sample-resume.pdf",
                                PlacementStatus = "NotPlaced",
                                CreatedAt = DateTime.UtcNow
                            };

                            context.Students.Add(student);
                        }
                    }
                }

                await context.SaveChangesAsync();

                // 7. Seed Sample Applications & Placements
                var raghu = await context.Students.FirstOrDefaultAsync(s => s.Email == "raghu.joshi@student.edu");
                var priya = await context.Students.FirstOrDefaultAsync(s => s.Email == "priya.patel@student.edu");
                var amit = await context.Students.FirstOrDefaultAsync(s => s.Email == "amit.verma@student.edu");
                var tcsDrive = await context.PlacementDrives.FirstOrDefaultAsync(d => d.JobRole.Contains("Ninja"));
                var infosysDrive = await context.PlacementDrives.FirstOrDefaultAsync(d => d.JobRole.Contains("Specialist"));

                if (raghu != null && tcsDrive != null && infosysDrive != null)
                {
                    // Raghu applies to TCS and is Selected (Placed)
                    var raghuApp = new Application
                    {
                        DriveId = tcsDrive.DriveId,
                        StudentId = raghu.StudentId,
                        AppliedDate = DateTime.UtcNow.AddDays(-4),
                        Status = "Selected",
                        Remarks = "Cleared all rounds with stellar performance in technical interviews."
                    };
                    context.Applications.Add(raghuApp);

                    raghu.PlacementStatus = "Placed";

                    var placement = new Placement
                    {
                        StudentId = raghu.StudentId,
                        DriveId = tcsDrive.DriveId,
                        CompanyId = tcsDrive.CompanyId,
                        JobRole = tcsDrive.JobRole,
                        SalaryPackage = tcsDrive.SalaryPackage,
                        PlacementDate = DateTime.UtcNow.AddDays(-1),
                        JoiningDate = DateTime.UtcNow.AddMonths(4),
                        Status = "Selected",
                        Remarks = "Direct campus placement offer."
                    };
                    context.Placements.Add(placement);

                    // Raghu also applied to Infosys and is Shortlisted
                    context.Applications.Add(new Application
                    {
                        DriveId = infosysDrive.DriveId,
                        StudentId = raghu.StudentId,
                        AppliedDate = DateTime.UtcNow.AddDays(-1),
                        Status = "Shortlisted",
                        Remarks = "Coding round cleared."
                    });
                }

                if (priya != null && tcsDrive != null)
                {
                    context.Applications.Add(new Application
                    {
                        DriveId = tcsDrive.DriveId,
                        StudentId = priya.StudentId,
                        AppliedDate = DateTime.UtcNow.AddDays(-3),
                        Status = "Shortlisted",
                        Remarks = "Shortlisted for Round 1."
                    });
                }

                if (amit != null && tcsDrive != null)
                {
                    context.Applications.Add(new Application
                    {
                        DriveId = tcsDrive.DriveId,
                        StudentId = amit.StudentId,
                        AppliedDate = DateTime.UtcNow.AddDays(-2),
                        Status = "Applied",
                        Remarks = "Application submitted."
                    });
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
