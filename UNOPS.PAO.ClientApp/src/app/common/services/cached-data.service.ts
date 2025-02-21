import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CachedDataService {

  http = inject( HttpClient );

  isLoading = signal(false);

  //Projects
  private allProjectData = signal([]);
  allProjects = this.allProjectData.asReadonly();

  //SDGs
  private allSDGsData = signal([]);
  allSDGs = this.allSDGsData.asReadonly();

  //Countries
  private allCountryData = signal([]);
  allCountries = this.allCountryData.asReadonly();

  //Currencies
  private allCurrencyData = signal([]);
  allCurrencies = this.allCurrencyData.asReadonly();

  //Eligible Entities
  private allEligibleEntitiesData = signal([]);
  allEligibleEntities = this.allEligibleEntitiesData.asReadonly();

  //Eligible Entities
  private allApplicationTypeData = signal([]);
  allApplicationTypes = this.allApplicationTypeData.asReadonly();

  //Selection Methodology
  private allSelectionMethodologyData = signal([]);
  allSelectionMethodologies = this.allSelectionMethodologyData.asReadonly();

  //Selection Methodology
  private allSaluationsData = signal([]);
  allSalutations = this.allSaluationsData.asReadonly();

  private allStatusData = signal([]);
  allStatus = this.allStatusData.asReadonly();

  private allPronounsData = signal([]);
  allPronouns = this.allPronounsData.asReadonly();

  constructor() { 
    this.loadSalutations();
    this.loadStatus();
    this.loadPronouns();
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

  }

  loadProjects(){
    if( ( this.allProjectData() == undefined ) || ( this.allProjectData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get('/api/unops/project').subscribe({
        next: (data: any) => {
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
      this.http.get('/api/values/sdg').subscribe({
        next: (data: any) => {
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
      this.http.get('/api/values/country').subscribe({
        next: (data: any) => {
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
      this.http.get('/api/values/currency').subscribe({
        next: (data: any) => {
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
    let salutations:any = [{
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
    let statuses:any = [{
      id: 'Active',
      name: 'Active'
    }, {
      id: 'Inactive',
      name: 'Inactive'
    }];
    this.allStatusData.set(statuses);
  }

  loadPronouns() {
    let pronouns:any = [{
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

  loadEligibleEntities(){
    if( ( this.allEligibleEntitiesData() == undefined ) || ( this.allEligibleEntitiesData().length <= 0 ) )
    {
      this.isLoading.set( true );
      this.http.get('/api/values/eligible-entity').subscribe({
        next: (data: any) => {
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
      this.http.get('/api/values/application-type').subscribe({
        next: (data: any) => {
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
      this.http.get('/api/values/selection-methodology').subscribe({
        next: (data: any) => {
          this.allSelectionMethodologyData.set( data );
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      });
    }
  }

}