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

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class GmailAddonController : ControllerBase
    {
        private readonly IInteractionManager _interactionManager;
        private readonly IContactManager _contactManager;
        protected readonly IPermissionService _permissionService;
        protected readonly ILogger _logger;
        protected readonly UserResolverService<int> _userResolverService;

        protected int CurrentUserId => _userResolverService.GetCurrentUserId();

        public GmailAddonController(IManagerWrapper manager,
            ILogger<GmailAddonController> logger,
            UserResolverService<int> userResolverService,
            IPermissionService permissionService = null)
        {
            _interactionManager = manager.InteractionManager;
            _contactManager = manager.ContactManager;
            _permissionService = permissionService;
            _logger = logger;
            _userResolverService = userResolverService;
        }

        [HttpPost(APIDictionary.GmailAddonInteraction)]
        public async Task<IActionResult> CreateInteraction([FromBody] InteractionRequest model)
        {
            var result = await _interactionManager.CreateGmailInteractionAsync(model);
            return Ok(result);
        }

        [HttpPut(APIDictionary.GmailAddonInteraction)]
        public async Task<IActionResult> UpdateInteraction([FromBody] UpdateInteractionRequest model)
        {
            var result = await _interactionManager.UpdateGmailInteractionAsync(model);
            return Ok(result);
        }

        [HttpPost(APIDictionary.GmailAddonFindInteraction)]
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

                // Check permission to read contacts
                var permissionResult = await CheckEntityPermissionAsync("Contact", "read");

                if (permissionResult != null)
                {
                    retVal.ContactPermission = "You do not have permission to read contacts.";
                    return Ok(retVal);
                }

                // Find related contacts
                var contacts = await _contactManager.GetContactsForGmailAddon(input);

                List<int> addedPartnerIds = new List<int>();

                if (contacts != null && contacts.Any())
                {
                    foreach(ContactModel? contact in contacts)
                    {
                        if (contact != null)
                        {
                            retVal.Contacts.Add(new GmailRelatedContact
                            {
                                Name = $"{contact.Salutation} {contact.FirstName} {contact.MiddleName} {contact.LastName}",
                                Title = contact.Title,
                                PartnerName = contact.Partner?.Name ?? string.Empty,
                                Id = contact.Id,
                            });

                            if(contact.Partner != null && !addedPartnerIds.Contains(contact.Partner.Id))
                            {
                                retVal.Partners.Add(new GmailRelatedPartner
                                {
                                    Id = contact.Partner.Id,
                                    Name = contact.Partner.Name,
                                    PartnerCode = contact.Partner.PartnerCode,
                                    Phone = contact.Partner.Phone
                                });

                                addedPartnerIds.Add(contact.Partner.Id);
                            }

                            retVal.UnmatchedEmails.Remove(contact.Email);
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

        protected async Task<ActionResult> CheckEntityPermissionAsync(string entityName, string action, object entity = null)
        {
            if (_permissionService == null)
            {
                _logger.LogWarning("IPermissionService not available in controller {ControllerName}. Permission check skipped.", GetType().Name);
                return null; // Allow access if permission service is not available
            }

            if (!await _permissionService.CanPerformActionAsync(entityName, action, User, entity))
            {
                _logger.LogWarning("User {UserId} attempted to perform {Action} on {EntityName} without permission",
                    CurrentUserId, action, entityName);
                return Forbid();
            }

            return null;
        }
    }
}
