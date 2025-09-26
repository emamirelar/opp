namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Utilities.Interfaces;

public class CommonEntitiesManager : IApplicationService
{
    private IMapper mapper;
    CommonEntityRepository repository;

    public CommonEntitiesManager(IMapper mapper, UNOPSAppDbContext context)
    {
        this.mapper = mapper;
        repository = new CommonEntityRepository(context);
    }
}