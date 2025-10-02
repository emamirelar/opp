import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div 
      *ngIf="loading$ | async" 
      class="fixed top-0 left-0 w-full h-full flex flex-col items-center justify-center bg-black bg-opacity-50 z-[9999]"
    >
      <div class="bg-white p-6 rounded-lg shadow-lg flex flex-col items-center">
        <p-progressSpinner [style]="{width: '50px', height: '50px'}"></p-progressSpinner>
        <span class="mt-3 text-lg font-medium">{{ message }}</span>
      </div>
    </div>
  `,
  styles: ``
})
export class LoadingOverlayComponent {
  private loadingState = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingState.asObservable();
  message: string = 'Loading...';

  show(message: string = 'Loading...') {
    this.message = message;
    this.loadingState.next(true);
  }

  hide() {
    this.loadingState.next(false);
  }
}

// Create a singleton service to access the loading overlay from anywhere
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LoadingOverlayService {
  private component: LoadingOverlayComponent | null = null;

  registerComponent(component: LoadingOverlayComponent) {
    this.component = component;
  }

  show(message: string = 'Loading...') {
    if (this.component) {
      this.component.show(message);
    } else {
      console.warn('LoadingOverlay component not registered');
    }
  }

  hide() {
    if (this.component) {
      this.component.hide();
    }
  }
} 