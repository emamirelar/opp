using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.UNOPSBusiness.Authorization;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using System.Net.Mail;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Migrations;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class GmailAddonController : GmailAddonBaseController
    {
        private readonly IInteractionManager _interactionManager;
        private readonly IContactManager _contactManager;
        private readonly IPartnerManager _partnerManager;

        protected int CurrentUserId => _userResolverService.GetCurrentUserId();

        public GmailAddonController(IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<GmailAddonController> logger,
        IAuthorizationService authorizationService,
        IPermissionService permissionService) : base(logger, authorizationService, userResolverService, permissionService)
        {
            _interactionManager = manager.InteractionManager;
            _contactManager = manager.ContactManager;
            _partnerManager = manager.PartnerManager;
        }

        [HttpPost(APIDictionary.GmailAddonInteraction)]
        [AccessControlled(EntityTypes.Interaction, "create")]
        public async Task<IActionResult> CreateInteraction([FromBody] InteractionRequest model)
        {
            var result = await _interactionManager.CreateGmailInteractionAsync(model);
            return Ok(result);
        }

        [HttpPut(APIDictionary.GmailAddonInteraction)]
        [AccessControlled(EntityTypes.Interaction, "update")]
        public async Task<IActionResult> UpdateInteraction([FromBody] UpdateInteractionRequest model)
        {
            var result = await _interactionManager.UpdateGmailInteractionAsync(model);
            return Ok(result);
        }

        [HttpPost(APIDictionary.GmailAddonFindInteraction)]
        [AccessControlled(EntityTypes.Interaction, "read")]
        public async Task<IActionResult> FindGmailInteraction([FromBody] GmailInteractionRequest model)
        {
            try
            {
                var interaction = await _interactionManager.FindGmailInteractionAsync(model);
                if (interaction == null)
                {
                    return NotFound($"No interaction found with Gmail Thread ID: {model.GmailThreadId}");
                }
                return Ok(interaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost(APIDictionary.GmailAddonFindRelatedRecords)]
        public async Task<IActionResult> FindRelatedRecords([FromBody] GmailRelatedRecordsRequest input)
        {
            try
            {
                var retVal = new GmailRelatedRecordsResponse();
                var unmatchedEmailStrings = new List<string>(input.EmailAddresses);

                input.partnerIds = new List<int>();

                //List<int> contactPartnerIds = new List<int>();
                List<int> softPartnerIds = new List<int>();

                var contactCreatePermissionResult = await CheckEntityPermissionAsync("Contact", "create");

                if (contactCreatePermissionResult == null)
                {
                    retVal.CanCreateContacts = true;
                }

                var contacts = await _contactManager.GetContactsForGmailAddon(input, User);
                if (contacts != null && contacts.Any())
                {
                    foreach (ContactModel? contact in contacts)
                    {
                        if (contact != null)
                        {
                            if (contact.Permissions.CanRead)
                            {
                                var gmailContact = new GmailRelatedContact
                                {
                                    Name = $"{contact.Salutation} {contact.FirstName} {contact.MiddleName} {contact.LastName}",
                                    Title = contact.Title,
                                    PartnerName = contact.Partner?.Name ?? string.Empty,
                                    Id = contact.Id,
                                    EmailAddress = contact.Email,
                                    Location = !string.IsNullOrEmpty(contact.MailingCity) && !string.IsNullOrEmpty(contact.MailingCountry)
                                                ? $"{contact.MailingCity}, {contact.MailingCountry}"
                                                : null,
                                    Phone = contact.Phone,
                                    ProfilePictureUrl = contact.ProfilePictureUrl,
                                    CanRead = true
                                };

                                // Add interactions if available
                                if (contact.Interactions != null && contact.Interactions.Any())
                                {
                                    gmailContact.Interactions = contact.Interactions
                                        .Where(i => i.Permissions.CanRead)
                                        .Select(i => new GmailRelatedInteraction
                                        {
                                            Id = i.Id,
                                            Type = i.Type.ToString(),
                                            Description = i.Description,
                                            Date = i.Date,
                                            CanRead = i.Permissions.CanRead
                                        }).ToList();
                                }

                                retVal.Contacts.Add(gmailContact);
                            }
                            else
                            {
                                retVal.Contacts.Add(new GmailRelatedContact
                                {
                                    EmailAddress = contact.Email,
                                    CanRead = false
                                });
                            }
                            
                            if (contact.Partner != null && !input.partnerIds.Contains(contact.Partner.Id))
                            {
                                input.partnerIds.Add(contact.Partner.Id);
                            }

                            unmatchedEmailStrings.Remove(contact.Email);
                        }
                    }
                }

                var partners = await _partnerManager.GetPartnersForGmailAddon(input, User);
                if (partners != null && partners.Any())
                {
                    foreach (PartnerModel? partner in partners)
                    {
                        if (partner != null)
                        {
                            if(partner.Permissions.CanRead)
                            {
                                GmailRelatedPartner currentPartner = new GmailRelatedPartner {
                                    Id = partner.Id,
                                    Name = partner.Name,
                                    PartnerCode = partner.PartnerCode,
                                    Phone = partner.Phone,
                                    LogoUrl = partner.LogoUrl,
                                    Location = !String.IsNullOrEmpty(partner.Address1City) && !String.IsNullOrEmpty(partner.Address1Country)
                                                    ? $"{partner.Address1City}, {partner.Address1Country}"
                                                    : null,
                                    CanRead = true,
                                    Contacts = new List<GmailRelatedContact>()
                                };

                                // Add partner interactions if available
                                if (partner.Interactions != null && partner.Interactions.Any())
                                {
                                    currentPartner.Interactions = partner.Interactions
                                        .Where(i => i.Permissions.CanRead)
                                        .Select(i => new GmailRelatedInteraction
                                        {
                                            Id = i.Id,
                                            Type = i.Type.ToString(),
                                            Description = i.Description,
                                            Date = i.Date,
                                            CanRead = i.Permissions.CanRead
                                        }).ToList();
                                }

                                if (partner.Contacts != null && partner.Contacts.Count() > 0)
                                {
                                    foreach (ContactModel? contact in partner.Contacts)
                                    {
                                        if (contact != null)
                                        {
                                            if (contact.Permissions.CanRead)
                                            {
                                                currentPartner.Contacts.Add(new GmailRelatedContact
                                                {
                                                    Name = $"{contact.Salutation} {contact.FirstName} {contact.MiddleName} {contact.LastName}",
                                                    Title = contact.Title,
                                                    Id = contact.Id,
                                                    EmailAddress = contact.Email,
                                                    CanRead = true
                                                });
                                            }
                                            else
                                            {
                                                currentPartner.Contacts.Add(new GmailRelatedContact
                                                {
                                                    EmailAddress = contact.Email,
                                                    CanRead = false
                                                });
                                            }
                                        }
                                    }
                                }
                                retVal.Partners.Add(currentPartner);
                            }
                            else
                            {
                                retVal.Partners.Add(new GmailRelatedPartner
                                {
                                    Name = partner.Name,
                                    CanRead = false
                                });
                            }
                        }
                    }
                }

                // Process unmatched emails with partner suggestions
                retVal.UnmatchedEmails = await _contactManager.GetUnmatchedEmailsWithPartnerSuggestionsAsync(unmatchedEmailStrings, User);

                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost(APIDictionary.GmailAddonCreateRecords)]
        [AccessControlled(EntityTypes.Contact, "create")]
        public async Task<IActionResult> CreateRecordsFromEmails([FromBody] GmailCreateRecordsRequest request)
        {
            try
            {
                if (request.SelectedContacts == null || !request.SelectedContacts.Any())
                {
                    return BadRequest("No emails selected for contact creation");
                }

                var contactCreatePermissionResult = await CheckEntityPermissionAsync("Contact", "create");
                var partnerCreatePermissionResult = await CheckEntityPermissionAsync("Partner", "create");

                if(contactCreatePermissionResult != null || partnerCreatePermissionResult != null)
                {
                    return BadRequest("User does not have necessary permission to create");
                }

                var createdContacts = new List<ContactModel>();
                var failedEmails = new List<string>();
                var createdPartners = new Dictionary<string, int>(); // Track created partners by name

                // First pass: Create unique partners for emails that don't have PartnerId
                var partnersToCreate = request.SelectedContacts
                    .Where(email => !email.PartnerId.HasValue)
                    .Select(email => new
                    {
                        Email = email,
                        PartnerName = !string.IsNullOrEmpty(email.PartnerName) 
                            ? email.PartnerName 
                            : $"{char.ToUpper(email.EmailAddress.Split('@')[1][0])}{email.EmailAddress.Split('@')[1].Substring(1)}"
                    })
                    .GroupBy(x => x.PartnerName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new { PartnerName = g.Key, FirstEmail = g.First() });

                foreach (var partnerGroup in partnersToCreate)
                {
                    try
                    {
                        var partnerRequest = new PartnerRequest
                        {
                            Name = partnerGroup.PartnerName,
                            ShortName = partnerGroup.PartnerName.Length > 10 ? partnerGroup.PartnerName.Substring(0, 10) : partnerGroup.PartnerName,
                            Status = "Draft",
                            NewEngagement = "Not Allowed",
                            PooledFund = "No",
                            DDRequired = "Yes",
                            DDEACDone = "No",
                            LevyPotentiallyApplies = "Potentially applies"
                        };

                        var createdPartner = await _partnerManager.CreatePartnerAsync(User, partnerRequest);
                        if (createdPartner != null)
                        {
                            createdPartners[partnerGroup.PartnerName] = createdPartner.Id;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to create partner {partnerGroup.PartnerName}: {ex.Message}");
                        // Mark all emails for this partner as failed
                        var failedEmailsForPartner = request.SelectedContacts
                            .Where(email => !email.PartnerId.HasValue && 
                                   ((!string.IsNullOrEmpty(email.PartnerName) && email.PartnerName.Equals(partnerGroup.PartnerName, StringComparison.OrdinalIgnoreCase)) ||
                                    (string.IsNullOrEmpty(email.PartnerName) && partnerGroup.PartnerName.Equals($"{char.ToUpper(email.EmailAddress.Split('@')[1][0])}{email.EmailAddress.Split('@')[1].Substring(1)}", StringComparison.OrdinalIgnoreCase))))
                            .Select(email => email.EmailAddress);
                        failedEmails.AddRange(failedEmailsForPartner);
                    }
                }

                // Second pass: Create contacts using existing or newly created partners
                foreach (var selectedEmail in request.SelectedContacts)
                {
                    try
                    {
                        // Skip if this email already failed during partner creation
                        if (failedEmails.Contains(selectedEmail.EmailAddress))
                        {
                            continue;
                        }

                        int partnerId;

                        // Determine the PartnerId to use
                        if (selectedEmail.PartnerId.HasValue)
                        {
                            partnerId = selectedEmail.PartnerId.Value;
                        }
                        else
                        {
                            // Use the newly created partner
                            var partnerName = !string.IsNullOrEmpty(selectedEmail.PartnerName) 
                                ? selectedEmail.PartnerName 
                                : $"{char.ToUpper(selectedEmail.EmailAddress.Split('@')[1][0])}{selectedEmail.EmailAddress.Split('@')[1].Substring(1)}";
                            
                            if (!createdPartners.TryGetValue(partnerName, out partnerId))
                            {
                                throw new Exception($"Partner {partnerName} was not created successfully");
                            }
                        }

                        // Extract name parts from email
                        var emailParts = selectedEmail.EmailAddress.Split('@');
                        var namePart = emailParts[0];

                        // Try to extract first and last name from email
                        var nameComponents = ExtractNameFromEmail(namePart);

                        var contactRequest = new ContactRequest
                        {
                            Email = selectedEmail.EmailAddress,
                            FirstName = nameComponents.FirstName,
                            LastName = nameComponents.LastName,
                            PartnerId = partnerId,
                            // Set default values for required fields
                            Salutation = "",
                            MiddleName = "",
                            Title = "",
                            Status = EntityStatus.Draft.ToString()
                        };

                        var createdContact = await _contactManager.CreateContactAsync(contactRequest);
                        if (createdContact != null)
                        {
                            createdContacts.Add(createdContact);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to create contact for email {selectedEmail.EmailAddress}: {ex.Message}");
                        failedEmails.Add(selectedEmail.EmailAddress);
                    }
                }

                var response = new
                {
                    CreatedContacts = createdContacts.Count,
                    CreatedPartners = createdPartners.Count,
                    FailedEmails = failedEmails.Distinct().ToList(),
                    Success = createdContacts.Any(),
                    Message = $"Successfully created {createdContacts.Count} contacts" +
                             (createdPartners.Any() ? $" and {createdPartners.Count} partners" : "") +
                             (failedEmails.Any() ? $", {failedEmails.Count} failed" : "")
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating contacts from emails: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private (string FirstName, string LastName) ExtractNameFromEmail(string emailPrefix)
        {
            // Simple name extraction logic - can be enhanced
            var parts = emailPrefix.Split(new char[] { '.', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                return (
                    FirstName: char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower(),
                    LastName: char.ToUpper(parts[1][0]) + parts[1].Substring(1).ToLower()
                );
            }
            else if (parts.Length == 1)
            {
                return (
                    FirstName: char.ToUpper(parts[0][0]) + parts[0].Substring(1).ToLower(),
                    LastName: ""
                );
            }

            return (FirstName: emailPrefix, LastName: "");
        }
    }
}
