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
                retVal.UnmatchedEmails = input.EmailAddresses;

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
                                retVal.Contacts.Add(new GmailRelatedContact
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
                                });
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

                            retVal.UnmatchedEmails.Remove(contact.Email);
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

                                if (partner.First5ContactsByDate != null && partner.First5ContactsByDate.Count() > 0)
                                {
                                    foreach (ContactModel? contact in partner.First5ContactsByDate)
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
                return Ok(retVal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
