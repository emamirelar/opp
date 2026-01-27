using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UNOPS.PAO.Models.Opportunity;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Controllers;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity.Controllers
{
    public class PartnershipAgreementControllerTests
    {
        private readonly Mock<IManagerWrapper> _mockManagerWrapper;
        private readonly PartnershipAgreementController _controller;

        public PartnershipAgreementControllerTests()
        {
            _mockManagerWrapper = new Mock<IManagerWrapper>();
            _controller = new PartnershipAgreementController(_mockManagerWrapper.Object);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-AGMT-CTRL-F-001")]
        public async Task UploadAgreement_ValidPDF_ReturnsCreated()
        {
            var upload = new AgreementUploadModel { FileName = "mou.pdf", PartnerId = 1 };
            var agreement = new AgreementModel { Id = 1, FileName = "mou.pdf" };
            _mockManagerWrapper.Setup(m => m.AgreementManager.UploadAgreementAsync(upload)).ReturnsAsync(agreement);

            var result = await _controller.UploadAgreement(upload);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        [Trait("TestId", "TC-OPP-AGMT-CTRL-F-002")]
        public async Task SearchAgreements_ByPartner_ReturnsFilteredList()
        {
            var partnerId = 1;
            var agreements = new List<AgreementModel> { new AgreementModel { Id = 1, PartnerId = partnerId } };
            _mockManagerWrapper.Setup(m => m.AgreementManager.SearchByPartnerAsync(partnerId)).ReturnsAsync(agreements);

            var result = await _controller.SearchByPartner(partnerId);

            Assert.IsType<OkObjectResult>(result);
        }

        public class AgreementUploadModel { public string FileName { get; set; } public int PartnerId { get; set; } }
        public class AgreementModel { public int Id { get; set; } public string FileName { get; set; } public int PartnerId { get; set; } }
    }
}
