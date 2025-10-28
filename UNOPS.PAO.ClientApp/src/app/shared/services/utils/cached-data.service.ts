import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { PartnerCategoryGroup, PartnerGroup } from '@partnerships/partners/models/partner-category-group.model';
import { PartnerTreeService } from '@partnerships/partners/services/partner-tree.service';
import { DropdownOption, UserData } from '../../models/api-responses.model';
import { Contact } from '@partnerships/contacts/models/contact.model';
import { Partner } from '@partnerships/partners/models/partner.model';

@Injectable({
  providedIn: 'root'
})
export class CachedDataService {
  http = inject( HttpClient );
  partnerTreeService = inject(PartnerTreeService);

  isLoading = signal(false);

  //Projects
  private allProjectData = signal<DropdownOption[]>([]);
  allProjects = this.allProjectData.asReadonly();

  //SDGs
  private allSDGsData = signal<DropdownOption[]>([]);
  allSDGs = this.allSDGsData.asReadonly();

  //Countries
  private allCountryData = signal<DropdownOption[]>([]);
  allCountries = this.allCountryData.asReadonly();

  //Currencies
  private allCurrencyData = signal<DropdownOption[]>([]);
  allCurrencies = this.allCurrencyData.asReadonly();

  //Eligible Entities
  private allEligibleEntitiesData = signal<DropdownOption[]>([]);
  allEligibleEntities = this.allEligibleEntitiesData.asReadonly();

  //Eligible Entities
  private allApplicationTypeData = signal<DropdownOption[]>([]);
  allApplicationTypes = this.allApplicationTypeData.asReadonly();

  //Selection Methodology
  private allSelectionMethodologyData = signal<DropdownOption[]>([]);
  allSelectionMethodologies = this.allSelectionMethodologyData.asReadonly();

  //Selection Methodology
  private allSaluationsData = signal<DropdownOption[]>([]);
  allSalutations = this.allSaluationsData.asReadonly();

  private allStatusData = signal<DropdownOption[]>([]);
  allStatus = this.allStatusData.asReadonly();

  private allPartnersData = signal<Partner[]>([]);
  allPartners = this.allPartnersData.asReadonly();

  private allPartnerStatusData = signal<DropdownOption[]>([]);
  allPartnerStatus = this.allPartnerStatusData.asReadonly();

  private allPartnerNewEngagementData = signal<DropdownOption[]>([]);
  allPartnerNewEngagement = this.allPartnerNewEngagementData.asReadonly();

  private allYesNoData = signal<DropdownOption[]>([]);
  allYesNo = this.allYesNoData.asReadonly();

  private allDueDiligenceRequiredData = signal<DropdownOption[]>([]);
  allDueDiligenceRequired = this.allDueDiligenceRequiredData.asReadonly();

  private allDueDiligenceApprovalData = signal<DropdownOption[]>([]);
  allDueDiligenceApproval = this.allDueDiligenceApprovalData.asReadonly();

  private allPartnerLevyAppliesData = signal<DropdownOption[]>([]);
  allPartnerLevyApplies = this.allPartnerLevyAppliesData.asReadonly();

  private allPartnerReasonForLevyNotData = signal<DropdownOption[]>([]);
  allPartnerReasonForLevyNot = this.allPartnerReasonForLevyNotData.asReadonly();

  private allPartnerLevyTreatmentData = signal<DropdownOption[]>([]);
  allPartnerLevyTreatment = this.allPartnerLevyTreatmentData.asReadonly();

  private allPartnerScopesData = signal<DropdownOption[]>([]);
  allPartnerScope = this.allPartnerScopesData.asReadonly();

  private allPronounsData = signal<DropdownOption[]>([]);
  allPronouns = this.allPronounsData.asReadonly();

  private allPartnerLevelTypesData = signal<DropdownOption[]>([]);
  allPartnerLevelTypes = this.allPartnerLevelTypesData.asReadonly();

  private allOrganizationUnitsData = signal<DropdownOption[]>([]);
  allOrganizationUnits = this.allOrganizationUnitsData.asReadonly();

  private allPartnerCategoriesData = signal<DropdownOption[]>([]);
  allPartnerCategories = this.allPartnerCategoriesData.asReadonly();

  private allLiaisonOfficesData = signal<DropdownOption[]>([]);
  allLiaisonOffices = this.allLiaisonOfficesData.asReadonly();

  // Add signal for partner category and group structure
  private partnerCategoryGroupData = signal<PartnerCategoryGroup[]>([]);
  partnerCategoryGroups = this.partnerCategoryGroupData.asReadonly();

  getParterGroupByCategoryCode(categoryCode?: string) : PartnerGroup[] {
    if (categoryCode) {
      return this.partnerCategoryGroups()?.find(group => group.partnerCategoryCode === categoryCode)?.children || [];
    }
    return [];
  }

