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

namespace UNOPS.PAO.Presentation.Controllers
{
    [Route("/")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class GmailAddonController : ControllerBase
    {
        private readonly IInteractionManager _interactionManager;
        private readonly IContactManager _contactManager;

        public GmailAddonController(IManagerWrapper manager)
        {
            _interactionManager = manager.InteractionManager;
            _contactManager = manager.ContactManager;
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

                // Find related contacts
                var contacts = await _contactManager.GetContactsForGmailAddon(input);

                if(contacts != null && contacts.Any())
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

                            if(contact.Partner != null)
                            {
                                retVal.Partners.Add(new GmailRelatedPartner
                                {
                                    Id = contact.Partner.Id,
                                    Name = contact.Partner.Name
                                });
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
    }
}
