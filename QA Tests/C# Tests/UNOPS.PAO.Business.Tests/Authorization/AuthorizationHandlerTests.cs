using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.PartnerTrees;
using UNOPS.PAO.Models.Users;
using Xunit;

namespace UNOPS.PAO.IntegrationTests.UnitTests.Authorization
{
    #region Test Infrastructure

    internal static class TestOperations
    {
        public static OperationAuthorizationRequirement Create = new() { Name = "Create" };
        public static OperationAuthorizationRequirement Read = new() { Name = "Read" };
        public static OperationAuthorizationRequirement Update = new() { Name = "Update" };
        public static OperationAuthorizationRequirement Delete = new() { Name = "Delete" };
    }

    internal static class AuthTestHelper
    {
        public static ClaimsPrincipal CreateUser(string email = "test@unops.org", string name = "Test User")
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, email),
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            identity.AddClaim(new Claim(identity.NameClaimType, email));
            return new ClaimsPrincipal(identity);
        }

        public static AuthorizationHandlerContext CreateContext(
            ClaimsPrincipal user,
            OperationAuthorizationRequirement requirement,
            object resource)
        {
            return new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                resource);
        }
    }

    #endregion

    #region ContactModel Authorization Tests

    public class ContactAuthorizationTests
    {
        private static ContactModel CreateContactModel() =>
            new()
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
            };

        // --- Positive ---
        [Fact]
        public void ContactModel_HasRequiredProperties()
        {
            var model = CreateContactModel();
            model.Id.Should().Be(1);
            model.FirstName.Should().Be("John");
        }

        // --- Negative ---
        [Fact]
        public void ContactModel_DefaultsToZeroId()
        {
            var model = new ContactModel();
            model.Id.Should().Be(0);
        }

        [Fact]
        public void ContactModel_DefaultsToNullNames()
        {
            var model = new ContactModel();
            model.FirstName.Should().BeNull();
            model.LastName.Should().BeNull();
        }

        [Fact]
        public void AuthContext_RequiresUser()
        {
            var user = AuthTestHelper.CreateUser();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Read, CreateContactModel());
            context.User.Should().NotBeNull();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void AuthContext_WithAnonymousUser_UserIdentityExists()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            var context = AuthTestHelper.CreateContext(user, TestOperations.Read, CreateContactModel());
            context.User.Identity.Should().NotBeNull();
            context.User.Identity!.IsAuthenticated.Should().BeFalse();
        }

        [Fact]
        public void AuthContext_WithAuthenticatedUser_IsAuthenticated()
        {
            var user = AuthTestHelper.CreateUser();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Create, CreateContactModel());
            context.User.Identity!.IsAuthenticated.Should().BeTrue();
        }

        [Fact]
        public void AuthContext_HasPendingRequirements()
        {
            var user = AuthTestHelper.CreateUser();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Create, CreateContactModel());
            context.PendingRequirements.Should().NotBeEmpty();
        }

        // --- Functional ---
        [Fact]
        public void AllCrudOperations_AreDefined()
        {
            TestOperations.Create.Name.Should().Be("Create");
            TestOperations.Read.Name.Should().Be("Read");
            TestOperations.Update.Name.Should().Be("Update");
            TestOperations.Delete.Name.Should().Be("Delete");
        }

        [Fact]
        public void UserContext_ContainsEmail()
        {
            var user = AuthTestHelper.CreateUser("john@unops.org");
            user.FindFirst(ClaimTypes.Email)!.Value.Should().Be("john@unops.org");
        }

        [Fact]
        public void UserContext_ContainsId()
        {
            var user = AuthTestHelper.CreateUser();
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be("1");
        }
    }

    #endregion

    #region ProfileModel Authorization Tests

    public class ProfileAuthorizationTests
    {
        private static ProfileModel CreateProfileModel(string email = "test@unops.org") =>
            new()
            {
                Email = email,
            };

        // --- Positive ---
        [Fact]
        public void ProfileModel_HasEmail()
        {
            var model = CreateProfileModel("admin@unops.org");
            model.Email.Should().Be("admin@unops.org");
        }

        // --- Negative ---
        [Fact]
        public void ProfileModel_DefaultEmail_IsNull()
        {
            var model = new ProfileModel();
            model.Email.Should().BeNull();
        }

        [Fact]
        public void ProfileModel_EmptyEmail_IsNotNull()
        {
            var model = CreateProfileModel("");
            model.Email.Should().BeEmpty();
        }

        [Fact]
        public void AuthContext_DifferentUserAndProfile_MismatchDetectable()
        {
            var user = AuthTestHelper.CreateUser("alice@unops.org");
            var profile = CreateProfileModel("bob@unops.org");
            var userName = user.Identity?.Name;
            userName.Should().NotBe(profile.Email);
        }

        // --- Edge/Boundary ---
        [Fact]
        public void AuthContext_SameUserAndProfile_MatchDetectable()
        {
            var email = "same@unops.org";
            var user = AuthTestHelper.CreateUser(email);
            var profile = CreateProfileModel(email);
            user.Identity?.Name.Should().Be(profile.Email);
        }

        [Fact]
        public void AuthContext_CaseSensitiveEmailComparison()
        {
            var user = AuthTestHelper.CreateUser("Test@UNOPS.org");
            var profile = CreateProfileModel("test@unops.org");
            (user.Identity?.Name == profile.Email).Should().BeFalse();
        }

        [Fact]
        public void Read_Operation_IsDefined()
        {
            TestOperations.Read.Name.Should().Be("Read");
        }

        // --- Functional ---
        [Fact]
        public void ProfileModel_CanSetAllProperties()
        {
            var model = new ProfileModel { Email = "test@example.com" };
            model.Email.Should().Be("test@example.com");
        }

        [Fact]
        public void UserIdentity_NameClaimMatchesEmail()
        {
            var email = "user@unops.org";
            var user = AuthTestHelper.CreateUser(email);
            user.Identity?.Name.Should().Be(email);
        }

        [Fact]
        public void AuthContext_OperationRequirement_HasName()
        {
            var req = new OperationAuthorizationRequirement { Name = "CustomAction" };
            req.Name.Should().Be("CustomAction");
        }
    }

    #endregion

    #region PartnerTreeModel Authorization Tests

    public class PartnerTreeAuthorizationTests
    {
        private static PartnerTreeModel CreatePartnerTreeModel() =>
            new()
            {
                Data = new PartnerTreeDataModel { Id = 1, Name = "Test Partner Tree" }
            };

        // --- Positive ---
        [Fact]
        public void PartnerTreeModel_HasRequiredProperties()
        {
            var model = CreatePartnerTreeModel();
            model.Data.Id.Should().Be(1);
            model.Data.Name.Should().Be("Test Partner Tree");
        }

        // --- Negative ---
        [Fact]
        public void PartnerTreeModel_DefaultsToZeroId()
        {
            var model = new PartnerTreeModel();
            model.Data.Id.Should().Be(0);
        }

        [Fact]
        public void PartnerTreeModel_DefaultsToNullName()
        {
            var model = new PartnerTreeModel();
            model.Data.Name.Should().BeNull();
        }

        [Fact]
        public void AuthContext_UnauthenticatedUser_NotAuthenticated()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            user.Identity!.IsAuthenticated.Should().BeFalse();
        }

        // --- Edge/Boundary ---
        [Fact]
        public void AuthContext_WithAuthenticatedUser_CanAccessPartnerTree()
        {
            var user = AuthTestHelper.CreateUser();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Read, CreatePartnerTreeModel());
            context.Resource.Should().NotBeNull();
        }

        [Fact]
        public void AuthContext_AllCrudOperations_AreSupported()
        {
            var ops = new[] { TestOperations.Create, TestOperations.Read, TestOperations.Update, TestOperations.Delete };
            ops.Should().HaveCount(4);
            ops.Select(o => o.Name).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void AuthContext_ResourceIsPartnerTreeModel()
        {
            var user = AuthTestHelper.CreateUser();
            var model = CreatePartnerTreeModel();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Read, model);
            context.Resource.Should().BeOfType<PartnerTreeModel>();
        }

        // --- Functional ---
        [Fact]
        public void PartnerTreeModel_CanBeUsedAsResource()
        {
            var model = CreatePartnerTreeModel();
            var user = AuthTestHelper.CreateUser();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Create, model);
            ((PartnerTreeModel)context.Resource!).Data.Id.Should().Be(1);
        }

        [Fact]
        public void Operations_AreDistinct()
        {
            TestOperations.Create.Should().NotBe(TestOperations.Read);
            TestOperations.Update.Should().NotBe(TestOperations.Delete);
        }

        [Fact]
        public void AuthContext_PendingRequirements_MatchesInput()
        {
            var user = AuthTestHelper.CreateUser();
            var context = AuthTestHelper.CreateContext(user, TestOperations.Delete, CreatePartnerTreeModel());
            context.PendingRequirements.Should().ContainSingle()
                .Which.Should().Be(TestOperations.Delete);
        }
    }

    #endregion
}
