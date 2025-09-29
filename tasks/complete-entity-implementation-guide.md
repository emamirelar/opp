# Complete Entity Implementation Guide

## Overview

This guide provides a comprehensive step-by-step approach for implementing new complete entities in the UNOPS PAO application with full CRUD operations and frontend views. Based on analysis of existing patterns (Partner, Contact, Interaction), this document outlines the required components across all layers of the architecture.

## Architecture Overview

The UNOPS PAO application follows a **Clean Architecture** pattern with these layers:

```
Frontend (Angular 19)
├── Components (List, View, Edit, Dialog)
├── Services (API communication)
├── Models (TypeScript interfaces)
└── Routing & Guards

Backend (.NET 8)
├── Controllers (API endpoints)
├── Managers (Business logic)
├── Models (DTOs)
├── Domain (Entities)
└── DataAccess (EF Core, Context)

Database (PostgreSQL)
├── Entity tables
├── Permission configuration
├── Relationships
└── Audit fields
```

## Backend Implementation Steps

### Step 1: Domain Entity (Required)

**Location**: `UNOPS.PAO.Domain/Entities/` or `UNOPS.PAO.UNOPSDomain/Entities/`

**Example**: Create `YourEntity.cs`

```csharp
public class YourEntity : BaseBusinessEntity
{
    // Primary business fields
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Foreign Keys
    public int? PartnerId { get; set; }
    public int? ContactId { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual UNOPSPartner? Partner { get; set; }
    
    [JsonIgnore]
    public virtual UNOPSContact? Contact { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();
}
```

**Key Requirements**:
- Inherit from `BaseBusinessEntity` (provides Id, CreatedBy, CreatedDate, etc.)
- Use `[JsonIgnore]` on navigation properties
- Follow existing naming conventions
- Include appropriate foreign key relationships

### Step 2: Data Transfer Object (DTO) Model (Required)

**Location**: `UNOPS.PAO.Models/`

**Example**: Create `YourEntityModel.cs`

```csharp
public class YourEntityModel : BaseModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Foreign Keys
    public int? PartnerId { get; set; }
    public int? ContactId { get; set; }
    
    // Related data (populated by joins)
    public string? PartnerName { get; set; }
    public string? ContactName { get; set; }
    
    // Permission-related properties
    public YourEntityPermissions? Permissions { get; set; }
    
    // Display helpers
    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : "Untitled";
    public string StatusDisplay => Status?.Replace("_", " ") ?? "Unknown";
}

public class YourEntityPermissions : BasePermissions
{
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanApprove { get; set; }
    // Add entity-specific permissions
}
```

**Key Requirements**:
- Inherit from `BaseModel`
- Include permission classes
- Add display helper properties
- Match domain entity structure but optimized for API transfer

### Step 3: Business Manager (Required)

**Location**: `UNOPS.PAO.Business/Managers/` or `UNOPS.PAO.UNOPSBusiness/Managers/`

**Example**: Create `YourEntityManager.cs`