  getPartnerGroupsForSelect = computed(() => this.partnerCategoryGroups()?.map(category => ({
    name: category.partnerCategoryName,
    value: category.partnerCategoryCode,
    items: category.children.map((group: any) => ({
      name: group.partnerGroupName,
      value: group.partnerGroupId,
      searchText: `${category.partnerCategoryName} ${group.partnerGroupName}`.toLowerCase()
    }))
  })) || []);

  getPartnerCategoriesForSelect = computed(() => this.partnerCategoryGroups()?.map(category => ({
    name: category.partnerCategoryName,
    id: category.partnerCategoryId
  })) || []);

  private allContactsData = signal<Contact[]>([]);
  allContacts = this.allContactsData.asReadonly();

  private allUsersData = signal<UserData[]>([]);
  allUsers  = this.allUsersData.asReadonly();

  private currentUserData = signal<UserData>({});
  currentUser = this.currentUserData.asReadonly();

  constructor() {
    this.loadSalutations();
    this.loadStatus();
    this.loadPronouns();
    this.loadPartnerLevyAppliesData();
    this.loadPartnerLevyTreatmentData();
    this.loadPartnerNewEngagement();
    this.loadPartnerReasonForLevyNotData();
    this.loadPartnerScopeData();
    this.loadPartnerStatus();
    this.loadYesNo();
    this.loadDueDiligenceRequiredData();
    this.loadDueDiligenceApprovalData();
    this.loadPartners();
    this.loadPartnerLevelTypeData();
    this.loadOrganizationUnits();
    this.loadPartnerCategoryGroups(); // Load category and group structure
    this.loadLiaisonOffices();
    this.loadContacts();
    this.loadUsers();
    this.loadCurrentUserData();
  }

  clearCachedData(){
    //clears cache projects
    this.allProjectData.set( [] );
    //clears cache SDG
    this.allSDGsData.set( [] );
    //clears cache projects
    this.allCountryData.set( [] );
    //clears cache projects
    this.allCurrencyData.set( [] );

    //clears cache Eligible Entities
    this.allEligibleEntitiesData.set( [] );
    //clears cache Eligible Entities
    this.allApplicationTypeData.set( [] );

    this.allSaluationsData.set([]);

    this.allPartnerStatusData.set([]);
    this.allPartnerNewEngagementData.set([]);
    this.allYesNoData.set([]);
    this.allDueDiligenceRequiredData.set([]);
    this.allDueDiligenceApprovalData.set([]);
    this.allPartnerLevyAppliesData.set([]);
    this.allPartnerReasonForLevyNotData.set([]);
    this.allPartnerLevyTreatmentData.set([]);
    this.allPartnerScopesData.set([]);
    this.allPartnersData.set([]);
    this.allContactsData.set([]);
    this.allOrganizationUnitsData.set([]);
    this.allPartnerCategoriesData.set([]);
    this.partnerCategoryGroupData.set([]); // Clear category and group structure
    this.allLiaisonOfficesData.set([]);
  }

