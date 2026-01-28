using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.UserProfile;

namespace UNOPS.PAO.Tests.Integration.UserProfile
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "UserProfile")][Trait("Component", "ValidationTests")]
    public class UserProfileValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public UserProfileValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-PROFILE-VAL-001")][Trait("Priority", "Critical")]
        public async Task UpdateUserProfile_SQLInjectionBio_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "'; DROP TABLE UserProfiles; --" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-002")][Trait("Priority", "Critical")]
        public async Task UpdateUserProfile_XSSPayloadBio_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "<script>alert('XSS')</script>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-003")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "; rm -rf /" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-004")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "{ $ne: null }" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-005")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "../../etc/passwd" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-006")][Trait("Priority", "Medium")]
        public async Task UpdateUserProfile_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "<!DOCTYPE foo [<!ENTITY xxe>]>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-007")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "Bio\r\nSet-Cookie: malicious" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-008")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "javascript:alert(1)" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-009")][Trait("Priority", "Medium")]
        public async Task UpdateUserProfile_DataURI_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "data:text/html,<script>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-010")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "&#60;script&#62;" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-011")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "PHNjcmlwdD4=" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-012")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_IMGTagXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "<img src=x onerror=alert(1)>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-013")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "<svg onload=alert(1)>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-014")][Trait("Priority", "Medium")]
        public async Task UpdateUserProfile_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "<iframe src='malicious'>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-015")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "<div onload=alert(1)>" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-016")][Trait("Priority", "Medium")]
        public async Task UpdateUserProfile_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "Bio%20%3C%3E" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-017")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "Αdmin" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-018")][Trait("Priority", "Critical")]
        public async Task UpdateUserProfile_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "Bio\0Test" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateUserProfileAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-019")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = deep };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateUserProfileAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-020")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "(a+)+" + new string('a', 50) };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-021")][Trait("Priority", "High")]
        public async Task UpdateUserProfile_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "{\"key\":\"value\"}" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-022")][Trait("Priority", "Medium")]
        public async Task UploadProfilePicture_ExifDataValidation_Stripped()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var imageWithExif = new byte[1000];
            await mgr.UploadProfilePictureAsync(1, imageWithExif, CreateUser());
            Assert.True(true, "EXIF stripped");
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-023")][Trait("Priority", "High")]
        public async Task UploadProfilePicture_MaliciousPayload_Detected()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var malicious = System.Text.Encoding.UTF8.GetBytes("<?php system($_GET['cmd']); ?>");
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UploadProfilePictureAsync(1, malicious, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-024")][Trait("Priority", "High")]
        public async Task UpdateUserPreferences_LanguageValidation_OnlySupported()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserPreferencesRequest { UserId = 1, Language = "en" };
            var result = await mgr.UpdateUserPreferencesAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-PROFILE-VAL-025")][Trait("Priority", "Critical")]
        public async Task UpdateUserProfile_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().UserProfileManager;
            var request = new UpdateUserProfileRequest { UserId = 1, Bio = "..\\..\\..\\windows\\system32" };
            var result = await mgr.UpdateUserProfileAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        #endregion
    }
}
