# CRM Enhancement Implementation Plan

## Project Overview

This document outlines the implementation strategy for enhancing the UNOPS PAO CRM system with:

1. **Related Information Panels**: Add related data display to existing entity views (partners, contacts, interactions)
2. **New Entity Management**: Create views for engagements, projects, UNOPS org units, liaison offices, focal points, categories, groups, countries, geo regions, continents
3. **Responsive Two-Column Layout**: 2/3 primary content, 1/3 related information with expandable capability
4. **Mobile-First Design**: Ensure perfect usability across mobile, tablet, and desktop devices
5. **Reusable Component Architecture**: Configuration-driven approach for scalability

## Current Architecture Analysis

### Technology Stack
- **Backend**: .NET 8 with Clean Architecture (Domain → Business → DataAccess → Presentation)
- **Frontend**: Angular 19 with standalone components, TailwindCSS, PrimeNG
- **Database**: PostgreSQL with Entity Framework Core
- **Security**: Comprehensive RBAC with row-level filtering via `IPermissionService`
- **AI**: Gemini integration for contextual information

### Existing Patterns Identified

#### Backend Patterns
- **Base Manager Pattern**: `BaseUNOPSManager` provides common CRUD functionality
- **Permission Integration**: `IPermissionService` handles entity-level and row-level permissions
- **Repository Pattern**: `BaseRepository<T>` for data access
- **Manager Wrapper**: `UNOPSManagerWrapper` centralizes manager instances

#### Frontend Patterns
- **Entity View Structure**: Consistent across `partner-view.component` and `contact-view.component`
- **Responsive Layout**: Already implements `xl:w-2/3` and `xl:w-1/3` column layout
- **Styling Guide**: Comprehensive patterns in `entity-view-styling-guide.md`
- **AI Panel Integration**: Existing `app-ai-panel` for contextual information

#### Current Layout Implementation
```html
<!-- From partner-view.component.html -->
<div class="flex flex-col gap-8">
  <div class="flex flex-col xl:flex-row gap-8">
    <div class="flex flex-col gap-8" [ngClass]="showAiPanel ? 'xl:w-2/3' : 'w-full'">
      <!-- Main content -->
    </div>
    <div class="xl:w-1/3 flex flex-col gap-8" *ngIf="showAiPanel">
      <!-- AI panels -->
    </div>
  </div>
</div>
```

## Implementation Strategy

### Phase 1: Backend Foundation

#### 1.1 New Domain Entities

Create new entities in `UNOPS.PAO.UNOPSDomain/Entities/Common/`:

```csharp
// Engagement.cs
public class Engagement : BaseBusinessEntity
{
    public string EngagementNumber { get; set; }
    public string Title { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? BudgetAmount { get; set; }
    public string Stage { get; set; }
    public string Description { get; set; }
    
    // Foreign Keys
    public int? PartnerId { get; set; }
    public int? ProjectId { get; set; }
    public int? OrganizationUnitId { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual UNOPSPartner Partner { get; set; }
    [JsonIgnore]
    public virtual Project Project { get; set; }
    [JsonIgnore]
    public virtual OrganizationHierarchy OrganizationUnit { get; set; }
}

// PartnerLiaisonOffice.cs
public class PartnerLiaisonOffice : BaseBusinessEntity
{
    public string OfficeName { get; set; }
    public string OfficeCode { get; set; }
    public string Country { get; set; }
    public string Region { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string Address { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual ICollection<UNOPSPartner> Partners { get; set; }
}

// PartnerFocalPoint.cs
public class PartnerFocalPoint : BaseBusinessEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Department { get; set; }
    public string Role { get; set; }
    
    // Foreign Keys
    public int PartnerId { get; set; }
    public int? UserId { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual UNOPSPartner Partner { get; set; }
    [JsonIgnore]
    public virtual PAOUser User { get; set; }
}

// Country.cs
public class Country : BaseBusinessEntity
{
    public string CountryCode { get; set; } // ISO 3166-1 alpha-2
    public string CountryName { get; set; }
    public string Region { get; set; }
    public string Continent { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual ICollection<UNOPSPartner> Partners { get; set; }
}

// GeoRegion.cs
public class GeoRegion : BaseBusinessEntity
{
    public string RegionCode { get; set; }
    public string RegionName { get; set; }
    public string ContinentId { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual Continent Continent { get; set; }
    [JsonIgnore]
    public virtual ICollection<Country> Countries { get; set; }
}

// Continent.cs
public class Continent : BaseBusinessEntity
{
    public string ContinentCode { get; set; }
    public string ContinentName { get; set; }
    
    // Navigation Properties
    [JsonIgnore]
    public virtual ICollection<GeoRegion> Regions { get; set; }
}
```

#### 1.2 Extend Existing Entities

Update `UNOPS.PAO.UNOPSDomain/Entities/UNOPSPartner.cs`:

