namespace UNOPS.PAO.DataAccess.Context;

using UNOPS.PAO.DataAccess.Interfaces;

public class DbContextSchema : IDbContextSchema
{
    public DbContextSchema(string schema)
    {
        Schema = schema;
    }

    public string Schema { get; set; }
}