```csharp
public interface IYourEntityManager
{
    Task<IEnumerable<YourEntityModel>> GetAllAsync(ClaimsPrincipal user);
    Task<YourEntityModel?> GetByIdAsync(ClaimsPrincipal user, int id);
    Task<YourEntityModel> CreateAsync(ClaimsPrincipal user, YourEntityModel model);
    Task<YourEntityModel> UpdateAsync(ClaimsPrincipal user, YourEntityModel model);
    Task<bool> DeleteAsync(ClaimsPrincipal user, int id);
    Task<IEnumerable<YourEntityModel>> GetByPartnerIdAsync(ClaimsPrincipal user, int partnerId);
}

public class YourEntityManager : BaseUNOPSManager, IYourEntityManager
{
    private readonly BaseRepository<YourEntity> _repository;
    
    public YourEntityManager(
        IMapper mapper, 
        UNOPSAppDbContext context, 
        IConfiguration configuration,
        IPermissionService permissionService,
        IHttpContextAccessor httpContextAccessor) 
        : base(mapper, context, configuration, null, "YourEntity", permissionService, httpContextAccessor)
    {
        _repository = new BaseRepository<YourEntity>(context, configuration, null);
    }
    
    public async Task<IEnumerable<YourEntityModel>> GetAllAsync(ClaimsPrincipal user)
    {
        var entities = await _repository.GetAllAsync(
            filter: e => !e.IsDeleted,
            include: e => e.Include(x => x.Partner)
                          .Include(x => x.Contact)
        );
        
        // Apply permission filtering
        var filteredEntities = await _permissionService.FilterEntitiesAsync(user, entities, _entityName);
        
        return await MapEntityToModelWithPermissionsAsync(user, filteredEntities);
    }
    
    public async Task<YourEntityModel?> GetByIdAsync(ClaimsPrincipal user, int id)
    {
        var entity = await _repository.GetByIdAsync(
            id: id,
            include: e => e.Include(x => x.Partner)
                          .Include(x => x.Contact)
        );
        
        if (entity == null || entity.IsDeleted) return null;
        
        // Check permissions
        var hasReadAccess = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "read");
        if (!hasReadAccess) return null;
        
        var model = _mapper.Map<YourEntityModel>(entity);
        model.Permissions = await GetEntityPermissionsAsync(user, entity);
        
        return model;
    }
    
    public async Task<YourEntityModel> CreateAsync(ClaimsPrincipal user, YourEntityModel model)
    {
        // Validate permissions
        var canCreate = await _permissionService.HasPermissionAsync(user, _entityName, "create");
        if (!canCreate) throw new UnauthorizedAccessException("Access denied");
        
        var entity = _mapper.Map<YourEntity>(model);
        entity.CreatedBy = GetCurrentUserId(user);
        entity.CreatedDate = DateTime.UtcNow;
        
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        
        return _mapper.Map<YourEntityModel>(entity);
    }
    
    public async Task<YourEntityModel> UpdateAsync(ClaimsPrincipal user, YourEntityModel model)
    {
        var entity = await _repository.GetByIdAsync(model.Id);
        if (entity == null || entity.IsDeleted) 
            throw new NotFoundException("Entity not found");
        
        // Check permissions
        var canUpdate = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "update");
        if (!canUpdate) throw new UnauthorizedAccessException("Access denied");
        
        // Map changes
        _mapper.Map(model, entity);
        entity.LastModifiedBy = GetCurrentUserId(user);
        entity.LastModifiedDate = DateTime.UtcNow;
        
        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();
        
        return _mapper.Map<YourEntityModel>(entity);
    }
    
    public async Task<bool> DeleteAsync(ClaimsPrincipal user, int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null || entity.IsDeleted) return false;
        
        // Check permissions
        var canDelete = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "delete");
        if (!canDelete) throw new UnauthorizedAccessException("Access denied");
        
        entity.IsDeleted = true;
        entity.LastModifiedBy = GetCurrentUserId(user);
        entity.LastModifiedDate = DateTime.UtcNow;
        
        await _repository.UpdateAsync(entity);
        await _repository.SaveChangesAsync();
        
        return true;
    }
    
    public async Task<IEnumerable<YourEntityModel>> GetByPartnerIdAsync(ClaimsPrincipal user, int partnerId)
    {
        var entities = await _repository.GetAllAsync(
            filter: e => e.PartnerId == partnerId && !e.IsDeleted,
            include: e => e.Include(x => x.Partner)
        );
        
        var filteredEntities = await _permissionService.FilterEntitiesAsync(user, entities, _entityName);
        return await MapEntityToModelWithPermissionsAsync(user, filteredEntities);
    }
    
    private async Task<YourEntityPermissions> GetEntityPermissionsAsync(ClaimsPrincipal user, YourEntity entity)
    {
        return new YourEntityPermissions
        {
            CanRead = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "read"),
            CanUpdate = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "update"),
            CanDelete = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "delete"),
            CanEdit = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "update"),
            CanApprove = await _permissionService.HasEntityAccessAsync(user, entity, _entityName, "approve")
        };
    }
}
```

**Key Requirements**:
- Inherit from `BaseUNOPSManager`
- Implement proper permission checking using `IPermissionService`
- Use `BaseRepository<T>` for data access
- Include proper error handling and validation
- Implement both interface and concrete class

### Step 4: API Controller (Required)

**Location**: `UNOPS.PAO.Presentation/Controllers/` or `UNOPS.PAO.UNOPSPresentation/Controllers/`

**Example**: Create `YourEntityController.cs`

