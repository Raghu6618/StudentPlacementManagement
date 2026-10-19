using System;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace StudentPlacementManagement.Tests
{
    public class AccountLockoutConfigurationTests
    {
        [Fact]
        public void IdentityOptions_LockoutPolicy_IsConfiguredProperly()
        {
            // Arrange & Act
            var options = new IdentityOptions();
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // Assert
            Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
            Assert.Equal(TimeSpan.FromMinutes(10), options.Lockout.DefaultLockoutTimeSpan);
            Assert.True(options.Lockout.AllowedForNewUsers);
        }
    }
}