```csharp
public class UNOPSPartner : Domain.Entities.Partner
{
    // Add new navigation properties
    [JsonIgnore]
    public virtual ICollection<Engagement> Engagements { get; set; }
    
    [JsonIgnore]
    public virtual ICollection<PartnerFocalPoint> FocalPoints { get; set; }
    
    // Foreign key for liaison office
    public int? LiaisonOfficeId { get; set; }
    [JsonIgnore]
    public virtual PartnerLiaisonOffice LiaisonOffice { get; set; }
    
    // Geographic relationships
    public int? CountryId { get; set; }
    [JsonIgnore]
    public virtual Country Country { get; set; }
}
```

#### 1.3 Create Backend Managers

Following the pattern of `UNOPSPartnerManager.cs`, create:

```
UNOPS.PAO.UNOPSBusiness/Managers/
├── UNOPSEngagementManager.cs
├── UNOPSPartnerLiaisonOfficeManager.cs
├── UNOPSPartnerFocalPointManager.cs
├── UNOPSCountryManager.cs
├── UNOPSGeoRegionManager.cs
└── UNOPSContinentManager.cs
```

**Example: UNOPSEngagementManager.cs**

```csharp
public class UNOPSEngagementManager : BaseUNOPSManager, IUNOPSEngagementManager
{
    private readonly BaseRepository<Engagement> _engagementRepository;
    
    public UNOPSEngagementManager(
        IMapper mapper, 
        UNOPSAppDbContext context, 
        IConfiguration configuration,
        IPermissionService permissionService,
        IHttpContextAccessor httpContextAccessor) 
        : base(mapper, context, configuration, null, "Engagement", permissionService, httpContextAccessor)
    {
        _engagementRepository = new BaseRepository<Engagement>(context, configuration, null);
    }
    
    public async Task<IEnumerable<EngagementModel>> GetEngagementsByPartnerIdAsync(ClaimsPrincipal user, int partnerId)
    {
        var engagements = await _engagementRepository.GetAllAsync(
            e => e.PartnerId == partnerId && !e.IsDeleted,
            include: e => e.Include(x => x.Partner).Include(x => x.Project)
        );
        
        // Apply permission filtering
        var filteredEngagements = await _permissionService.FilterEntitiesAsync(user, engagements, _entityName);
        
        return await MapEntityToModelWithPermissionsAsync(user, filteredEngagements);
    }
    
    // Standard CRUD operations following base pattern...
}
```

#### 1.4 API Controllers

Create controllers following `PartnerController.cs` pattern:

```
UNOPS.PAO.UNOPSPresentation/Controllers/
├── EngagementController.cs
├── PartnerLiaisonOfficeController.cs
├── PartnerFocalPointController.cs
├── CountryController.cs
├── GeoRegionController.cs
└── ContinentController.cs
```

**Example: EngagementController.cs**

```csharp
[Route("/")]
[Authorize(AuthenticationSchemes = "IAP")]
public class EngagementController : BaseController
{
    private readonly IUNOPSEngagementManager _manager;
    
    public EngagementController(
        IManagerWrapper managerWrapper,
        UserResolverService<int> userResolverService,
        IAuthorizationService authorizationService,
        ILogger<EngagementController> logger)
        : base(logger, authorizationService, userResolverService)
    {
        _manager = ((UNOPSManagerWrapper)managerWrapper).EngagementManager;
    }
    
    [HttpGet(APIDictionary.EngagementsByPartner + "/{partnerId}")]
    [AccessControlled(EntityTypes.Engagement, "read")]
    public async Task<ActionResult> GetEngagementsByPartner(int partnerId)
    {
        try
        {
            var engagements = await _manager.GetEngagementsByPartnerIdAsync(User, partnerId);
            return Ok(engagements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving engagements for partner {PartnerId}", partnerId);
            return StatusCode(500, new { error = "Failed to retrieve engagements" });
        }
    }
    
    // Standard CRUD endpoints...
}
```

### Phase 2: Frontend Infrastructure

#### 2.1 Base Entity View Component

Create `src/app/shared/components/base-entity-view/base-entity-view.component.ts`:

```typescript
export abstract class BaseEntityViewComponent<T> implements OnInit, OnDestroy {
  // Dependency injection
  protected router = inject(Router);
  protected activatedRoute = inject(ActivatedRoute);
  protected permissionUtilityService = inject(PermissionUtilityService);
  protected panelLayoutService = inject(PanelLayoutService);
  
  // Abstract properties that must be implemented
  abstract entityType: string;
  abstract entityDisplayName: string;
  
  // Signals for reactive state management
  recordData = signal<T>({} as T);
  recordId = signal<number>(0);
  infoLoading = signal<boolean>(false);
  showRelatedPanel = signal<boolean>(true);
  expandedPanel = signal<string | null>(null);
  
  // Computed properties
  recordPermissions = computed(() => this.getEntityPermissions(this.recordData()));
  relatedInfoConfig = computed(() => this.getRelatedInfoConfig());
  layoutClasses = computed(() => this.getLayoutClasses());
  
  // Lifecycle
  ngOnInit() {
    this.activatedRoute.paramMap.subscribe(params => {
      const id = Number(params.get('id') || params.get('recordId'));
      if (id && id > 0) {
        this.recordId.set(id);
        this.loadEntity(id);
      }
    });
    
    this.panelLayoutService.expandedPanel$.subscribe(
      panel => this.expandedPanel.set(panel)
    );
  }
  
  ngOnDestroy() {
    // Cleanup subscriptions
  }
  
  // Abstract methods
  protected abstract loadEntity(id: number): Promise<void>;
  protected abstract getEntityPermissions(data: T): any;
  protected abstract getRelatedInfoConfig(): RelatedInfoConfig[];
  
  // Common methods
  protected getLayoutClasses(): string {
    const expanded = this.expandedPanel();
    const showRelated = this.showRelatedPanel();
    
    if (expanded) return 'w-full';
    if (showRelated) return 'xl:w-2/3';
    return 'w-full';
  }
  
  toggleRelatedPanel(): void {
    this.showRelatedPanel.update(show => !show);
  }
  
  expandPanel(panelType: string): void {
    this.panelLayoutService.expandPanel(panelType);
  }
  
  closeExpandedPanel(): void {
    this.panelLayoutService.collapsePanel();
  }
}
```