```csharp
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class YourEntityController : BaseController
{
    private readonly IYourEntityManager _manager;
    
    public YourEntityController(
        IManagerWrapper managerWrapper,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<YourEntityController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = ((UNOPSManagerWrapper)managerWrapper).YourEntityManager;
    }
    
    [HttpGet(APIDictionary.YourEntities)]
    [AccessControlled(EntityTypes.YourEntity, "read")]
    public async Task<ActionResult> GetYourEntities()
    {
        try
        {
            var entities = await _manager.GetAllAsync(User);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving your entities");
            return StatusCode(500, new { error = "Failed to retrieve entities" });
        }
    }
    
    [HttpGet(APIDictionary.YourEntity + "/{id}")]
    [AccessControlled(EntityTypes.YourEntity, "read")]
    public async Task<ActionResult> GetYourEntity(int id)
    {
        try
        {
            var entity = await _manager.GetByIdAsync(User, id);
            if (entity == null)
            {
                return NotFound(new { error = "Entity not found" });
            }
            return Ok(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving your entity {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve entity" });
        }
    }
    
    [HttpPost(APIDictionary.YourEntities)]
    [AccessControlled(EntityTypes.YourEntity, "create")]
    public async Task<ActionResult> CreateYourEntity([FromBody] YourEntityModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var createdEntity = await _manager.CreateAsync(User, model);
            return CreatedAtAction(nameof(GetYourEntity), new { id = createdEntity.Id }, createdEntity);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating your entity");
            return StatusCode(500, new { error = "Failed to create entity" });
        }
    }
    
    [HttpPut(APIDictionary.YourEntity + "/{id}")]
    [AccessControlled(EntityTypes.YourEntity, "update")]
    public async Task<ActionResult> UpdateYourEntity(int id, [FromBody] YourEntityModel model)
    {
        try
        {
            if (id != model.Id)
            {
                return BadRequest(new { error = "ID mismatch" });
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var updatedEntity = await _manager.UpdateAsync(User, model);
            return Ok(updatedEntity);
        }
        catch (NotFoundException)
        {
            return NotFound(new { error = "Entity not found" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating your entity {Id}", id);
            return StatusCode(500, new { error = "Failed to update entity" });
        }
    }
    
    [HttpDelete(APIDictionary.YourEntity + "/{id}")]
    [AccessControlled(EntityTypes.YourEntity, "delete")]
    public async Task<ActionResult> DeleteYourEntity(int id)
    {
        try
        {
            var deleted = await _manager.DeleteAsync(User, id);
            if (!deleted)
            {
                return NotFound(new { error = "Entity not found" });
            }
            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting your entity {Id}", id);
            return StatusCode(500, new { error = "Failed to delete entity" });
        }
    }
    
    [HttpGet(APIDictionary.YourEntitiesByPartner + "/{partnerId}")]
    [AccessControlled(EntityTypes.YourEntity, "read")]
    public async Task<ActionResult> GetYourEntitiesByPartner(int partnerId)
    {
        try
        {
            var entities = await _manager.GetByPartnerIdAsync(User, partnerId);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving your entities for partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "Failed to retrieve entities" });
        }
    }
}
```

**Key Requirements**:
- Inherit from `BaseController`
- Use `[AccessControlled]` attributes for authorization
- Add routes to `APIDictionary` class
- Implement proper error handling and HTTP status codes
- Follow RESTful patterns

### Step 5: Update Supporting Classes (Required)

#### 5.1 Add to API Dictionary

**Location**: `UNOPS.PAO.Presentation/` or similar

```csharp
public static class APIDictionary
{
    // ... existing routes ...
    
    // Your Entity routes
    public const string YourEntities = "api/your-entities";
    public const string YourEntity = "api/your-entities";
    public const string YourEntitiesByPartner = "api/partners";
}
```

#### 5.2 Add to Entity Types

```csharp
public static class EntityTypes
{
    // ... existing types ...
    public const string YourEntity = "YourEntity";
}
```

#### 5.3 Update Manager Wrapper

**Location**: `UNOPS.PAO.UNOPSBusiness/Wrappers/UNOPSManagerWrapper.cs`

```csharp
public class UNOPSManagerWrapper : IManagerWrapper
{
    // ... existing managers ...
    public IYourEntityManager YourEntityManager { get; }
    
    public UNOPSManagerWrapper(
        // ... existing parameters ...
        IYourEntityManager yourEntityManager)
    {
        // ... existing assignments ...
        YourEntityManager = yourEntityManager;
    }
}
```

#### 5.4 Update Database Context

**Location**: `UNOPS.PAO.DataAccess/Context/` or `UNOPS.PAO.UNOPSDataAccess/Context/`

```csharp
public class UNOPSAppDbContext : AppDbContext
{
    // ... existing DbSets ...
    public DbSet<YourEntity> YourEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Your Entity configuration
        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.ToTable("YourEntities");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(255);
                  
            entity.Property(e => e.Description)
                  .HasMaxLength(2000);
                  
            entity.Property(e => e.Status)
                  .IsRequired()
                  .HasMaxLength(50);
            
            // Foreign key relationships
            entity.HasOne(e => e.Partner)
                  .WithMany()
                  .HasForeignKey(e => e.PartnerId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            entity.HasOne(e => e.Contact)
                  .WithMany()
                  .HasForeignKey(e => e.ContactId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Indexes
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.PartnerId);
            entity.HasIndex(e => e.CreatedDate);
        });
    }
}
```

#### 5.5 AutoMapper Configuration

**Location**: `UNOPS.PAO.Business/MappingProfiles/` or similar

```csharp
public class YourEntityMappingProfile : Profile
{
    public YourEntityMappingProfile()
    {
        CreateMap<YourEntity, YourEntityModel>()
            .ForMember(dest => dest.PartnerName, opt => opt.MapFrom(src => src.Partner != null ? src.Partner.Name : null))
            .ForMember(dest => dest.ContactName, opt => opt.MapFrom(src => src.Contact != null ? $"{src.Contact.FirstName} {src.Contact.LastName}".Trim() : null));
            
        CreateMap<YourEntityModel, YourEntity>()
            .ForMember(dest => dest.Partner, opt => opt.Ignore())
            .ForMember(dest => dest.Contact, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedBy, opt => opt.Ignore())
            .ForMember(dest => dest.LastModifiedDate, opt => opt.Ignore());
    }
}
```

