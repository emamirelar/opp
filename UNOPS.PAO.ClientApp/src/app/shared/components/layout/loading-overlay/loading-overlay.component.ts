import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule],
  templateUrl: './loading-overlay.component.html',
  styleUrls: ['./loading-overlay.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
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
