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

        public GmailAddonController(IManagerWrapper manager)
        {
            _interactionManager = manager.InteractionManager;
        }

        [HttpPost(APIDictionary.GmailAddonInteraction)]
        public async Task<IActionResult> CreateInteraction([FromBody] InteractionRequest model)
        {
            var result = await _interactionManager.CreateInteractionAsync(model);
            return Ok(result);
        }
    }
}
