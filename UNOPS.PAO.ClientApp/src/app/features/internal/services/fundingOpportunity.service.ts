import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class FundingOpportunityService {
  http = inject(HttpClient);

  private fundingOpportunityData = signal([]);
  allFundingOpportunities = this.fundingOpportunityData.asReadonly();
  isLoading = signal(false);

  constructor() {}

  getAllFundingOppotunities() {
    this.isLoading.set(true);
    this.http.get('/api/funding-opportunity').subscribe({
      next: (data: any) => {
        this.fundingOpportunityData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      },
    });
  }

  getRecordDetailsById( recordId: string ){

    this.isLoading.set( true );
    return this.http.get('/api/funding-opportunity/'+recordId).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  updateRecordDetailsById( requestJson: any ){

    this.isLoading.set( true );
    return this.http.put('/api/funding-opportunity', requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  getAllProposalsById( recordId: string ){
    this.isLoading.set( true );
    return this.http.get('/api/funding-opportunity/'+recordId+'/proposal').pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  createFundingOpportunity( requestJson: object ){

    this.isLoading.set( true );
    return this.http.post('/api/funding-opportunity', requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

}