#### 5.6 Dependency Injection Registration

**Location**: `Startup.cs` or `Program.cs`

```csharp
// Add to ConfigureServices method
services.AddScoped<IYourEntityManager, YourEntityManager>();
```

## Frontend Implementation Steps

### Step 1: TypeScript Model (Required)

**Location**: `UNOPS.PAO.ClientApp/src/app/features/internal/models/`

**Example**: Create `your-entity.model.ts`

```typescript
export interface YourEntity {
  id: number;
  name: string;
  description?: string;
  status: string;
  startDate: Date;
  endDate?: Date;
  
  // Foreign Keys
  partnerId?: number;
  contactId?: number;
  
  // Related data
  partnerName?: string;
  contactName?: string;
  
  // Audit fields
  createdBy: number;
  createdDate: Date;
  lastModifiedBy?: number;
  lastModifiedDate?: Date;
  
  // Display helpers
  displayName: string;
  statusDisplay: string;
  
  // Permissions
  permissions?: YourEntityPermissions;
}

export interface YourEntityPermissions {
  canRead: boolean;
  canUpdate: boolean;
  canDelete: boolean;
  canEdit: boolean;
  canApprove: boolean;
}

// Create/Update DTOs
export interface CreateYourEntityRequest {
  name: string;
  description?: string;
  status: string;
  startDate: Date;
  endDate?: Date;
  partnerId?: number;
  contactId?: number;
}

export interface UpdateYourEntityRequest extends CreateYourEntityRequest {
  id: number;
}
```

### Step 2: Angular Service (Required)

**Location**: `UNOPS.PAO.ClientApp/src/app/features/internal/services/`

**Example**: Create `your-entity.service.ts`

```typescript
@Injectable({
  providedIn: 'root'
})
export class YourEntityService {
  private readonly baseUrl = '/api/your-entities';

  constructor(private http: HttpClient) {}

  // Get all entities
  getYourEntities(): Observable<YourEntity[]> {
    return this.http.get<YourEntity[]>(this.baseUrl);
  }

  // Get entity by ID
  getYourEntityById(id: number): Observable<YourEntity> {
    return this.http.get<YourEntity>(`${this.baseUrl}/${id}`);
  }

  // Create new entity
  createYourEntity(entity: CreateYourEntityRequest): Observable<YourEntity> {
    return this.http.post<YourEntity>(this.baseUrl, entity);
  }

  // Update existing entity
  updateYourEntity(id: number, entity: UpdateYourEntityRequest): Observable<YourEntity> {
    return this.http.put<YourEntity>(`${this.baseUrl}/${id}`, entity);
  }

  // Delete entity
  deleteYourEntity(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Get entities by partner
  getYourEntitiesByPartnerId(partnerId: number): Observable<YourEntity[]> {
    return this.http.get<YourEntity[]>(`/api/partners/${partnerId}/your-entities`);
  }

  // Helper methods
  getStatusOptions(): { label: string; value: string }[] {
    return [
      { label: 'Draft', value: 'draft' },
      { label: 'Active', value: 'active' },
      { label: 'Completed', value: 'completed' },
      { label: 'Cancelled', value: 'cancelled' }
    ];
  }

  getStatusSeverity(status: string): 'success' | 'warning' | 'danger' | 'info' {
    switch (status?.toLowerCase()) {
      case 'active': return 'success';
      case 'completed': return 'info';
      case 'draft': return 'warning';
      case 'cancelled': return 'danger';
      default: return 'info';
    }
  }
}
```

### Step 3: List Component (Required)

**Location**: `UNOPS.PAO.ClientApp/src/app/features/internal/components/your-entity/your-entity.component.ts`