#### 2.2 Related Info Panel Component

Create `src/app/shared/components/related-info-panel/related-info-panel.component.ts`:

```typescript
@Component({
  selector: 'app-related-info-panel',
  standalone: true,
  imports: [CommonModule, PanelModule, ButtonModule, SkeletonModule, ChartModule],
  template: `
    <p-panel [toggleable]="true" [(collapsed)]="isCollapsed">
      <ng-template pTemplate="header">
        <div class="flex justify-between items-center w-full">
          <div class="flex items-center gap-2">
            <i [class]="config.icon + ' text-lg'" [style.color]="config.iconColor"></i>
            <span class="font-semibold">{{ config.title }}</span>
            @if (itemCount() > 0) {
              <p-badge [value]="itemCount().toString()" severity="info"></p-badge>
            }
          </div>
          <div class="flex items-center gap-2">
            @if (config.allowAdd && hasCreatePermission()) {
              <p-button 
                icon="pi pi-plus"
                [rounded]="true"
                [text]="true"
                size="small"
                (onClick)="onAdd()"
                [pTooltip]="'Add ' + config.title">
              </p-button>
            }
            @if (config.expandable && itemCount() > config.previewLimit) {
              <p-button
                icon="pi pi-external-link"
                [rounded]="true"
                [text]="true" 
                size="small"
                (onClick)="onExpand()"
                [pTooltip]="'View all ' + config.title">
              </p-button>
            }
          </div>
        </div>
      </ng-template>
      
      <div class="related-info-content">
        @if (isLoading()) {
          <div class="flex flex-col gap-2">
            <p-skeleton height="3rem" *ngFor="let i of [1,2,3]"></p-skeleton>
          </div>
        } @else if (items().length === 0) {
          <div class="text-center text-muted-color py-6">
            <i [class]="config.emptyIcon + ' text-4xl mb-3'"></i>
            <p>{{ config.emptyMessage }}</p>
            @if (config.allowAdd && hasCreatePermission()) {
              <p-button 
                [label]="'Add ' + config.title"
                icon="pi pi-plus"
                size="small"
                [outlined]="true"
                (onClick)="onAdd()"
                class="mt-3">
              </p-button>
            }
          </div>
        } @else {
          <ng-container [ngSwitch]="config.displayTemplate">
            
            <!-- List Template -->
            <div *ngSwitchCase="'list'" class="flex flex-col gap-2">
              <div 
                *ngFor="let item of previewItems(); trackBy: trackByFn"
                class="flex items-center justify-between p-3 border-round border-1 border-surface-border hover:bg-surface-hover cursor-pointer transition-colors"
                (click)="onItemClick(item)">
                <div class="flex-1">
                  <div class="font-medium">{{ getItemTitle(item) }}</div>
                  <div class="text-sm text-muted-color">{{ getItemSubtitle(item) }}</div>
                </div>
                <div class="flex items-center gap-2">
                  <p-badge 
                    *ngIf="getItemStatus(item)"
                    [value]="getItemStatus(item)"
                    [severity]="getItemStatusSeverity(item)">
                  </p-badge>
                  <i class="pi pi-chevron-right text-muted-color"></i>
                </div>
              </div>
            </div>
            
            <!-- Cards Template -->
            <div *ngSwitchCase="'cards'" class="grid grid-cols-1 md:grid-cols-2 gap-3">
              <div 
                *ngFor="let item of previewItems(); trackBy: trackByFn"
                class="p-3 border-round border-1 border-surface-border hover:bg-surface-hover cursor-pointer transition-all hover:shadow-lg"
                (click)="onItemClick(item)">
                <div class="font-medium mb-2">{{ getItemTitle(item) }}</div>
                <div class="text-sm text-muted-color mb-2">{{ getItemSubtitle(item) }}</div>
                <div class="flex justify-between items-center">
                  <p-badge 
                    *ngIf="getItemStatus(item)"
                    [value]="getItemStatus(item)"
                    [severity]="getItemStatusSeverity(item)">
                  </p-badge>
                  <span class="text-xs text-muted-color">{{ getItemDate(item) | date:'short' }}</span>
                </div>
              </div>
            </div>
            
            <!-- Chart Template -->
            <div *ngSwitchCase="'chart'" class="chart-container">
              <p-chart 
                type="doughnut" 
                [data]="chartData()"
                [options]="chartOptions"
                class="w-full h-48">
              </p-chart>
            </div>
            
            <!-- Table Template -->
            <div *ngSwitchCase="'table'" class="overflow-x-auto">
              <table class="w-full">
                <thead>
                  <tr class="border-bottom-1 border-surface-border">
                    <th *ngFor="let col of config.tableColumns" 
                        class="text-left p-2 text-sm font-medium text-muted-color">
                      {{ col.header }}
                    </th>
                    <th class="w-12"></th>
                  </tr>
                </thead>
                <tbody>
                  <tr 
                    *ngFor="let item of previewItems(); trackBy: trackByFn"
                    class="border-bottom-1 border-surface-border hover:bg-surface-hover cursor-pointer"
                    (click)="onItemClick(item)">
                    <td *ngFor="let col of config.tableColumns" class="p-2 text-sm">
                      {{ getColumnValue(item, col.field) }}
                    </td>
                    <td class="p-2">
                      <i class="pi pi-chevron-right text-muted-color"></i>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </ng-container>
          
          @if (itemCount() > config.previewLimit) {
            <div class="flex justify-center pt-3 border-top-1 border-surface-border">
              <p-button
                [label]="'View all ' + itemCount() + ' ' + config.title.toLowerCase()"
                icon="pi pi-external-link"
                [text]="true"
                size="small"
                (onClick)="onExpand()">
              </p-button>
            </div>
          }
        }
      </div>
    </p-panel>
  `
})
export class RelatedInfoPanelComponent implements OnInit {
  @Input() config!: RelatedInfoConfig;
  @Input() entityId!: number;
  @Input() entityType!: string;
  
  // Outputs
  @Output() onExpand = new EventEmitter<string>();
  @Output() onAdd = new EventEmitter<void>();
  @Output() onItemSelect = new EventEmitter<any>();
  
  // State
  items = signal<any[]>([]);
  isLoading = signal<boolean>(false);
  isCollapsed = signal<boolean>(false);
  itemCount = computed(() => this.items().length);
  previewItems = computed(() => this.items().slice(0, this.config.previewLimit || 5));
  chartData = computed(() => this.generateChartData());
  
  // Services
  private http = inject(HttpClient);
  private permissionService = inject(PermissionUtilityService);
  
  chartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' }
    }
  };
  
  ngOnInit() {
    this.loadData();
  }
  
  private async loadData() {
    if (!this.config.apiEndpoint) return;
    
    this.isLoading.set(true);
    try {
      const url = this.config.apiEndpoint.replace('{id}', this.entityId.toString());
      const response = await this.http.get<any[]>(url).toPromise();
      this.items.set(response || []);
    } catch (error) {
      console.error('Error loading related data:', error);
      this.items.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }
  
  hasCreatePermission(): boolean {
    return this.permissionService.hasPermission(this.config.relatedEntityType, 'create');
  }
  
  onItemClick(item: any): void {
    this.onItemSelect.emit(item);
  }
  
  trackByFn(index: number, item: any): any {
    return item.id || index;
  }
  
  getItemTitle(item: any): string {
    return item[this.config.titleField] || item.name || item.title || 'Untitled';
  }
  
  getItemSubtitle(item: any): string {
    return item[this.config.subtitleField] || item.description || '';
  }
  
  getItemStatus(item: any): string {
    return item[this.config.statusField] || item.status || '';
  }
  
  getItemStatusSeverity(item: any): string {
    const status = this.getItemStatus(item)?.toLowerCase();
    const severityMap: {[key: string]: string} = {
      active: 'success',
      completed: 'success', 
      draft: 'warning',
      pending: 'warning',
      inactive: 'danger',
      cancelled: 'danger',
      ...this.config.statusSeverityMap
    };
    return severityMap[status] || 'info';
  }
  
  getItemDate(item: any): Date {
    const dateField = this.config.dateField || 'createdDate';
    return item[dateField] ? new Date(item[dateField]) : new Date();
  }
  
  getColumnValue(item: any, field: string): string {
    return item[field] || '-';
  }
  
  private generateChartData(): any {
    if (this.config.displayTemplate !== 'chart' || !this.config.chartConfig) {
      return {};
    }
    
    const groupField = this.config.chartConfig.groupBy;
    const data = this.items();
    
    const grouped = data.reduce((acc, item) => {
      const key = item[groupField] || 'Other';
      acc[key] = (acc[key] || 0) + 1;
      return acc;
    }, {} as {[key: string]: number});
    
    return {
      labels: Object.keys(grouped),
      datasets: [{
        data: Object.values(grouped),
        backgroundColor: this.config.chartConfig.colors || [
          '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF', '#FF9F40'
        ]
      }]
    };
  }
}
```

