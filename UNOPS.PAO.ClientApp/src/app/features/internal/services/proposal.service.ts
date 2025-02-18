import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ProposalService {
  http = inject(HttpClient);

  private proposalData = signal([]);
  allProposals = this.proposalData.asReadonly();
  isLoading = signal(false);

  constructor() { }

  getAllProposals() {
    this.isLoading.set(true);
    this.http.get(`/api/proposal`).subscribe({
      next: (data: any) => {
        this.proposalData.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.isLoading.set(false);
      },
    });
  }

  getProposalById(recordId: string) {
    this.isLoading.set(true);
    return this.http.get(`/api/proposal/${recordId}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }
}
