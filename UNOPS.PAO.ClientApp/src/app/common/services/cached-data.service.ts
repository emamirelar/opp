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

  constructor() { }

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