#### 2.3 Configuration System

Create `src/app/shared/models/related-info-config.model.ts`:

```typescript
export interface RelatedInfoConfig {
  type: string;
  title: string;
  relatedEntityType: string;
  apiEndpoint: string;
  icon: string;
  iconColor?: string;
  displayTemplate: 'list' | 'cards' | 'table' | 'chart';
  previewLimit: number;
  expandable: boolean;
  allowAdd: boolean;
  
  // Display field mappings
  titleField: string;
  subtitleField?: string;
  statusField?: string;
  dateField?: string;
  
  // Status mapping
  statusSeverityMap?: {[status: string]: string};
  
  // Table configuration
  tableColumns?: {
    field: string;
    header: string;
    width?: string;
  }[];
  
  // Chart configuration
  chartConfig?: {
    groupBy: string;
    colors?: string[];
  };
  
  // Empty state
  emptyMessage: string;
  emptyIcon: string;
  
  // Routing
  detailRoute?: string;
  addRoute?: string;
}

export interface EntityRelationshipConfig {
  entityType: string;
  relatedPanels: RelatedInfoConfig[];
}

// Configuration constants
export const ENTITY_RELATIONSHIPS: EntityRelationshipConfig[] = [
  {
    entityType: 'partner',
    relatedPanels: [
      {
        type: 'engagements',
        title: 'Engagements',
        relatedEntityType: 'engagement',
        apiEndpoint: '/api/partners/{id}/engagements',
        icon: 'pi pi-briefcase',
        iconColor: '#3B82F6',
        displayTemplate: 'cards',
        previewLimit: 4,
        expandable: true,
        allowAdd: true,
        titleField: 'title',
        subtitleField: 'engagementNumber',
        statusField: 'stage',
        dateField: 'startDate',
        statusSeverityMap: {
          'planning': 'warning',
          'active': 'success',
          'completed': 'info',
          'on-hold': 'warning'
        },
        emptyMessage: 'No engagements found',
        emptyIcon: 'pi pi-briefcase',
        detailRoute: '/partnerships/engagements',
        addRoute: '/partnerships/engagements/new'
      },
      {
        type: 'projects',
        title: 'Projects',
        relatedEntityType: 'project',
        apiEndpoint: '/api/partners/{id}/projects',
        icon: 'pi pi-folder-open',
        iconColor: '#10B981',
        displayTemplate: 'table',
        previewLimit: 5,
        expandable: true,
        allowAdd: true,
        titleField: 'name',
        subtitleField: 'projectNumber',
        statusField: 'stage',
        dateField: 'startDate',
        tableColumns: [
          { field: 'projectNumber', header: 'Project #', width: '120px' },
          { field: 'name', header: 'Name' },
          { field: 'stage', header: 'Stage', width: '100px' },
          { field: 'budgetAmount', header: 'Budget', width: '120px' }
        ],
        emptyMessage: 'No projects found',
        emptyIcon: 'pi pi-folder-open'
      },
      {
        type: 'focalPoints',
        title: 'Focal Points',
        relatedEntityType: 'partnerFocalPoint',
        apiEndpoint: '/api/partners/{id}/focal-points',
        icon: 'pi pi-user',
        iconColor: '#8B5CF6',
        displayTemplate: 'list',
        previewLimit: 3,
        expandable: true,
        allowAdd: true,
        titleField: 'fullName',
        subtitleField: 'role',
        statusField: 'status',
        emptyMessage: 'No focal points assigned',
        emptyIcon: 'pi pi-user'
      },
      {
        type: 'interactionStats',
        title: 'Interaction Overview',
        relatedEntityType: 'interaction',
        apiEndpoint: '/api/partners/{id}/interaction-stats',
        icon: 'pi pi-chart-pie',
        iconColor: '#F59E0B',
        displayTemplate: 'chart',
        previewLimit: 0,
        expandable: true,
        allowAdd: false,
        titleField: 'type',
        chartConfig: {
          groupBy: 'type',
          colors: ['#3B82F6', '#10B981', '#F59E0B', '#EF4444', '#8B5CF6']
        },
        emptyMessage: 'No interaction data available',
        emptyIcon: 'pi pi-chart-pie'
      }
    ]
  },
  {
    entityType: 'contact',
    relatedPanels: [
      {
        type: 'interactions',
        title: 'Recent Interactions',
        relatedEntityType: 'interaction',
        apiEndpoint: '/api/contacts/{id}/interactions',
        icon: 'pi pi-comments',
        iconColor: '#3B82F6',
        displayTemplate: 'list',
        previewLimit: 5,
        expandable: true,
        allowAdd: true,
        titleField: 'subject',
        subtitleField: 'type',
        statusField: 'status',
        dateField: 'date',
        emptyMessage: 'No interactions recorded',
        emptyIcon: 'pi pi-comments'
      },
      {
        type: 'engagements',
        title: 'Involved Engagements',
        relatedEntityType: 'engagement',
        apiEndpoint: '/api/contacts/{id}/engagements',
        icon: 'pi pi-briefcase',
        iconColor: '#10B981',
        displayTemplate: 'cards',
        previewLimit: 3,
        expandable: true,
        allowAdd: false,
        titleField: 'title',
        subtitleField: 'engagementNumber',
        statusField: 'stage',
        emptyMessage: 'Not involved in any engagements',
        emptyIcon: 'pi pi-briefcase'
      }
    ]
  }
];
```