```typescript
@Component({
  selector: 'app-your-entity',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    TagModule,
    DialogModule,
    TooltipModule,
    ConfirmDialogModule,
    YourEntityDialogComponent
  ],
  templateUrl: './your-entity.component.html',
  styleUrls: ['./your-entity.component.scss']
})
export class YourEntityComponent implements OnInit {
  // Signals for reactive state
  entities = signal<YourEntity[]>([]);
  loading = signal<boolean>(false);
  selectedEntity = signal<YourEntity | null>(null);
  
  // Dialog states
  showCreateDialog = signal<boolean>(false);
  showEditDialog = signal<boolean>(false);
  showViewDialog = signal<boolean>(false);

  // Services
  private yourEntityService = inject(YourEntityService);
  private confirmationService = inject(ConfirmationService);
  private messageService = inject(MessageService);
  private router = inject(Router);
  private permissionService = inject(PermissionUtilityService);

  // Computed permissions
  canCreate = computed(() => this.permissionService.hasPermission('YourEntity', 'create'));
  canEdit = computed(() => this.selectedEntity()?.permissions?.canEdit ?? false);
  canDelete = computed(() => this.selectedEntity()?.permissions?.canDelete ?? false);

  ngOnInit() {
    this.loadEntities();
  }

  async loadEntities() {
    this.loading.set(true);
    try {
      const entities = await this.yourEntityService.getYourEntities().toPromise();
      this.entities.set(entities || []);
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load entities'
      });
      console.error('Error loading entities:', error);
    } finally {
      this.loading.set(false);
    }
  }

  onView(entity: YourEntity) {
    this.router.navigate(['/your-entities', entity.id]);
  }

  onEdit(entity: YourEntity) {
    this.selectedEntity.set(entity);
    this.showEditDialog.set(true);
  }

  onDelete(entity: YourEntity) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${entity.displayName}"?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.deleteEntity(entity.id)
    });
  }

  onCreate() {
    this.selectedEntity.set(null);
    this.showCreateDialog.set(true);
  }

  private async deleteEntity(id: number) {
    try {
      await this.yourEntityService.deleteYourEntity(id).toPromise();
      this.messageService.add({
        severity: 'success',
        summary: 'Success',
        detail: 'Entity deleted successfully'
      });
      await this.loadEntities();
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to delete entity'
      });
      console.error('Error deleting entity:', error);
    }
  }

  async onDialogSave(entity: YourEntity) {
    await this.loadEntities();
    this.showCreateDialog.set(false);
    this.showEditDialog.set(false);
    this.selectedEntity.set(null);
  }

  onDialogCancel() {
    this.showCreateDialog.set(false);
    this.showEditDialog.set(false);
    this.selectedEntity.set(null);
  }

  getStatusSeverity(status: string) {
    return this.yourEntityService.getStatusSeverity(status);
  }
}
```

**Template**: Create `your-entity.component.html`

```html
<div class="your-entity-container">
  <!-- Header -->
  <div class="flex justify-between items-center mb-6">
    <h1 class="text-2xl font-semibold text-surface-900">Your Entities</h1>
    <p-button 
      *ngIf="canCreate()"
      label="Add New Entity"
      icon="pi pi-plus"
      (onClick)="onCreate()">
    </p-button>
  </div>

  <!-- Data Table -->
  <p-table 
    [value]="entities()" 
    [loading]="loading()"
    [paginator]="true"
    [rows]="25"
    [showCurrentPageReport]="true"
    currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
    [globalFilterFields]="['name', 'description', 'status', 'partnerName']"
    #table
    styleClass="p-datatable-sm">
    
    <!-- Table Header -->
    <ng-template pTemplate="header">
      <tr>
        <th pSortableColumn="name">
          Name <p-sortIcon field="name"></p-sortIcon>
        </th>
        <th pSortableColumn="status">
          Status <p-sortIcon field="status"></p-sortIcon>
        </th>
        <th pSortableColumn="partnerName">
          Partner <p-sortIcon field="partnerName"></p-sortIcon>
        </th>
        <th pSortableColumn="startDate">
          Start Date <p-sortIcon field="startDate"></p-sortIcon>
        </th>
        <th pSortableColumn="createdDate">
          Created <p-sortIcon field="createdDate"></p-sortIcon>
        </th>
        <th>Actions</th>
      </tr>
    </ng-template>

    <!-- Table Body -->
    <ng-template pTemplate="body" let-entity>
      <tr>
        <td>
          <div class="flex flex-col">
            <span class="font-medium">{{ entity.displayName }}</span>
            <span class="text-sm text-surface-600" *ngIf="entity.description">
              {{ entity.description | slice:0:100 }}{{ entity.description.length > 100 ? '...' : '' }}
            </span>
          </div>
        </td>
        <td>
          <p-tag 
            [value]="entity.statusDisplay" 
            [severity]="getStatusSeverity(entity.status)">
          </p-tag>
        </td>
        <td>{{ entity.partnerName || '-' }}</td>
        <td>{{ entity.startDate | date:'MMM d, y' }}</td>
        <td>{{ entity.createdDate | date:'MMM d, y' }}</td>
        <td>
          <div class="flex gap-2">
            <p-button
              icon="pi pi-eye"
              [rounded]="true"
              [text]="true"
              size="small"
              severity="secondary"
              (onClick)="onView(entity)"
              [pTooltip]="'View Details'">
            </p-button>
            <p-button
              *ngIf="entity.permissions?.canEdit"
              icon="pi pi-pencil"
              [rounded]="true"
              [text]="true"
              size="small"
              severity="secondary"
              (onClick)="onEdit(entity)"
              [pTooltip]="'Edit'">
            </p-button>
            <p-button
              *ngIf="entity.permissions?.canDelete"
              icon="pi pi-trash"
              [rounded]="true"
              [text]="true"
              size="small"
              severity="danger"
              (onClick)="onDelete(entity)"
              [pTooltip]="'Delete'">
            </p-button>
          </div>
        </td>
      </tr>
    </ng-template>

    <!-- Empty State -->
    <ng-template pTemplate="emptymessage">
      <tr>
        <td colspan="6" class="text-center py-12">
          <div class="flex flex-col items-center gap-4">
            <i class="pi pi-inbox text-6xl text-surface-400"></i>
            <div>
              <p class="text-xl font-medium text-surface-600">No entities found</p>
              <p class="text-surface-500">Get started by creating your first entity</p>
            </div>
            <p-button
              *ngIf="canCreate()"
              label="Add New Entity"
              icon="pi pi-plus"
              (onClick)="onCreate()">
            </p-button>
          </div>
        </td>
      </tr>
    </ng-template>
  </p-table>

  <!-- Create Dialog -->
  <app-your-entity-dialog
    [visible]="showCreateDialog()"
    [entity]="null"
    mode="create"
    (onSave)="onDialogSave($event)"
    (onCancel)="onDialogCancel()">
  </app-your-entity-dialog>

  <!-- Edit Dialog -->
  <app-your-entity-dialog
    [visible]="showEditDialog()"
    [entity]="selectedEntity()"
    mode="edit"
    (onSave)="onDialogSave($event)"
    (onCancel)="onDialogCancel()">
  </app-your-entity-dialog>
</div>
```