  loadProjects(){
    if( ( this.allProjectData() == undefined ) || ( this.allProjectData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/unops/project').subscribe({
        next: (data) => {
          this.allProjectData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadSDGs(){
    if( ( this.allSDGsData() == undefined ) || ( this.allSDGsData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/values/sdg').subscribe({
        next: (data) => {
          this.allSDGsData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadCountries(){
    if( ( this.allApplicationTypeData() == undefined ) || ( this.allApplicationTypeData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/values/country').subscribe({
        next: (data) => {
          this.allCountryData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadCurrencies(){
    if( ( this.allApplicationTypeData() == undefined ) || ( this.allApplicationTypeData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/values/currency').subscribe({
        next: (data) => {
          this.allCurrencyData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadSalutations(){
    const salutations: DropdownOption[] = [{
      id: 'Mr.',
      name: 'Mr.'
    }, {
      id: 'Ms.',
      name: 'Ms.'
    }, {
      id: 'Mrs.',
      name: 'Mrs.'
    }, {
      id: 'Dr.',
      name: 'Dr.'
    }, {
      id: 'Prof.',
      name: 'Prof.'
    }];
    this.allSaluationsData.set(salutations);
  }

  loadStatus(){
    const statuses: DropdownOption[] = [{
      id: 'Active',
      name: 'Active'
    }, {
      id: 'Inactive',
      name: 'Inactive'
    }];
    this.allStatusData.set(statuses);
  }

  loadPartnerStatus() {
    const partnerStatuses: DropdownOption[] = [{
      id: 'Active',
      name: 'Active'
    }, {
      id: 'Locked',
      name: 'Locked'
    }, {
      id: 'Inactive',
      name: 'Inactive'
    }];
    this.allPartnerStatusData.set(partnerStatuses);
  }

  loadPartnerNewEngagement() {
    const partnerNewEngagements: DropdownOption[] = [{
      id: 'Allowed',
      name: 'Allowed'
    }, {
      id: 'Not Allowed',
      name: 'Not Allowed'
    }];
    this.allPartnerNewEngagementData.set(partnerNewEngagements);
  }

  loadYesNo() {
    const yesNo: DropdownOption[] = [{
      id: 'Yes',
      name: 'Yes'
    }, {
      id: 'No',
      name: 'No'
    }];
    this.allYesNoData.set(yesNo);
  }

  loadDueDiligenceRequiredData() {
    const dueDiligenceRequired: DropdownOption[] = [{
      id: 'Required',
      name: 'Required'
    }, {
      id: 'NotRequired',
      name: 'Not Required'
    }];
    this.allDueDiligenceRequiredData.set(dueDiligenceRequired);
  }

  loadDueDiligenceApprovalData() {
    const dueDiligenceApproval: DropdownOption[] = [{
      id: 'Approved',
      name: 'Approved'
    }, {
      id: 'NotApproved',
      name: 'Not Approved'
    }];
    this.allDueDiligenceApprovalData.set(dueDiligenceApproval);
  }

  loadPartnerLevyAppliesData() {
    const partnerLevyApplies: DropdownOption[] = [{
      id: 'DoesNotApply',
      name: 'Does Not Apply'
    }, {
      id: 'PotentiallyApplied',
      name: 'Potentially Applied'
    }, {
      id: 'PotentiallyNotApplied',
      name: 'Potentially Not Applied'
    }];
    this.allPartnerLevyAppliesData.set(partnerLevyApplies);
  }

  loadPartnerReasonForLevyNotData() {
    const partnerReasonForLevyNot: DropdownOption[] = [{
      id: '3a) Vertical Fund',
      name: '3a) Vertical Fund'
    }, {
      id: '3d) International Financial Institution',
      name: '3d) International Financial Institution'
    }, {
      id: '3c) Programme Country',
      name: '3c) Programme Country'
    }, {
      id: '4) Pooled Fund',
      name: '4) Pooled Fund'
    }, {
      id: '3b) Funds from UN entity',
      name: '3b) Funds from UN entity'
    }, {
      id: '3a / 4) Vertical Fund / Pooled Fund',
      name: '3a / 4) Vertical Fund / Pooled Fund'
    }, {
      id: '6) Thematic Fund',
      name: '6) Thematic Fund'
    }];
    this.allPartnerReasonForLevyNotData.set(partnerReasonForLevyNot);
  }

  loadPartnerLevyTreatmentData() {
    const partnerLevyTreatment: DropdownOption[] = [{
      id: 'Please consult funding source',
      name: 'Please consult funding source'
    }, {
      id: 'UNOPS administers',
      name: 'UNOPS administers'
    }, {
      id: 'Funding source administers directly (no changes required to the partner agreement)',
      name: 'Funding source administers directly (no changes required to the partner agreement)'
    }, {
      id: 'N/A',
      name: 'N/A'
    }];
    this.allPartnerLevyTreatmentData.set(partnerLevyTreatment);
  }

  loadPartnerScopeData() {
    const partnerScopes: DropdownOption[] = [{
      id: 'Global',
      name: 'Global'
    }, {
      id: 'Regional',
      name: 'Regional'
    }, {
      id: 'Local',
      name: 'Local'
    }];
    this.allPartnerScopesData.set(partnerScopes);
  }

  loadPronouns() {
    const pronouns: DropdownOption[] = [{
      id: 'He/Him',
      name: 'He/Him'
    }, {
      id: 'She/Her',
      name: 'She/Her'
    }, {
      id: 'They/Them',
      name: 'They/Them'
    }, {
      id: 'He/They',
      name: 'He/They'
    }, {
      id: 'She/They',
      name: 'She/They'
    }, {
      id: 'Not Listed',
      name: 'Not Listed'
    }];
    this.allPronounsData.set(pronouns);
  }

  loadPartnerLevelTypeData() {
    const partnerLevelTypes: DropdownOption[] = [
      {
        id: 'Level_1',
        name: 'Level 1'
      },
      {
        id: 'Level_2',
        name: 'Level 2'
      },
      {
        id: 'Level_3',
        name: 'Level 3'
      },
      {
        id: 'Level_4',
        name: 'Level 4'
      }
    ];
    this.allPartnerLevelTypesData.set(partnerLevelTypes);
  }

  loadEligibleEntities(){
    if( ( this.allEligibleEntitiesData() == undefined ) || ( this.allEligibleEntitiesData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/values/eligible-entity').subscribe({
        next: (data) => {
          this.allEligibleEntitiesData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadApplicationTypes(){
    if( ( this.allApplicationTypeData() == undefined ) || ( this.allApplicationTypeData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/values/application-type').subscribe({
        next: (data) => {
          this.allApplicationTypeData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadSelectionMethodologies(){
    if( ( this.allApplicationTypeData() == undefined ) || ( this.allApplicationTypeData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get<DropdownOption[]>('/api/values/selection-methodology').subscribe({
        next: (data) => {
          this.allSelectionMethodologyData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

  loadPartners(){
    // Initialize with empty array
    if (this.allPartnersData() === undefined || this.allPartnersData().length <= 0) {
      // Default to empty array before API response
      this.allPartnersData.set([]);

      this.isLoading.set(true);
      this.http.get<Partner[]>('/api/values/partners').subscribe({
        next: (data) => {
          // Ensure data is an array
          this.allPartnersData.set(Array.isArray(data) ? data : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          // Keep empty array on error
          this.allPartnersData.set([]);
          this.isLoading.set(false);
        }
      });
    }
  }

  /**
   * Forces a refresh of the partners cache by clearing current data and reloading
   */
  refreshPartners(){
    // Clear current cache
    this.allPartnersData.set([]);
    // Reload from API
    this.loadPartners();
  }

  /**
   * Forces a refresh of the contacts cache by clearing current data and reloading
   */
  refreshContacts(){
    // Clear current cache
    this.allContactsData.set([]);
    // Reload from API
    this.loadContacts();
  }

  loadOrganizationUnits() {
    if ((this.allOrganizationUnitsData() == undefined) || (this.allOrganizationUnitsData().length <= 0)) {
      this.isLoading.set(true);
      this.http.get<DropdownOption[]>('/api/values/organization-units').subscribe({
        next: (data) => {
          this.allOrganizationUnitsData.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      });
    }
  }

  loadPartnerCategoryGroups() {
    if ((this.partnerCategoryGroupData() == undefined) || (this.partnerCategoryGroupData().length <= 0)) {
      this.isLoading.set(true);
      this.partnerTreeService.getCategoryAndGroupStructure().subscribe({
        next: (data: any) => {
          this.partnerCategoryGroupData.set(data);
          this.isLoading.set(false);
        },
        error: (err: any) => {
          console.error('Error fetching partner category and group structure:', err);
          this.isLoading.set(false);
        }
      });
    }
  }

  loadContacts() {
    // Initialize with empty array
    if (this.allContactsData() === undefined || this.allContactsData().length <= 0) {
      // Default to empty array before API response
      this.allContactsData.set([]);

      this.isLoading.set(true);
      this.http.get<Contact[]>('/api/values/contacts').subscribe({
        next: (data) => {
          // Ensure data is an array
          this.allContactsData.set(Array.isArray(data) ? data : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          // Keep empty array on error
          this.allContactsData.set([]);
          this.isLoading.set(false);
        }
      });
    }
  }

  loadUsers() {
    // OPTIMIZED: Load only initial subset of users instead of all 13,000+
    // This prevents UI freezing when there are many users
    if (this.allUsersData() === undefined || this.allUsersData().length <= 0) {
      // Default to empty array before API response
      this.allUsersData.set([]);

      this.isLoading.set(true);

      // Use the new paginated endpoint to load only the first 100 users
      const initialRequest = {
        pageIndex: 0,
        pageSize: 100,
        activeOnly: true
      };

      this.http.post<{ records: UserData[] }>('/api/values/users/paged', initialRequest).subscribe({
        next: (response) => {
          // Set only the records from the paginated response
          this.allUsersData.set(response.records || []);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.warn('Failed to load initial users, falling back to search-only mode:', err);
          // Keep empty array on error - components should use UserSearchService for dynamic loading
          this.allUsersData.set([]);
          this.isLoading.set(false);
        }
      });
    }
  }

  loadCurrentUserData() {
    // Initialize with empty array
    if (this.currentUserData()?.id === undefined) {
      // Default to empty array before API response
      //this.currentUserData.set([]);

      this.isLoading.set(true);
      this.currentUserData.set({});
      this.isLoading.set(false);
      /*this.http.get('/api/current-user-data').subscribe({
        next: (data: any) => {
          this.currentUserData.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          // Keep empty array on error
          //this.allUsersData.set(new Object);
          this.isLoading.set(false);
        }
      });*/
    }
  }

  loadLiaisonOffices() {
    if ((this.allLiaisonOfficesData() == undefined) || (this.allLiaisonOfficesData().length <= 0)) {
      // Default to empty array before API response
      this.allLiaisonOfficesData.set([]);
      this.isLoading.set(true);
      this.http.get<DropdownOption[]>('/api/values/liaison-offices').subscribe({
        next: (data) => {
          this.allLiaisonOfficesData.set(Array.isArray(data) ? data : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.allLiaisonOfficesData.set([]);
          this.isLoading.set(false);
        }
      });
    }
  }

}