#### 2.4 Panel Layout Service

Create `src/app/shared/services/panel-layout.service.ts`:

```typescript
@Injectable({providedIn: 'root'})
export class PanelLayoutService {
  private expandedPanelSubject = new BehaviorSubject<string | null>(null);
  private layoutModeSubject = new BehaviorSubject<'normal' | 'expanded'>('normal');
  
  expandedPanel$ = this.expandedPanelSubject.asObservable();
  layoutMode$ = this.layoutModeSubject.asObservable();
  
  expandPanel(panelId: string): void {
    this.expandedPanelSubject.next(panelId);
    this.layoutModeSubject.next('expanded');
    
    // Add body class for full-screen mode
    document.body.classList.add('panel-expanded');
  }
  
  collapsePanel(): void {
    this.expandedPanelSubject.next(null);
    this.layoutModeSubject.next('normal');
    
    // Remove body class
    document.body.classList.remove('panel-expanded');
  }
  
  isExpanded(panelId: string): boolean {
    return this.expandedPanelSubject.value === panelId;
  }
  
  getCurrentExpanded(): string | null {
    return this.expandedPanelSubject.value;
  }
}
```

### Phase 3: Enhanced Layout Components

#### 3.1 Enhanced Entity Layout Component

Create `src/app/shared/components/enhanced-entity-layout/enhanced-entity-layout.component.ts`:

```typescript
@Component({
  selector: 'app-enhanced-entity-layout',
  standalone: true,
  imports: [CommonModule, RelatedInfoPanelComponent, ButtonModule, SidebarModule],
  template: `
    <div class="enhanced-entity-layout flex flex-col gap-8" [ngClass]="layoutClasses()">
      
      <!-- Main content area -->
      <div class="flex flex-col xl:flex-row gap-8" 
           [ngClass]="contentWrapperClasses()">
        
        <!-- Primary content -->
        <div class="flex flex-col gap-8" 
             [ngClass]="mainContentClasses()">
          <ng-content select="[slot=main-content]"></ng-content>
        </div>
        
        <!-- Related info sidebar -->
        <div class="xl:w-1/3 flex flex-col gap-4" 
             [ngClass]="sidebarClasses()"
             *ngIf="showSidebar() && !isExpanded()">
          
          <!-- Toggle button for mobile -->
          <div class="xl:hidden mb-4">
            <p-button 
              [label]="'Related Information'"
              icon="pi pi-bars"
              [outlined]="true"
              (onClick)="toggleMobileSidebar()"
              styleClass="w-full">
            </p-button>
          </div>
          
          <!-- Related panels -->
          <app-related-info-panel
            *ngFor="let config of relatedConfigs(); trackBy: trackByConfig"
            [config]="config"
            [entityId]="entityId()"
            [entityType]="entityType()"
            (onExpand)="handlePanelExpand($event)"
            (onAdd)="handleAddItem($event)"
            (onItemSelect)="handleItemSelect($event)"
            class="related-panel">
          </app-related-info-panel>
          
          <!-- Custom related content -->
          <ng-content select="[slot=related-content]"></ng-content>
        </div>
      </div>
      
      <!-- Mobile sidebar -->
      <p-sidebar 
        [(visible)]="showMobileSidebar"
        position="right"
        styleClass="related-info-mobile-sidebar"
        [style]="{ width: '100vw' }">
        
        <ng-template pTemplate="header">
          <span class="text-xl font-semibold">Related Information</span>
        </ng-template>
        
        <div class="flex flex-col gap-4 p-4">
          <app-related-info-panel
            *ngFor="let config of relatedConfigs(); trackBy: trackByConfig"
            [config]="config"
            [entityId]="entityId()" 
            [entityType]="entityType()"
            (onExpand)="handlePanelExpand($event); showMobileSidebar = false"
            (onAdd)="handleAddItem($event)"
            (onItemSelect)="handleItemSelect($event)">
          </app-related-info-panel>
        </div>
      </p-sidebar>
      
      <!-- Expanded panel overlay -->
      <div class="fixed inset-0 bg-surface-ground z-50 overflow-auto"
           *ngIf="expandedPanel()">
        
        <div class="expanded-panel-header sticky top-0 bg-surface-ground border-bottom-1 border-surface-border p-4 z-10">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <p-button
                icon="pi pi-arrow-left"
                [rounded]="true"
                [text]="true"
                (onClick)="closeExpandedPanel()"
                [pTooltip]="'Back to ' + entityDisplayName()">
              </p-button>
              <h2 class="text-2xl font-semibold m-0">{{ getExpandedPanelTitle() }}</h2>
            </div>
            
            <div class="flex items-center gap-2">
              <p-button
                icon="pi pi-external-link"
                [rounded]="true"
                [text]="true"
                (onClick)="navigateToExpandedView()"
                [pTooltip]="'Open in new tab'">
              </p-button>
              <p-button
                icon="pi pi-times"
                [rounded]="true"
                [text]="true"
                severity="secondary"
                (onClick)="closeExpandedPanel()">
              </p-button>
            </div>
          </div>
        </div>
        
        <div class="expanded-panel-content p-6">
          <app-expanded-panel-view
            [panelType]="expandedPanel()!"
            [entityType]="entityType()"
            [entityId]="entityId()"
            [config]="getExpandedPanelConfig()"
            (onClose)="closeExpandedPanel()"
            (onItemSelect)="handleItemSelect($event)">
          </app-expanded-panel-view>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .enhanced-entity-layout {
      min-height: calc(100vh - 200px);
    }
    
    .related-panel {
      @apply transition-all duration-200;
    }
    
    .related-panel:hover {
      @apply transform -translate-y-1 shadow-md;
    }
    
    :host ::ng-deep .related-info-mobile-sidebar {
      .p-sidebar-content {
        padding: 0;
      }
    }
    
    :host ::ng-deep .panel-expanded {
      overflow: hidden;
    }
    
    .expanded-panel-header {
      backdrop-filter: blur(10px);
      background-color: rgba(var(--surface-ground), 0.95);
    }
    
    @media (max-width: 1279px) {
      .related-panel {
        display: none;
      }
    }
  `]
})
export class EnhancedEntityLayoutComponent implements OnInit {
  // Inputs
  @Input() entityType = signal<string>('');
  @Input() entityDisplayName = signal<string>('');
  @Input() entityId = signal<number>(0);
  @Input() showSidebar = signal<boolean>(true);
  @Input() relatedConfigs = signal<RelatedInfoConfig[]>([]);
  
  // Outputs  
  @Output() onPanelExpand = new EventEmitter<string>();
  @Output() onAddItem = new EventEmitter<{type: string, config: RelatedInfoConfig}>();
  @Output() onItemSelect = new EventEmitter<{type: string, item: any}>();
  
  // State
  expandedPanel = signal<string | null>(null);
  showMobileSidebar = false;
  
  // Services
  private panelLayoutService = inject(PanelLayoutService);
  private router = inject(Router);
  private breakpointObserver = inject(BreakpointObserver);
  
  // Computed properties
  isExpanded = computed(() => this.expandedPanel() !== null);
  isMobile = signal<boolean>(false);
  
  layoutClasses = computed(() => {
    return {
      'layout-expanded': this.isExpanded(),
      'layout-mobile': this.isMobile()
    };
  });
  
  contentWrapperClasses = computed(() => {
    const expanded = this.isExpanded();
    return {
      'flex-col': this.isMobile() && !expanded,
      'xl:flex-row': !this.isMobile() && !expanded
    };
  });
  
  mainContentClasses = computed(() => {
    const expanded = this.isExpanded();
    const showSidebar = this.showSidebar();
    
    if (expanded) return 'w-full';
    if (showSidebar && !this.isMobile()) return 'xl:w-2/3';
    return 'w-full';
  });
  
  sidebarClasses = computed(() => ({
    'hidden xl:block': !this.isMobile(),
    'block': this.isMobile()
  }));
  
  ngOnInit() {
    // Subscribe to panel expansion
    this.panelLayoutService.expandedPanel$.subscribe(
      panel => this.expandedPanel.set(panel)
    );
    
    // Monitor breakpoint changes
    this.breakpointObserver.observe(['(max-width: 1279px)'])
      .subscribe(result => {
        this.isMobile.set(result.matches);
        if (result.matches && this.showMobileSidebar) {
          this.showMobileSidebar = false;
        }
      });
  }
  
  handlePanelExpand(panelType: string): void {
    this.panelLayoutService.expandPanel(panelType);
    this.onPanelExpand.emit(panelType);
  }
  
  handleAddItem(event: any): void {
    const config = this.relatedConfigs().find(c => c.type === event);
    if (config) {
      this.onAddItem.emit({ type: event, config });
    }
  }
  
  handleItemSelect(event: any): void {
    this.onItemSelect.emit(event);
  }
  
  closeExpandedPanel(): void {
    this.panelLayoutService.collapsePanel();
  }
  
  toggleMobileSidebar(): void {
    this.showMobileSidebar = !this.showMobileSidebar;
  }
  
  getExpandedPanelTitle(): string {
    const panelType = this.expandedPanel();
    if (!panelType) return '';
    
    const config = this.relatedConfigs().find(c => c.type === panelType);
    return config?.title || panelType;
  }
  
  getExpandedPanelConfig(): RelatedInfoConfig | undefined {
    const panelType = this.expandedPanel();
    if (!panelType) return undefined;
    
    return this.relatedConfigs().find(c => c.type === panelType);
  }
  
  navigateToExpandedView(): void {
    const config = this.getExpandedPanelConfig();
    if (config?.detailRoute) {
      window.open(`${config.detailRoute}?partnerId=${this.entityId()}`, '_blank');
    }
  }
  
  trackByConfig(index: number, config: RelatedInfoConfig): string {
    return config.type;
  }
}
```

