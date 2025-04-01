namespace UNOPS.PAO.Business.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;

public interface IContactManager
{
    Task<ContactModel> CreateContactAsync(ContactRequest model);

    IEnumerable<ContactModel> GetContacts(int userId);

    Task<ContactModel?> GetContact(int userId, int id);

    IEnumerable<ExternalContactModel> GetPostedContacts();

    Task<ExternalContactModel?> GetPostedContact(int id);

    Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model);

    Task DeleteContactAsync(int userId, int id);

    IEnumerable<ContactModel> GetPartnerContacts(int partnerId);
    Task<ContactModel?> GetContactAsync(int id);
}