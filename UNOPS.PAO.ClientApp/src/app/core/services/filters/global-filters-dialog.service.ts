import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class GlobalFiltersDialogService {
  private openDialogSubject = new Subject<void>();
  
  // Observable that other components can subscribe to
  openDialog$ = this.openDialogSubject.asObservable();
  
  // Method to trigger dialog opening
  openDialog(): void {
    this.openDialogSubject.next();
  }
}
