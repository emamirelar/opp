# business-projects-and-opportunities
Github repo for Partner and Opportunity project




## Database Migrations 


```bash
dotnet ef migrations list --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```

```bash
dotnet ef migrations add RemoveUnusedGrantPlusEntities --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```

```bash
dotnet ef database update --context UNOPS.PAO.UNOPSDataAccess.Context.UNOPSAppDbContext --project UNOPS.PAO.UNOPSDataAccess/UNOPS.PAO.UNOPSDataAccess.csproj --startup-project UNOPS.PAO.Server
```

