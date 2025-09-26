namespace UNOPS.PAO.UNOPSPresentation.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSPresentation.Helpers;

[Route("/")]
[ApiController]
[Authorize(AuthenticationSchemes = "IAP")]

public class CommonEntitiesController : ControllerBase
{
    CommonEntitiesManager manager;

    public CommonEntitiesController(CommonEntitiesManager manager)
    {
        this.manager = manager;
    }

}