### Phase 4: Updated Entity Views

#### 4.1 Updated Partner View Component

Update `src/app/features/internal/components/partner/view/partner-view.component.ts`:

```typescript
export class PartnerViewComponent extends BaseEntityViewComponent<Partner> implements OnInit {
  override entityType = 'partner';
  override entityDisplayName = 'Partner';
  
  // Partner-specific services
  partnerService = inject(PartnerService);
  
  // Configuration
  relatedInfoConfig = computed<RelatedInfoConfig[]>(() => {
    const config = ENTITY_RELATIONSHIPS.find(er => er.entityType === 'partner');
    return config?.relatedPanels || [];
  });
  
  protected async loadEntity(id: number): Promise<void> {
    this.infoLoading.set(true);
    try {
      const partner = await this.partnerService.getPartnerById(id);
      this.recordData.set(partner);
    } catch (error) {
      console.error('Error loading partner:', error);
    } finally {
      this.infoLoading.set(false);
    }
  }
  
  protected getEntityPermissions(data: Partner): any {
    return this.permissionUtilityService.getEntityPermissions('partner', data);
  }
  
  protected getRelatedInfoConfig(): RelatedInfoConfig[] {
    return this.relatedInfoConfig();
  }
  
  // Partner-specific methods
  handleEditClick(): void {
    // Existing edit logic
  }
  
  handleApprovalClick(): void {
    // Existing approval logic  
  }
  
  handleActivateClick(): void {
    // Existing activation logic
  }
  
  // Event handlers for enhanced layout
  onPanelExpand(panelType: string): void {
    console.log(`Expanding panel: ${panelType}`);
  }
  
  onAddItem(event: {type: string, config: RelatedInfoConfig}): void {
    const { type, config } = event;
    if (config.addRoute) {
      this.router.navigate([config.addRoute], {
        queryParams: { partnerId: this.recordId() }
      });
    }
  }
  
  onItemSelect(event: {type: string, item: any}): void {
    const { type, item } = event;
    const config = this.relatedInfoConfig().find(c => c.type === type);
    
    if (config?.detailRoute && item.id) {
      this.router.navigate([config.detailRoute, item.id]);
    }
  }
}
```