### Step 4: View Component (Required)

**Location**: `UNOPS.PAO.ClientApp/src/app/features/internal/components/your-entity/view/your-entity-view.component.ts`

```typescript
@Component({
  selector: 'app-your-entity-view',
  standalone: true,
  imports: [
    CommonModule,
    PanelModule,
    ButtonModule,
    TagModule,
    DividerModule,
    SkeletonModule,
    EnhancedEntityLayoutComponent
  ],
  templateUrl: './your-entity-view.component.html',
  styleUrls: ['./your-entity-view.component.scss']
})
export class YourEntityViewComponent implements OnInit {
  // Signals for reactive state
  entity = signal<YourEntity | null>(null);
  loading = signal<boolean>(false);
  entityId = signal<number>(0);

  // Services
  private yourEntityService = inject(YourEntityService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private messageService = inject(MessageService);

  // Computed properties
  entityPermissions = computed(() => this.entity()?.permissions);

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id && id > 0) {
        this.entityId.set(id);
        this.loadEntity(id);
      }
    });
  }

  private async loadEntity(id: number) {
    this.loading.set(true);
    try {
      const entity = await this.yourEntityService.getYourEntityById(id).toPromise();
      this.entity.set(entity);
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load entity'
      });
      console.error('Error loading entity:', error);
      this.router.navigate(['/your-entities']);
    } finally {
      this.loading.set(false);
    }
  }

  onEdit() {
    this.router.navigate(['/your-entities', this.entityId(), 'edit']);
  }

  onBack() {
    this.router.navigate(['/your-entities']);
  }

  getStatusSeverity(status: string) {
    return this.yourEntityService.getStatusSeverity(status);
  }
}
```

### Step 5: Create/Edit Dialog Component (Required)

**Location**: `UNOPS.PAO.ClientApp/src/app/features/internal/components/your-entity/dialog/your-entity-dialog.component.ts`

