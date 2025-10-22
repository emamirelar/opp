using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Models;

using UNOPS.PAO.UNOPSBusiness.Authorization;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.DataAccess.Services;
using System.Net.Mail;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Migrations;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using Google.Apis.Drive.v3.Data;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Models.Integrations;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Integrations
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class GmailAddonController : BaseController
    {
        private readonly IInteractionManager _interactionManager;
        private readonly IGmailAddonManager _gmailAddonManager;

        protected new int CurrentUserId => _userResolverService.GetCurrentUserId();

        public GmailAddonController(IManagerWrapper manager,
        UserResolverService<int> userResolverService,
        ILogger<GmailAddonController> logger,
        IAuthorizationService authorizationService,
        IPermissionService permissionService) : base(logger, authorizationService, userResolverService, permissionService)
        {
            _interactionManager = manager.InteractionManager;
            _gmailAddonManager = manager.GmailAddonManager;
        }

        [HttpPost(APIDictionary.GmailAddonInteraction)]
        [AccessControlled(EntityTypes.Interaction, "create")]
        public async Task<IActionResult> CreateInteraction([FromBody] InteractionRequest model)
        {
            var result = await _interactionManager.CreateGmailInteractionAsync(model);
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
                var response = await _gmailAddonManager.FindRelatedRecordsAsync(input, User);
                return Ok(response);
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
                var result = await _gmailAddonManager.CreateRecordsFromEmailsAsync(request, User);
                
                var response = new
                {
                    CreatedContacts = result.CreatedContacts,
                    CreatedPartners = result.CreatedPartners,
                    FailedEmails = result.FailedEmails,
                    Success = result.Success,
                    Message = result.Message
                };

                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contacts from emails");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
}
