using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.TestData;

/// <summary>
/// Factory for creating Contact test data
/// </summary>
public class ContactTestDataFactory
{
    private int _sequenceNumber = 1;

    public Contact CreateContact(Action<Contact>? customize = null)
    {
        var contact = new Contact
        {
            Id = _sequenceNumber++,
            Name = $"Contact {_sequenceNumber}",
            FirstName = $"FirstName{_sequenceNumber}",
            LastName = $"LastName{_sequenceNumber}",
            Email = $"contact{_sequenceNumber}@example.com",
            Title = $"Title{_sequenceNumber}",
            Phone = $"+123456789{_sequenceNumber:D2}",
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        customize?.Invoke(contact);
        return contact;
    }

    public Contact CreateContactWithPartner(int partnerId, string partnerName = "Test Partner")
    {
        return CreateContact(c =>
        {
            c.PartnerId = partnerId;
            c.Partner = new Partner { Id = partnerId, Name = partnerName };
        });
    }

    public Contact CreateDeletedContact()
    {
        return CreateContact(c =>
        {
            c.IsDeleted = true;
            c.DeletedDate = DateTime.UtcNow;
        });
    }

    public Contact CreateContactForDuplicateTesting(string firstName, string lastName, string email)
    {
        return CreateContact(c =>
        {
            c.FirstName = firstName;
            c.LastName = lastName;
            c.Email = email;
        });
    }
}
