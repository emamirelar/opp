namespace UNOPS.PAO.Presentation.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.Domain.Entities;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

[Route("/")]
[ApiController]
[Authorize]
public class GeminiController : ControllerBase
{
    private readonly IGeminiManager manager;

    public GeminiController(IManagerWrapper manager)
    {
        this.manager = manager.GeminiManager;
    }

    [HttpPost(APIDictionary.Gemini)]
    // Internal call: Create a Gemini
    public async Task<ActionResult> FetchResponseFromGemini([FromBody] GeminiProcessRequest req)
    {
        try {
            bool isFromAiAssistant = req.AiAssistant;
            string relatedMessage = "";

            AiPrompt promptModel = manager.MapModelToEntity(req);

            // Call the GetPromptData method and get the first prompt
            var promptData = manager.GetPromptData(promptModel.Type).FirstOrDefault();

            if (promptData == null)
            {
                return NotFound(new { message = $"Prompt configuration for the screen '{req.Type}' is not found." });
            }

            if (!isFromAiAssistant) {
                // Query the AiScreenMapping table based on Type
                var screenMappings = (await manager.GetScreenMappingsByType(promptData.Type)).ToArray();
                relatedMessage = await manager.GetDataBasedOnScreenMapping(promptData.Type, req.Id, screenMappings);
            } else {
                relatedMessage = req.Message;
            }

            // Fetch result from Gemini
            return Ok(await manager.fetchResultFromGemini(promptData, relatedJsonData));

        } catch (Exception ex) {
            return BadRequest(new { message = ex.Message });
        }
    }
}
