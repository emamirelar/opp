using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Identity.Security.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Documents;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Documents;
[Route("/")]
public class DocumentTypeController : BaseController
{
    private readonly IDocumentTypeManager _manager;

    public DocumentTypeController(
        IManagerWrapper manager,
        ILogger<DocumentTypeController> logger,
        IAuthorizationService authorizationService,
        UserResolverService<int> userResolverService)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = manager.DocumentTypeManager;
    }

    [HttpGet(APIDictionary.DocumentType + "/{entityName}")]
    public async Task<ActionResult> GetAll(string entityName,
        int pageIndex = -1, int pageSize = 10,
        string? orderBy = null, bool? ascending = true)
    {
        return await HandleOperationAsync(async () =>
        {
            var parameters = new DocumentTypeRequestParameters()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                OrderBy = orderBy,
                Ascending = ascending,
                EntityType = EntityNames.ByName(entityName)
            };

            return await Task.FromResult(_manager.GetDocumentTypesAsync(parameters));
        });
    }
}
