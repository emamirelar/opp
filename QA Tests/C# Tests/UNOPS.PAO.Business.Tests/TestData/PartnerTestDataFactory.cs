using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.TestData;

/// <summary>
/// Factory for creating Partner test data
/// </summary>
public class PartnerTestDataFactory
{
    private int _sequenceNumber = 1;

    public Partner CreatePartner(Action<Partner>? customize = null)
    {
        var partner = new Partner
        {
            Id = _sequenceNumber++,
            Name = $"Test Partner {_sequenceNumber}",
            PartnerShortDescription = $"Short description {_sequenceNumber}",
            Status = EntityStatus.Active,
            PartnerApprovalStatus = PartnerApprovalStatus.NotApproved,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        customize?.Invoke(partner);
        return partner;
    }

    public Partner CreateApprovedPartner(int? erpDimValue = null)
    {
        return CreatePartner(p =>
        {
            p.PartnerApprovalStatus = PartnerApprovalStatus.Approved;
            p.PartnerApprovalDate = DateTime.UtcNow;
            p.ErpDimValue = erpDimValue ?? (1000 + _sequenceNumber);
            p.CanCreateNewOpportunities = true;
        });
    }

    public List<Partner> CreatePartnersWithErpDimValues(params int[] erpDimValues)
    {
        return erpDimValues.Select(value => CreateApprovedPartner(value)).ToList();
    }

    public Partner CreatePartnerInReservedRange()
    {
        return CreateApprovedPartner(erpDimValue: 8500); // In 8000-9999 range
    }

    public Partner CreateDeletedPartner()
    {
        return CreatePartner(p =>
        {
            p.IsDeleted = true;
            p.DeletedDate = DateTime.UtcNow;
        });
    }
}