```typescript
@Component({
  selector: 'app-your-entity-dialog',
  standalone: true,
  imports: [
    CommonModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    CalendarModule,
    ReactiveFormsModule,
    FormsModule
  ],
  templateUrl: './your-entity-dialog.component.html'
})
export class YourEntityDialogComponent implements OnInit {
  // Inputs
  @Input() visible = false;
  @Input() entity: YourEntity | null = null;
  @Input() mode: 'create' | 'edit' = 'create';

  // Outputs
  @Output() onSave = new EventEmitter<YourEntity>();
  @Output() onCancel = new EventEmitter<void>();

  // Form
  entityForm!: FormGroup;
  saving = signal<boolean>(false);

  // Dropdown options
  statusOptions = [
    { label: 'Draft', value: 'draft' },
    { label: 'Active', value: 'active' },
    { label: 'Completed', value: 'completed' },
    { label: 'Cancelled', value: 'cancelled' }
  ];

  // Services
  private fb = inject(FormBuilder);
  private yourEntityService = inject(YourEntityService);
  private messageService = inject(MessageService);

  ngOnInit() {
    this.initializeForm();
  }

  ngOnChanges() {
    if (this.entity && this.entityForm) {
      this.populateForm();
    }
  }

  private initializeForm() {
    this.entityForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      description: ['', [Validators.maxLength(2000)]],
      status: ['draft', [Validators.required]],
      startDate: [new Date(), [Validators.required]],
      endDate: [null],
      partnerId: [null],
      contactId: [null]
    });

    if (this.entity) {
      this.populateForm();
    }
  }

  private populateForm() {
    if (this.entity) {
      this.entityForm.patchValue({
        name: this.entity.name,
        description: this.entity.description,
        status: this.entity.status,
        startDate: new Date(this.entity.startDate),
        endDate: this.entity.endDate ? new Date(this.entity.endDate) : null,
        partnerId: this.entity.partnerId,
        contactId: this.entity.contactId
      });
    }
  }

  async onSaveClick() {
    if (this.entityForm.invalid) {
      this.markFormGroupTouched(this.entityForm);
      return;
    }

    this.saving.set(true);
    try {
      const formValue = this.entityForm.value;
      
      if (this.mode === 'create') {
        const createRequest: CreateYourEntityRequest = {
          name: formValue.name,
          description: formValue.description,
          status: formValue.status,
          startDate: formValue.startDate,
          endDate: formValue.endDate,
          partnerId: formValue.partnerId,
          contactId: formValue.contactId
        };
        
        const created = await this.yourEntityService.createYourEntity(createRequest).toPromise();
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Entity created successfully'
        });
        this.onSave.emit(created);
      } else {
        const updateRequest: UpdateYourEntityRequest = {
          id: this.entity!.id,
          name: formValue.name,
          description: formValue.description,
          status: formValue.status,
          startDate: formValue.startDate,
          endDate: formValue.endDate,
          partnerId: formValue.partnerId,
          contactId: formValue.contactId
        };
        
        const updated = await this.yourEntityService.updateYourEntity(this.entity!.id, updateRequest).toPromise();
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Entity updated successfully'
        });
        this.onSave.emit(updated);
      }
    } catch (error) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: `Failed to ${this.mode} entity`
      });
      console.error(`Error ${this.mode}ing entity:`, error);
    } finally {
      this.saving.set(false);
    }
  }

  onCancelClick() {
    this.entityForm.reset();
    this.onCancel.emit();
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  get dialogHeader(): string {
    return this.mode === 'create' ? 'Create New Entity' : 'Edit Entity';
  }

  get saveButtonLabel(): string {
    return this.mode === 'create' ? 'Create' : 'Update';
  }
}
```

### Step 6: Routing Configuration (Required)

**Location**: `UNOPS.PAO.ClientApp/src/app/features/internal/internal-routing.module.ts` or standalone routing

```typescript
const routes: Routes = [
  // ... existing routes ...
  
  // Your Entity routes
  {
    path: 'your-entities',
    loadComponent: () => import('./components/your-entity/your-entity.component')
      .then(m => m.YourEntityComponent),
    canActivate: [AuthGuard, PermissionGuard],
    data: { permission: { entity: 'YourEntity', action: 'read' } }
  },
  {
    path: 'your-entities/:id',
    loadComponent: () => import('./components/your-entity/view/your-entity-view.component')
      .then(m => m.YourEntityViewComponent),
    canActivate: [AuthGuard, PermissionGuard],
    data: { permission: { entity: 'YourEntity', action: 'read' } }
  }
];
```

### Step 7: Navigation Menu (Required)

Add to your navigation menu configuration:

```typescript
// In your navigation service or component
const menuItems = [
  // ... existing items ...
  {
    label: 'Your Entities',
    icon: 'pi pi-list',
    routerLink: '/your-entities',
    visible: this.permissionService.hasPermission('YourEntity', 'read')
  }
];
```

## Database Implementation Steps

### Step 1: Entity Framework Migration (Required)

Generate and apply the database migration:

```bash
# Generate migration
dotnet ef migrations add AddYourEntity --context UNOPSAppDbContext

# Review the generated migration file and modify if necessary

# Apply migration to database
dotnet ef database update --context UNOPSAppDbContext
```

### Step 2: Seed Data (Optional)

Create seed data for development/testing:

```csharp
public class YourEntitySeeder
{
    public static void SeedYourEntities(UNOPSAppDbContext context)
    {
        if (!context.YourEntities.Any())
        {
            var entities = new List<YourEntity>
            {
                new YourEntity
                {
                    Name = "Sample Entity 1",
                    Description = "This is a sample entity for testing",
                    Status = "active",
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-30)
                },
                new YourEntity
                {
                    Name = "Sample Entity 2", 
                    Description = "Another sample entity",
                    Status = "draft",
                    StartDate = DateTime.UtcNow.AddDays(10),
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow.AddDays(-15)
                }
            };

            context.YourEntities.AddRange(entities);
            context.SaveChanges();
        }
    }
}
```

## Permission Configuration Steps

### Step 1: Entity Permissions (Required)

Add your entity to the permission system:

