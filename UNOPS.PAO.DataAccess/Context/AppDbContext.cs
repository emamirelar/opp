using UNOPS.PAO.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using UNOPS.PAO.UNOPSDomain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UNOPS.PAO.DataAccess.Context;

public class AppDbContext : AuditableDbContext<int, int>
{
    private readonly UserResolverService<int> _userResolverService;
    public AppDbContext(DbContextOptions<AppDbContext> options, UserResolverService<int> userResolverService, IDbContextSchema schema)
        : base(options, userResolverService, schema)
    {
    }

    protected AppDbContext(DbContextOptions options, UserResolverService<int> userResolverService, IDbContextSchema schema) : base(options, userResolverService, schema)
    {
    }

    public DbSet<PAOUser> PAOUsers { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<PartnerAgreement> PartnerAgreements { get; set; }

    public DbSet<EligibleEntity> EligibleEntities { get; set; }

    public DbSet<EntityUserRole> EntityUserRoles { get; set; }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Interaction> Interactions { get; set; }
    public DbSet<Partner> Partners { get; set; }
    public DbSet<PartnerTree> PartnerTrees { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentRelationship> DocumentRelationships { get; set; }
    public DbSet<OrganizationUnitRelationship> OrganizationUnitRelationships { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<UNOPS.PAO.Domain.Entities.Link> Links { get; set; }
    public DbSet<OrganizationHierarchy> OrganizationHierarchies { get; set; }
    public DbSet<LiaisonOffice> LiaisonOffices { get; set; }

    public DbSet<EntityEmbeddings> EntityEmbeddings { get; set; }
    public DbSet<InteractionContact> InteractionContacts { get; set; }
    public DbSet<InteractionUser> InteractionUsers { get; set; }
    public DbSet<InteractionPartner> InteractionPartners { get; set; }
    public DbSet<UserProfile> UserProfile { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    public DbSet<Notification> Notifications { get; set; }
    public DbSet<SavedFilter> SavedFilters { get; set; }

    // Opportunity and related entities
    public DbSet<Opportunity> Opportunities { get; set; }
    public DbSet<OpportunityFundingPartner> OpportunityFundingPartners { get; set; }
    public DbSet<OpportunityClientPartner> OpportunityClientPartners { get; set; }
    public DbSet<OpportunityStakeholder> OpportunityStakeholders { get; set; }
    public DbSet<OpportunityDeliverable> OpportunityDeliverables { get; set; }
    public DbSet<OpportunityCountry> OpportunityCountries { get; set; }
    public DbSet<OpportunitySDG> OpportunitySDGs { get; set; }
    public DbSet<OpportunitySDGTarget> OpportunitySDGTargets { get; set; }
    public DbSet<OpportunitySDGIndicator> OpportunitySDGIndicators { get; set; }
    public DbSet<OpportunityUNCFOutcome> OpportunityUNCFOutcomes { get; set; }
    public DbSet<OpportunityUNCFIndicator> OpportunityUNCFIndicators { get; set; }
    public DbSet<OpportunityUNOPSMission> OpportunityUNOPSMissions { get; set; }
    public DbSet<OpportunityInteraction> OpportunityInteractions { get; set; }

    // Infrastructure entities
    public DbSet<EntityRole> EntityRoles { get; set; }
    public DbSet<EntityRolePerson> EntityRolePersons { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ProposedInitiativeType> ProposedInitiativeTypes { get; set; }
    public DbSet<Comment> Comments { get; set; }

    // Artifacts system entities
    public DbSet<ArtifactDataType> ArtifactDataTypes { get; set; }
    public DbSet<ArtifactType> ArtifactTypes { get; set; }
    public DbSet<EntityArtifact> EntityArtifacts { get; set; }
    public DbSet<ArtifactExtractionRule> ArtifactExtractionRules { get; set; }

    // External Data Service entities (Read-Only)
    public DbSet<SDG> SDGs { get; set; }
    public DbSet<SDGTarget> SDGTargets { get; set; }
    public DbSet<SDGIndicator> SDGIndicators { get; set; }
    public DbSet<UNCFOutcome> UNCFOutcomes { get; set; }
    public DbSet<UNCFIndicator> UNCFIndicators { get; set; }
    public DbSet<UNCFMetadata> UNCFMetadatas { get; set; }
    public DbSet<UNOPSMission> UNOPSMissions { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    
    // Output catalog entities
    public DbSet<Unit> Units { get; set; }
    public DbSet<ProjectCategory> ProjectCategories { get; set; }
    public DbSet<Output> Outputs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings
            .Ignore(RelationalEventId.PendingModelChangesWarning));
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder
            .Entity<IdentityUserRole<int>>()
            .ToTable("AspNetUserRoles", t => t.ExcludeFromMigrations())
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        // Configure PAOUser and UserProfile relationship
        modelBuilder
            .Entity<PAOUser>()
            .ToTable("AspNetUsers", t => t.ExcludeFromMigrations())
            .HasOne(x => x.UserProfile)
            .WithOne()
            .HasForeignKey<UserProfile>(x => x.UserId)
            .IsRequired(false); // Make it optional to avoid constraint issues during creation

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfile", "public");
            entity.HasKey(e => e.UserId);
            
            entity.Property(up => up.UserId)
                .IsRequired();

            entity.HasIndex(up => up.UserId)
                .IsUnique();
                
            // Ignore the computed Name property since it's calculated from FirstName and LastName
            entity.Ignore(e => e.Name);
                
            entity.Property(e => e.FirstName)
                .HasMaxLength(100);
            
            entity.Property(e => e.LastName)
                .HasMaxLength(100);
            
            entity.Property(e => e.UserEmail)
                .HasMaxLength(256);
            
            entity.Property(e => e.OrgUnit)
                .HasMaxLength(200);
                
            entity.Property(e => e.DutyStation)
                .HasMaxLength(200);
                
            entity.Property(e => e.Position)
                .HasMaxLength(200);
        });

        modelBuilder
            .Entity<Partner>(p =>
            {
                // Configure one-to-many relationship with Contacts
                p.HasMany(x => x.Contacts)
                    .WithOne(c => c.Partner)
                    .HasForeignKey(c => c.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure ErpDimValue as a unique index (allows nulls, unlike alternate keys)
                p.HasIndex(x => x.ErpDimValue)
                    .IsUnique()
                    .HasFilter("\"ErpDimValue\" IS NOT NULL");

                p.Ignore(x => x.OrganizationUnitRelationships);
            });

        modelBuilder
            .Entity<Contact>()
            .Ignore(c => c.OrganizationUnitRelationships); // Handle manually through extension methods

        modelBuilder.Entity<EntityUserRole>(entity =>
        {
            entity.HasOne(e => e.UserRole)
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RoleId });
        });

        modelBuilder.Entity<Interaction>(entity =>
        {
            entity.Property(e => e.EmailAddresses)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                            new ValueComparer<List<string>>(
                                (c1, c2) => c1.SequenceEqual(c2),
                                  c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                  c => c.ToList()))
                  .HasColumnType("text");

            entity.HasMany(i => i.InteractionContacts)
                .WithOne(ic => ic.Interaction)
                .HasForeignKey(ic => ic.InteractionId);

            entity.HasMany(i => i.InteractionUsers)
                .WithOne(iu => iu.Interaction)
                .HasForeignKey(iu => iu.InteractionId);

            entity.HasMany(i => i.InteractionPartners)
                .WithOne(ip => ip.Interaction)
                .HasForeignKey(ip => ip.InteractionId);

            entity.Ignore(x => x.OrganizationUnitRelationships);
        });

        modelBuilder
            .Entity<InteractionContact>()
            .HasKey(ic => new { ic.InteractionId, ic.ContactId });

        modelBuilder
            .Entity<InteractionPartner>()
            .HasKey(ip => new { ip.InteractionId, ip.PartnerId });

        modelBuilder
            .Entity<InteractionUser>()
            .HasKey(iu => new { iu.InteractionId, iu.UserId });

        modelBuilder
            .Entity<PartnerTree>();

        // Add discriminator configuration for PartnerTree inheritance hierarchy
        modelBuilder
            .Entity<PartnerTree>()
            .HasDiscriminator<string>("Discriminator")
            .HasValue<PartnerTree>("PartnerTree");

        modelBuilder
            .Entity<AiPrompt>();

        // AiChatSession entity removed - session data now managed by ADK session state

        modelBuilder.Entity<Document>(doc =>
        {
            doc.HasOne(x => x.DocumentType)
                .WithMany()
                .HasForeignKey(x => x.DocumentTypeId);
        });

        modelBuilder.Entity<DocumentRelationship>()
            .HasKey(dr => new { dr.DocumentId, dr.EntityId, dr.EntityType });

        modelBuilder.Entity<DocumentRelationship>(entity =>
        {
            entity.HasKey(e => new { e.DocumentId, e.EntityId, e.EntityType });

            entity.HasOne(e => e.Document)
                .WithMany(d => d.DocumentRelationships)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EntityId)
                .IsRequired();

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => new { e.EntityId, e.EntityType });
        });

        modelBuilder.Entity<OrganizationUnitRelationship>(entity =>
        {
            
            entity.HasOne(e => e.OrganizationHierarchy)
                .WithMany(o => o.EntityRelationships)
                .HasForeignKey(e => e.OrganizationHierarchyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.EntityId)
                .IsRequired();

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => new { e.EntityId, e.EntityType });
            
            // Add unique constraint for the business logic (one relationship per entity/org unit combo)
            entity.HasIndex(e => new { e.EntityId, e.EntityType, e.OrganizationHierarchyId })
                .IsUnique();

        });

        modelBuilder
            .Entity<UNOPS.PAO.Domain.Entities.Link>()
            .ToTable("Links", "public")
            .HasDiscriminator<string>("Discriminator")
            .HasValue<UNOPS.PAO.Domain.Entities.Link>("Link")
            .HasValue<UNOPSLink>("UNOPSLink");
            
        modelBuilder
            .Entity<UNOPSLink>();

        modelBuilder.Entity<EntityEmbeddings>(entity =>
        {
            entity.HasIndex(e => e.EntityName);
            entity.HasIndex(e => e.EntityId);
            entity.Property(e => e.FullEmbedding)
              .HasColumnType("vector(768)");
            entity.HasIndex(e => new { e.EntityName, e.EntityId })
                    .IsUnique(); // This ensures uniqueness at the database level
        });

        modelBuilder.Entity<OrganizationHierarchy>(entity =>
        {
            entity.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Type)
                .HasColumnType("text")
                .IsRequired();

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.ToTable("UserPreferences", "public");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityByDefaultColumn();
            
            entity.Property(e => e.UserId)
                .IsRequired();
            
            // Foreign key relationship to UserProfile
            entity.HasOne(e => e.UserProfile)
                .WithOne(up => up.UserPreference)
                .HasForeignKey<UserPreference>(e => e.UserId)
                .HasPrincipalKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.GlobalFilterJson)
                .HasColumnType("text");
            
            entity.Property(e => e.AdditionalSettingsJson)
                .HasColumnType("text");
        });


        // Ignore GlobalFilters class - it's not an entity, just a plain class for JSON serialization
        modelBuilder.Ignore<GlobalFilters>();

        // Opportunity entity configuration
        modelBuilder.Entity<Opportunity>(entity =>
        {
            entity.HasOne(x => x.ResponsibleOrgUnit)
                .WithMany()
                .HasForeignKey(x => x.ResponsibleOrgUnitId)
                .IsRequired(false);
                
            entity.HasOne(x => x.ProposedInitiativeType)
                .WithMany()
                .HasForeignKey(x => x.ProposedInitiativeTypeId)
                .IsRequired(false);
                
            entity.Property(x => x.Description)
                .IsRequired();
                
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Status);
        });

        // OpportunityFundingPartner configuration
        modelBuilder.Entity<OpportunityFundingPartner>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.FundingPartners)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId);
                
            entity.HasOne(x => x.Currency)
                .WithMany()
                .HasForeignKey(x => x.CurrencyId)
                .IsRequired();
                
            entity.HasIndex(x => x.OpportunityId);
        });

        // OpportunityClientPartner configuration
        modelBuilder.Entity<OpportunityClientPartner>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.ClientPartners)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Partner)
                .WithMany()
                .HasForeignKey(x => x.PartnerId);
                
            entity.HasIndex(x => x.OpportunityId);
        });

        // OpportunityStakeholder configuration
        modelBuilder.Entity<OpportunityStakeholder>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.Stakeholders)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired(false);
                
            entity.HasOne(x => x.EntityRole)
                .WithMany()
                .HasForeignKey(x => x.EntityRoleId)
                .IsRequired(false);
                
            entity.HasIndex(x => x.OpportunityId);
        });

        // OpportunityDeliverable configuration
        modelBuilder.Entity<OpportunityDeliverable>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.Deliverables)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasIndex(x => x.OpportunityId);
        });

        // OpportunityCountry configuration
        modelBuilder.Entity<OpportunityCountry>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.Countries)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.Country)
                .WithMany()
                .HasForeignKey(x => x.CountryId);
                
            entity.HasIndex(x => x.OpportunityId);
        });

        // OpportunitySDG configuration
        modelBuilder.Entity<OpportunitySDG>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.SDGs)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.SDG)
                .WithMany()
                .HasForeignKey(x => x.SDGId);
                
            entity.HasIndex(x => x.OpportunityId);
            entity.HasIndex(x => x.SDGId);
        });

        // OpportunitySDGTarget configuration (cascades delete from OpportunitySDG)
        modelBuilder.Entity<OpportunitySDGTarget>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.SDGTargets)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.OpportunitySDG)
                .WithMany(x => x.Targets)
                .HasForeignKey(x => x.OpportunitySDGId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.SDGTarget)
                .WithMany()
                .HasForeignKey(x => x.SDGTargetId);
                
            entity.HasIndex(x => x.OpportunityId);
            entity.HasIndex(x => x.OpportunitySDGId);
            entity.HasIndex(x => x.SDGTargetId);
        });

        // OpportunitySDGIndicator configuration (cascades delete from OpportunitySDGTarget)
        modelBuilder.Entity<OpportunitySDGIndicator>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.SDGIndicators)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.OpportunitySDGTarget)
                .WithMany(x => x.Indicators)
                .HasForeignKey(x => x.OpportunitySDGTargetId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.SDGIndicator)
                .WithMany()
                .HasForeignKey(x => x.SDGIndicatorId);

            entity.HasIndex(x => x.OpportunityId);
            entity.HasIndex(x => x.OpportunitySDGTargetId);
            entity.HasIndex(x => x.SDGIndicatorId);
        });

        // OpportunityUNCFOutcome configuration (cascades delete from OpportunityCountry)
        modelBuilder.Entity<OpportunityUNCFOutcome>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.UNCFOutcomes)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.OpportunityCountry)
                .WithMany(x => x.UNCFOutcomes)
                .HasForeignKey(x => x.OpportunityCountryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.UNCFOutcome)
                .WithMany()
                .HasForeignKey(x => x.UNCFOutcomeId);
                
            entity.HasIndex(x => x.OpportunityId);
            entity.HasIndex(x => x.OpportunityCountryId);
            entity.HasIndex(x => x.UNCFOutcomeId);
        });

        // OpportunityUNCFIndicator configuration (cascades delete from OpportunityUNCFOutcome)
        modelBuilder.Entity<OpportunityUNCFIndicator>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.UNCFIndicators)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.OpportunityUNCFOutcome)
                .WithMany(x => x.Indicators)
                .HasForeignKey(x => x.OpportunityUNCFOutcomeId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.UNCFIndicator)
                .WithMany()
                .HasForeignKey(x => x.UNCFIndicatorId);

            entity.HasIndex(x => x.OpportunityId);
            entity.HasIndex(x => x.OpportunityUNCFOutcomeId);
            entity.HasIndex(x => x.UNCFIndicatorId);
        });

        // EntityRole configuration
        modelBuilder.Entity<EntityRole>(entity =>
        {
            entity.HasIndex(x => new { x.EntityType, x.Name });
            entity.HasIndex(x => x.Code).IsUnique();
        });

        // EntityRolePerson configuration
        modelBuilder.Entity<EntityRolePerson>(entity =>
        {
            entity.HasOne(x => x.EntityRole)
                .WithMany()
                .HasForeignKey(x => x.EntityRoleId);
                
            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired(false);
                
            entity.HasOne(x => x.Contact)
                .WithMany()
                .HasForeignKey(x => x.ContactId)
                .IsRequired(false);
                
            entity.HasIndex(x => new { x.EntityType, x.EntityId, x.EntityRoleId });
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(x => new { x.EntityType, x.EntityId });
            entity.HasIndex(x => x.Timestamp);
        });

        // ArtifactDataType configuration
        modelBuilder.Entity<ArtifactDataType>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.Order);
        });

        // ArtifactType configuration
        modelBuilder.Entity<ArtifactType>(entity =>
        {
            entity.HasOne(x => x.ArtifactDataType)
                .WithMany(x => x.ArtifactTypes)
                .HasForeignKey(x => x.ArtifactDataTypeId);
                
            entity.HasIndex(x => x.ArtifactTypeCode).IsUnique();
            entity.HasIndex(x => x.Category);
            entity.HasIndex(x => x.Order);
        });

        // EntityArtifact configuration
        modelBuilder.Entity<EntityArtifact>(entity =>
        {
            entity.HasOne(x => x.ArtifactType)
                .WithMany(x => x.EntityArtifacts)
                .HasForeignKey(x => x.ArtifactTypeId);
                
            entity.HasOne(x => x.Document)
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .IsRequired(false);
                
            entity.HasOne(x => x.SourceArtifact)
                .WithMany(x => x.ExtractedArtifacts)
                .HasForeignKey(x => x.SourceArtifactId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(x => new { x.EntityType, x.EntityId });
            entity.HasIndex(x => x.ArtifactTypeId);
            entity.HasIndex(x => x.EffectiveDate);
            entity.HasIndex(x => x.IsExtracted);
        });

        // ArtifactExtractionRule configuration
        modelBuilder.Entity<ArtifactExtractionRule>(entity =>
        {
            entity.HasOne(x => x.SourceArtifactType)
                .WithMany(x => x.SourceExtractionRules)
                .HasForeignKey(x => x.SourceArtifactTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(x => x.ExtractedArtifactType)
                .WithMany(x => x.TargetExtractionRules)
                .HasForeignKey(x => x.ExtractedArtifactTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(x => x.SourceArtifactTypeId);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.ExecutionOrder);
        });

        // SDG configuration (External Data Service - Read Only)
        modelBuilder.Entity<SDG>(entity =>
        {
            entity.HasIndex(x => x.SDGNumber);
            entity.HasIndex(x => x.SDGId);
            entity.HasIndex(x => x.Status);
        });

        // SDGTarget configuration (External Data Service - Read Only)
        modelBuilder.Entity<SDGTarget>(entity =>
        {
            entity.HasIndex(x => x.SDGTargetId);
            entity.HasIndex(x => x.SDGId);
            entity.HasIndex(x => x.TargetType);
            entity.HasIndex(x => x.Status);
        });

        // SDGIndicator configuration (External Data Service - Read Only)
        modelBuilder.Entity<SDGIndicator>(entity =>
        {
            entity.HasIndex(x => x.SDGIndicatorId);
            entity.HasIndex(x => x.SDGTargetId);
            entity.HasIndex(x => x.Status);
        });

        // UNCFOutcome configuration (External Data Service - Read Only)
        modelBuilder.Entity<UNCFOutcome>(entity =>
        {
            entity.HasIndex(x => x.UNCFOutcomeId);
            entity.HasIndex(x => x.Country);
            entity.HasIndex(x => x.UNCooperationFrameworkVersionNo);
            entity.HasIndex(x => x.Status);
        });

        // UNCFIndicator configuration (External Data Service - Read Only)
        modelBuilder.Entity<UNCFIndicator>(entity =>
        {
            entity.HasIndex(x => x.UNCFIndicatorId);
            entity.HasIndex(x => x.UNCFOutcomeExternalId);
            entity.HasIndex(x => x.Country);
            entity.HasIndex(x => x.UNCooperationFrameworkVersionNo);
            entity.HasIndex(x => x.UNCFIndicatorStartDate);
            entity.HasIndex(x => x.UNCFIndicatorEndDate);
            entity.HasIndex(x => x.Status);
            
            // Composite index for efficient parent outcome lookups
            entity.HasIndex(x => new { x.UNCFOutcomeExternalId, x.UNCooperationFrameworkVersionNo });
        });

        // UNCFMetadata configuration (External Data Service - Read Only)
        modelBuilder.Entity<UNCFMetadata>(entity =>
        {
            entity.HasIndex(x => x.UNCFMetadataId);
            entity.HasIndex(x => x.Country);
            entity.HasIndex(x => x.UNCooperationFrameworkVersionNo);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.UNCFLastUpdatedDate);
            
            // Composite unique index for Country + Version combination
            entity.HasIndex(x => new { x.Country, x.UNCooperationFrameworkVersionNo }).IsUnique();
        });

        // UNOPSMission configuration (reference data - follows SDG/UNCFOutcome pattern)
        modelBuilder.Entity<UNOPSMission>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.DisplayOrder);
            entity.HasIndex(x => x.Status);
        });

        // OpportunityUNOPSMission configuration (junction table - follows OpportunitySDG pattern)
        modelBuilder.Entity<OpportunityUNOPSMission>(entity =>
        {
            entity.HasOne(x => x.Opportunity)
                .WithMany(x => x.UNOPSMissions)
                .HasForeignKey(x => x.OpportunityId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(x => x.UNOPSMission)
                .WithMany(x => x.Opportunities)
                .HasForeignKey(x => x.UNOPSMissionId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(x => x.OpportunityId);
            entity.HasIndex(x => x.UNOPSMissionId);
            
            // Unique constraint to prevent duplicate mission assignments
            entity.HasIndex(x => new { x.OpportunityId, x.UNOPSMissionId }).IsUnique();
        });

        // Country configuration (External Data Service - Read Only)
        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasIndex(x => x.Iso2Code).IsUnique();
            entity.HasIndex(x => x.Iso3Code);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.RegionDescription);
            entity.HasIndex(x => x.ContinentDescription);
            entity.HasIndex(x => x.Status);
        });

        // Currency configuration (External Data Service - Read Only)
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Status);
        });

        // ExchangeRate configuration (External Data Service - Read Only)
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasIndex(x => new { x.Currency, x.Effective_Date });
            entity.HasIndex(x => x.Status);
        });

        // Comment configuration
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasOne(x => x.ParentComment)
                .WithMany(x => x.Replies)
                .HasForeignKey(x => x.ParentCommentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.EntityType, x.EntityId });
            entity.HasIndex(x => x.ParentCommentId);
            entity.HasIndex(x => x.CreatedDate);
            entity.HasIndex(x => x.IsPinned);
        });
    }
}
