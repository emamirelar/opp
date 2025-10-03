import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

interface WorflowData{
  "stage": string,
  "displayName": string
}

@Injectable({
  providedIn: 'root'
})
export class WorkflowService {

  http = inject( HttpClient );

  isLoading = signal(false);

  constructor() { }

  getWorkFlowForEntity( entityName: string ){
    this.isLoading.set( true );
    return this.http.get('/api/workflow/'+entityName).pipe(tap(
    {
      next: (data) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  getNextWorkFlowAtionsForARecordById( entityName: string, recordId: string|number ){
    this.isLoading.set( true );
    return this.http.get('/api/workflow/'+entityName+'/'+recordId).pipe(tap(
    {
      next: (data) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  changeWorkflow(requestJson: object) {
    this.isLoading.set( true );

    return this.http.post('/api/workflow', requestJson).pipe(tap(
      {
        next: (data) => {
          this.isLoading.set( false );
        },
        error: (err) => {
          this.isLoading.set( false );
        }
      }));
  }
}
