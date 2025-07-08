import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { EntityPanelService, EntityPanelState } from '../../services/entity-panel.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-entity-details-panel',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  template: `
    <div class="entity-panel-overlay" *ngIf="panelState.isOpen" (click)="closePanel()">
      <div class="entity-panel" (click)="$event.stopPropagation()">
        <!-- Panel Header -->
        <div class="panel-header">
          <div class="panel-title">
            <h3>{{ panelState.entityType | titlecase }} Details</h3>
            <p class="panel-subtitle">ID: {{ panelState.entityId }}</p>
          </div>
          <button 
            type="button" 
            class="close-button"
            (click)="closePanel()"
            title="Close panel">
            <i class="pi pi-times"></i>
          </button>
        </div>

        <!-- Panel Content -->
        <div class="panel-content">
          <div class="coming-soon-container">
            <div class="coming-soon-icon">
              <i class="pi pi-cog pi-spin"></i>
            </div>
            <h4>Coming Soon</h4>
            <p>{{ panelState.entityType | titlecase }} details panel is under development.</p>
            <div class="entity-info">
              <div class="info-item">
                <strong>Entity Type:</strong> {{ panelState.entityType }}
              </div>
              <div class="info-item">
                <strong>Entity ID:</strong> {{ panelState.entityId }}
              </div>
              <div class="info-item" *ngIf="panelState.entityData">
                <strong>Data Preview:</strong>
                <pre>{{ panelState.entityData | json }}</pre>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .entity-panel-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 1000;
      display: flex;
      justify-content: flex-end;
      backdrop-filter: blur(2px);
    }

    .entity-panel {
      width: 400px;
      max-width: 90vw;
      background: white;
      height: 100vh;
      box-shadow: -2px 0 10px rgba(0, 0, 0, 0.1);
      display: flex;
      flex-direction: column;
      animation: slideInRight 0.3s ease-out;
    }

    @keyframes slideInRight {
      from {
        transform: translateX(100%);
      }
      to {
        transform: translateX(0);
      }
    }

    .panel-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      padding: 1.5rem;
      border-bottom: 1px solid #e5e7eb;
      background: #f9fafb;
    }

    .panel-title h3 {
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
      color: #111827;
    }

    .panel-subtitle {
      margin: 0.25rem 0 0 0;
      font-size: 0.875rem;
      color: #6b7280;
    }

    .close-button {
      background: none;
      border: none;
      font-size: 1.25rem;
      color: #6b7280;
      cursor: pointer;
      padding: 0.5rem;
      border-radius: 0.375rem;
      transition: all 0.2s;
    }

    .close-button:hover {
      background-color: #f3f4f6;
      color: #374151;
    }

    .panel-content {
      flex: 1;
      padding: 1.5rem;
      overflow-y: auto;
    }

    .coming-soon-container {
      text-align: center;
      padding: 2rem 0;
    }

    .coming-soon-icon {
      font-size: 3rem;
      color: #6b7280;
      margin-bottom: 1rem;
    }

    .coming-soon-container h4 {
      margin: 0 0 0.5rem 0;
      font-size: 1.5rem;
      color: #111827;
    }

    .coming-soon-container p {
      margin: 0 0 2rem 0;
      color: #6b7280;
      line-height: 1.6;
    }

    .entity-info {
      background: #f9fafb;
      border-radius: 0.5rem;
      padding: 1rem;
      text-align: left;
    }

    .info-item {
      margin-bottom: 0.75rem;
    }

    .info-item:last-child {
      margin-bottom: 0;
    }

    .info-item strong {
      color: #374151;
      display: block;
      margin-bottom: 0.25rem;
    }

    .info-item pre {
      background: white;
      border: 1px solid #e5e7eb;
      border-radius: 0.375rem;
      padding: 0.5rem;
      font-size: 0.75rem;
      overflow-x: auto;
      max-height: 150px;
      overflow-y: auto;
    }

    /* Dark theme support */
    :host-context(.app-dark) .entity-panel {
      background: #1f2937;
      color: #f9fafb;
    }

    :host-context(.app-dark) .panel-header {
      background: #111827;
      border-bottom-color: #374151;
    }

    :host-context(.app-dark) .panel-title h3 {
      color: #f9fafb;
    }

    :host-context(.app-dark) .panel-subtitle {
      color: #9ca3af;
    }

    :host-context(.app-dark) .close-button {
      color: #9ca3af;
    }

    :host-context(.app-dark) .close-button:hover {
      background-color: #374151;
      color: #f3f4f6;
    }

    :host-context(.app-dark) .coming-soon-container h4 {
      color: #f9fafb;
    }

    :host-context(.app-dark) .coming-soon-container p {
      color: #9ca3af;
    }

    :host-context(.app-dark) .entity-info {
      background: #111827;
    }

    :host-context(.app-dark) .info-item strong {
      color: #f3f4f6;
    }

    :host-context(.app-dark) .info-item pre {
      background: #0f172a;
      border-color: #374151;
      color: #e5e7eb;
    }
  `]
})
export class EntityDetailsPanelComponent implements OnInit, OnDestroy {
  private entityPanelService = inject(EntityPanelService);
  private subscription?: Subscription;
  
  panelState: EntityPanelState = { isOpen: false };

  ngOnInit() {
    this.subscription = this.entityPanelService.panelState$.subscribe(
      state => this.panelState = state
    );
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  closePanel() {
    this.entityPanelService.closePanel();
  }
} 