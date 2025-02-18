namespace UNOPS.PAO.DataAccess.Context;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using UNOPS.PAO.DataAccess.Interfaces;

public class DbSchemaAwareModelCacheKeyFactory
    : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
    {
        return (context.GetType(), context is IDbContextSchema schema ? schema.Schema : null, designTime);
    }
}