```sql
-- Add entity to EntityTypes if using database configuration
INSERT INTO EntityTypes (EntityTypeName, Description) 
VALUES ('YourEntity', 'Your Entity management');

-- Add basic permissions
INSERT INTO EntityPermissions (EntityTypeId, PermissionName, Description)
SELECT et.Id, 'read', 'View your entities'
FROM EntityTypes et WHERE et.EntityTypeName = 'YourEntity'
UNION ALL
SELECT et.Id, 'create', 'Create new your entities'
FROM EntityTypes et WHERE et.EntityTypeName = 'YourEntity'
UNION ALL
SELECT et.Id, 'update', 'Update existing your entities'
FROM EntityTypes et WHERE et.EntityTypeName = 'YourEntity'
UNION ALL
SELECT et.Id, 'delete', 'Delete your entities'
FROM EntityTypes et WHERE et.EntityTypeName = 'YourEntity';
```

### Step 2: Role-Based Permissions (Required)

Assign permissions to roles:

```sql
-- Grant permissions to appropriate roles
-- Replace with your actual role names
INSERT INTO RoleEntityPermissions (RoleId, EntityPermissionId)
SELECT r.Id, ep.Id
FROM Roles r
CROSS JOIN EntityPermissions ep
JOIN EntityTypes et ON ep.EntityTypeId = et.Id
WHERE et.EntityTypeName = 'YourEntity'
  AND r.RoleName IN ('Admin', 'Manager', 'User'); -- Adjust role names as needed
```

## Testing Checklist

### Backend Testing
- [ ] All API endpoints return correct HTTP status codes
- [ ] Permission checking works correctly for all operations
- [ ] Entity creation, update, and deletion work properly
- [ ] Foreign key relationships are correctly established
- [ ] AutoMapper mappings work correctly
- [ ] Manager methods handle permissions and filtering
- [ ] Database constraints are properly enforced

### Frontend Testing
- [ ] List view displays entities correctly
- [ ] Create dialog saves new entities
- [ ] Edit dialog updates existing entities
- [ ] Delete functionality works with confirmation
- [ ] View component displays entity details
- [ ] Permissions are correctly checked and enforced
- [ ] Navigation and routing work properly
- [ ] Form validation displays appropriate messages

### Integration Testing
- [ ] End-to-end CRUD operations work
- [ ] Permission system integration functions correctly
- [ ] Related entity relationships display properly
- [ ] Search and filtering work as expected
- [ ] Error handling provides meaningful messages

## Performance Considerations

### Database Optimization
- Create appropriate indexes on frequently queried columns
- Use `Include()` statements judiciously to avoid N+1 queries
- Consider pagination for large datasets
- Implement soft deletion rather than hard deletion

### Frontend Optimization
- Use Angular signals for reactive state management
- Implement proper loading states
- Use OnPush change detection strategy where appropriate
- Consider virtual scrolling for large lists
- Implement proper error boundaries

## Security Considerations

### Authorization
- Always check permissions in both frontend and backend
- Use `[AccessControlled]` attributes on all API endpoints
- Implement row-level security where needed
- Validate all user inputs

### Data Protection
- Sanitize all user inputs
- Use parameterized queries to prevent SQL injection
- Implement proper CORS policies
- Use HTTPS for all communications

## Deployment Steps

### Development Deployment
1. Run database migrations
2. Seed test data if needed
3. Build and test the application
4. Verify all functionality works

### Production Deployment
1. Review all code changes
2. Run comprehensive tests
3. Apply database migrations in maintenance window
4. Deploy backend changes first
5. Deploy frontend changes
6. Verify production functionality
7. Monitor for errors

## Common Patterns and Best Practices

### Naming Conventions
- Use PascalCase for C# classes, properties, and methods
- Use camelCase for TypeScript interfaces and variables
- Use kebab-case for Angular component selectors and file names
- Use descriptive names that clearly indicate purpose

### Code Organization
- Keep components focused on single responsibilities
- Use services for API communication and business logic
- Implement proper error handling at all layers
- Write comprehensive tests for critical functionality

### Performance Best Practices
- Use appropriate HTTP status codes
- Implement proper caching strategies
- Minimize database round trips
- Use pagination for large datasets
- Implement loading states for better user experience

## Troubleshooting Common Issues

### Backend Issues
- **Permission denied errors**: Check role assignments and permission configuration
- **Database constraint errors**: Verify foreign key relationships and required fields
- **Mapping errors**: Ensure AutoMapper profiles are properly configured
- **Null reference exceptions**: Check for proper null handling in navigation properties

### Frontend Issues
- **Route not found**: Verify routing configuration and guards
- **Permission errors**: Check permission service implementation
- **API call failures**: Verify service URLs and HTTP method configurations
- **Form validation issues**: Ensure form validators are properly configured

### Database Issues
- **Migration failures**: Check for conflicting schema changes
- **Performance issues**: Analyze query execution plans and add indexes
- **Foreign key constraint violations**: Verify related entity existence
- **Transaction deadlocks**: Review concurrent operations and locking strategies

This comprehensive guide provides all the necessary steps to implement a complete entity with full CRUD operations and frontend views following the established patterns in your UNOPS PAO application. Remember to test thoroughly at each step and maintain consistency with existing code patterns and conventions.
