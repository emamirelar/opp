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
        AiPrompt request = manager.MapModelToEntity(req);

        // Call the GetPromptData method and get the first prompt
        var prompt = manager.GetPromptData(request.Type).FirstOrDefault();

        if (prompt == null)
        {
            return BadRequest();
        }

        // Query the AiScreenMapping table based on Type

        var screenMappings = (await manager.GetScreenMappingsByType(request.Type)).ToArray();
        var relatedJsonData = await manager.GetDataBasedOnScreenMapping(req.Id, screenMappings);

        // Fetch result from Gemini
        return Ok(await manager.fetchResultFromGemini(prompt.Prompt, relatedJsonData));
    }
}
