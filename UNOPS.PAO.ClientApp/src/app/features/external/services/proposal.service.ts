import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProposalService {
  http = inject(HttpClient);

  private proposalData = signal([]);
  applicantProposals = this.proposalData.asReadonly();
  isLoading = signal(false);

  constructor() {}

  getApplicantProposals() {
    this.isLoading.set(true);
    this.http.get(`/api/external/proposal`).subscribe({
      next: (data: any) => {
        this.proposalData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      },
    });
  }


  getRecordDetailsById(recordId: string) {
    this.isLoading.set(true);
    return this.http.get('/api/external/proposal/' + recordId).pipe(
      tap({
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        },
      }),
    );
  }

  updateRecordDetailsById(requestJson: any) {
    this.isLoading.set(true);
    return this.http.put('/api/external/proposal', requestJson).pipe(
      tap({
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        },
      }),
    );
  }

  createRecordByFundingOpportunityId(fundingOpportunityId: any) {
    this.isLoading.set(true);
    return this.http
      .post('/api/external/proposal', {
        fundingOpportunityId: fundingOpportunityId,
        name: '',
        stage: 'Draft',
      })
      .pipe(
        tap({
          next: (event) => {
            this.isLoading.set(false);
          },
          error: (err) => {
            this.isLoading.set(false);
          },
        }),
      );
  }
}
