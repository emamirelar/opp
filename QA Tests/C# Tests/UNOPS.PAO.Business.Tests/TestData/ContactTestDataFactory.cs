using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.Business.Tests.TestData;

/// <summary>
/// Factory for creating Contact test data.
/// Uses UNOPSContact (not base Contact) to match PostgreSQL TPH schema.
/// </summary>
public class ContactTestDataFactory
{
    private int _sequenceNumber = 1;

    public UNOPSContact CreateContact(Action<UNOPSContact>? customize = null)
    {
        var contact = new UNOPSContact
        {
            Name = $"Contact {_sequenceNumber}",
            FirstName = $"FirstName{_sequenceNumber}",
            LastName = $"LastName{_sequenceNumber}",
            Email = $"contact{_sequenceNumber}@example.com",
            Title = $"Title{_sequenceNumber}",
            Phone = $"+123456789{_sequenceNumber:D2}",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 1,
            LastModifiedBy = 1,
            LastModifiedDate = DateTime.UtcNow,
            IsDeleted = false
        };
        _sequenceNumber++;

        customize?.Invoke(contact);
        return contact;
    }

    public UNOPSContact CreateContactWithPartner(int partnerId, string partnerName = "Test Partner")
    {
        return CreateContact(c =>
        {
            c.PartnerId = partnerId;
        });
    }

    public UNOPSContact CreateDeletedContact()
    {
        return CreateContact(c =>
        {
            c.IsDeleted = true;
            c.DeletedDate = DateTime.UtcNow;
        });
    }

    public UNOPSContact CreateContactForDuplicateTesting(string firstName, string lastName, string email)
    {
        return CreateContact(c =>
        {
            c.FirstName = firstName;
            c.LastName = lastName;
            c.Email = email;
        });
    }
}
