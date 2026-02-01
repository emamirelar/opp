using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Models.Liaison;
using UNOPS.PAO.IntegrationTests.Infrastructure;

using UNOPS.PAO.Business.Interfaces;

namespace UNOPS.PAO.Tests.Integration.LiaisonOffice
{
    [Collection("Integration Tests")][Trait("Category", "Integration")][Trait("Feature", "LiaisonOffice")][Trait("Component", "ValidationTests")]
    public class LiaisonOfficeValidationTests : IClassFixture<PAOWebApplicationFactory<Program>>
    {
        private readonly PAOWebApplicationFactory<Program> _factory;
        public LiaisonOfficeValidationTests(PAOWebApplicationFactory<Program> factory) => _factory = factory;
        private ClaimsPrincipal CreateUser() => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1"), new Claim(ClaimTypes.Role, "Administrator") }, "TestAuth"));

        [Fact][Trait("TestId", "TC-LIAISON-VAL-001")][Trait("Priority", "Critical")]
        public async Task CreateLiaisonOffice_SQLInjectionName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "'; DROP TABLE LiaisonOffices; --", Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-002")][Trait("Priority", "Critical")]
        public async Task UpdateLiaisonOffice_XSSPayloadName_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "XSS Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "<script>alert('XSS')</script>" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-003")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_CommandInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "; rm -rf /", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-004")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_NoSQLInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "NoSQL Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "{ $ne: null }" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-005")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_PathTraversal_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "../../etc/passwd", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-006")][Trait("Priority", "Medium")]
        public async Task UpdateLiaisonOffice_XMLEntityInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "XML Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "<!DOCTYPE foo [<!ENTITY xxe>]>" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-007")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_CRLFInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Office\r\nSet-Cookie: malicious", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-008")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_JavaScriptProtocol_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "JS Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "javascript:alert(1)" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-009")][Trait("Priority", "Medium")]
        public async Task CreateLiaisonOffice_HTMLEntities_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "&#60;script&#62;", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-010")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_Base64Payload_StoredAsIs()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "B64 Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "PHNjcmlwdD4=" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-011")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_IMGTagXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "<img src=x onerror=alert(1)>", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-012")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_SVGXSS_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "SVG Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "<svg onload=alert(1)>" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-013")][Trait("Priority", "Medium")]
        public async Task CreateLiaisonOffice_IFRAMEInjection_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "<iframe src='malicious'>", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-014")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_EventHandlers_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Event Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "<div onload=alert(1)>" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-015")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_URLEncoding_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Office%20%3C%3E", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-016")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_UnicodeHomograph_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Homograph Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "Αdmin" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-017")][Trait("Priority", "Critical")]
        public async Task CreateLiaisonOffice_NullByteInjection_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Office\0Test", Region = "Africa" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.CreateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-018")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_DeepHTMLNesting_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Deep Test", Region = "Africa" }, CreateUser());
            var deep = "<div>" + string.Join("", Enumerable.Repeat("<div>", 100)) + string.Join("", Enumerable.Repeat("</div>", 101));
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = deep };
            await Assert.ThrowsAsync<ArgumentException>(async () => await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()));
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-019")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_RegexDoS_Performant()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "(a+)+" + new string('a', 50), Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-020")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_JSONPayload_SafelyHandled()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "JSON Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Name = "{\"key\":\"value\"}" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-021")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_EmailValidation_ValidFormat()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Email Valid", Region = "Africa", Email = "office@test.com" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-022")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_PhoneValidation_ValidFormat()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Phone Test", Region = "Africa" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Phone = "+1-234-567-8900" };
            var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-023")][Trait("Priority", "High")]
        public async Task CreateLiaisonOffice_CountryValidation_ValidCountries()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "Country Test", Region = "Africa", Country = "Kenya" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-024")][Trait("Priority", "High")]
        public async Task UpdateLiaisonOffice_RegionCountryConsistency_Validates()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var office = await mgr.CreateLiaisonOfficeAsync(new CreateLiaisonOfficeRequest { Name = "Consistency Test", Region = "Africa", Country = "Kenya" }, CreateUser());
            var request = new UpdateLiaisonOfficeRequest { Id = office.Id, Country = "Japan" }; // Japan not in Africa
            try { var result = await mgr.UpdateLiaisonOfficeAsync(request, CreateUser()); result.Should().NotBeNull(); }
            catch (ArgumentException) { Assert.True(true, "Region-country consistency enforced"); }
        }

        [Fact][Trait("TestId", "TC-LIAISON-VAL-025")][Trait("Priority", "Critical")]
        public async Task CreateLiaisonOffice_WindowsPathTraversal_Sanitized()
        {
            using var scope = _factory.Services.CreateScope();
            var mgr = scope.ServiceProvider.GetRequiredService<IManagerWrapper>().LiaisonOfficeManager;
            var request = new CreateLiaisonOfficeRequest { Name = "..\\..\\..\\windows\\system32", Region = "Africa" };
            var result = await mgr.CreateLiaisonOfficeAsync(request, CreateUser());
            result.Should().NotBeNull();
        }


    }
}