Update `src/app/features/internal/components/partner/view/partner-view.component.html`:

```html
<app-enhanced-entity-layout
  [entityType]="entityType"
  [entityDisplayName]="entityDisplayName"
  [entityId]="recordId()"
  [showSidebar]="showRelatedPanel()"
  [relatedConfigs]="relatedInfoConfig()"
  (onPanelExpand)="onPanelExpand($event)"
  (onAddItem)="onAddItem($event)"
  (onItemSelect)="onItemSelect($event)">
  
  <!-- Main content slot -->
  <div slot="main-content">
    <!-- Existing partner information panel -->
    <p-panel class="{{infoLoading() ? 'opacity-50' : ''}} unops-card unops-surface-elevated unops-rounded-lg unops-shadow-lg">
      <ng-template pTemplate="header">
        <div class="flex justify-between items-center w-full">
          <div class="flex items-center gap-2">
            <span class="unops-text-headline-medium">Partner Information</span>
            <app-entity-tags [tags]="recordData().tags"></app-entity-tags>
          </div>
          <div class="flex items-center gap-2">
            <!-- Existing action buttons -->
            @if (recordData().permissions?.canApprove) {
              <p-button [label]="'button.approve' | translate"
                        icon="pi pi-check"
                        [rounded]="true"
                        severity="primary"
                        (onClick)="handleApprovalClick()"></p-button>
            }
            @if (recordData().permissions?.canActivate) {
              <p-button [label]="'button.activate' | translate"
                        icon="pi pi-play"
                        [rounded]="true"
                        severity="primary"
                        (onClick)="handleActivateClick()"></p-button>
            }
            @if (recordData().permissions?.canUpdate) {
              <p-button icon="pi pi-pencil"
                        [rounded]="true"
                        [text]="true"
                        [size]="'small'"
                        class="edit-button partner-edit-button"
                        (onClick)="handleEditClick()"></p-button>
            }
          </div>
        </div>
      </ng-template>
      
      <!-- Existing partner content (keeping all current styling) -->
      <div class="partner-info-content partner-information">
        <!-- All existing partner information display code -->
        <!-- ... keeping exactly as is for compatibility ... -->
      </div>
    </p-panel>
  </div>
  
  <!-- Custom related content slot (for any partner-specific panels) -->
  <div slot="related-content">
    <!-- Any additional custom panels specific to partners -->
  </div>
  
</app-enhanced-entity-layout>

<!-- Existing dialogs and modals -->
<!-- Keep all existing dialog code -->
```

## Implementation Timeline

### Phase 1: Backend Foundation (2-3 weeks)
- Week 1: Create domain entities and relationships
- Week 2: Implement managers and repositories  
- Week 3: Create API controllers and endpoints

### Phase 2: Frontend Infrastructure (2-3 weeks)  
- Week 1: Base entity view component and related info panel
- Week 2: Configuration system and panel layout service
- Week 3: Enhanced entity layout component

### Phase 3: Integration & Testing (1-2 weeks)
- Week 1: Update existing partner and contact views
- Week 2: Testing, bug fixes, performance optimization

### Phase 4: New Entity Views (2-4 weeks)
- Generate views for all new entities using established patterns
- Configure related information for each entity type
- Implement entity-specific customizations

## Key Benefits

1. **Reusability**: Base components work across all entity types
2. **Consistency**: Unified patterns and styling throughout
3. **Mobile-First**: Perfect responsive behavior on all devices
4. **Performance**: Lazy loading and efficient data fetching
5. **Maintainability**: Configuration-driven approach reduces duplication
6. **Scalability**: Easy to add new entities and relationships
7. **Accessibility**: Following UNOPS design system standards

This implementation plan leverages your existing excellent architecture while providing the flexibility and reusability needed for comprehensive entity management.
