import { Component, inject, signal, OnInit, computed, ViewContainerRef, effect, ChangeDetectorRef, OnDestroy, Type, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { AiAssistantPanelComponent } from '../../common/reusables/widgets/ai-assistant/ai-assistant-panel.component';
import { AiAssistantData } from '../../common/reusables/widgets/ai-assistant/ai-assistant.data';

import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { PartnerViewComponent } from '../internal/components/partner/view/partner-view.component';
import { ContactViewComponent } from '../internal/components/contact/view/contact-view.component';
import { InteractionModalComponent } from '../internal/components/interaction/modal/interaction-modal.component';

import { TranslateModule } from '@ngx-translate/core';

/**
 * @uiEntity AiAssistant
 * @route /ai
 * @description AI Assistant content component that renders within the main application layout. Provides AI chat functionality with entity preview and context-aware assistance.
 * @capabilities ai_chat, session_management, context_awareness, entity_preview, smart_assistance
 * @synonyms ai_chat, virtual_assistant, smart_help, ai_support, intelligent_assistant
 * @mandatoryFields None
 * @help_when_stuck Start by typing your question in the chat input. The AI can help you navigate the app, understand features, find data, or perform tasks. Use the chat history button in the topbar to access previous conversations.
 * @common_tasks
 *   - Getting help: Type questions about how to use features or find information
 *   - Entity assistance: Ask about partners, contacts, or interactions for contextual help
 *   - Navigation help: Get guidance on where to find specific features or data
 *   - Task automation: Request help with complex workflows or data entry
 */

@Component({
  selector: 'app-ai-content',
  standalone: true,
  imports: [
    CommonModule,
    HttpClientModule,
    AiAssistantPanelComponent,
    DialogModule,
    ConfirmDialogModule,
    ButtonModule,
    TooltipModule,
    PartnerViewComponent,
    ContactViewComponent,
    InteractionModalComponent,
    TranslateModule
  ],
  template: `
    <div class="ai-content-wrapper h-full w-full flex">
      <!-- Main AI Assistant Area -->
      <div class="ai-main-area flex-1 flex flex-col" 
           [class.with-right-panel]="rightPanelVisible"
           [style.margin-right]="rightPanelVisible ? rightPanelWidth + 'px' : '0px'">
        
        <app-ai-assistant-panel 
          [hideHeader]="false"
          [viewContainerRef]="viewContainerRef"
          [rightPanelEntityType]="rightPanelEntityType"
          [rightPanelEntityId]="rightPanelEntityId"
          mode="fullscreen"
          (cardClicked)="onCardClicked($event)"
          (urlClicked)="onUrlClicked($event)">
        </app-ai-assistant-panel>
      </div>

      <!-- Right Panel (Entity Details) -->
      <div class="ai-right-panel" 
           [class.panel-visible]="rightPanelVisible" 
           [class.resizing]="resizing"
           [style.flex-basis]="rightPanelVisible ? rightPanelWidth + 'px' : '0px'"
           [style.min-width]="rightPanelVisible ? '500px' : '0px'"
           [style.max-width]="rightPanelVisible ? '60vw' : '0px'"
           [style.display]="rightPanelVisible ? 'flex' : 'none'">
        
        <div class="right-panel-content">
          <div class="right-panel-header" *ngIf="rightPanelVisible">
            <div class="panel-title">
              <h4>{{ getEntityDisplayName() }}</h4>
              <span class="panel-subtitle">{{ rightPanelEntityType | titlecase }} • ID: {{ rightPanelEntityId }}</span>
            </div>
            <div class="panel-actions">
              <button type="button" 
                      class="panel-action-btn" 
                      (click)="openRightPanelInNewTab()" 
                      pTooltip="Open in new tab"
                      tooltipPosition="left">
                <i class="pi pi-external-link"></i>
              </button>
              <button type="button" 
                      class="panel-action-btn" 
                      (click)="closeRightPanel()" 
                      pTooltip="Close panel"
                      tooltipPosition="left">
                <i class="pi pi-times"></i>
              </button>
            </div>
          </div>
          
          <ng-container *ngIf="rightPanelType === 'component' && rightPanelComponent">
            <ng-container [ngSwitch]="rightPanelEntityType">
              <app-partner-view *ngSwitchCase="'Partner'" [recordId]="rightPanelEntityId || ''" [showAiPanel]="false"></app-partner-view>
              <app-contact-view *ngSwitchCase="'Contact'" [recordId]="rightPanelEntityId || ''" [showAiPanel]="false"></app-contact-view>
              <app-interaction-modal *ngSwitchCase="'Interaction'" [recordId]="rightPanelEntityId || ''"></app-interaction-modal>
            </ng-container>
          </ng-container>
          
          <ng-container *ngIf="rightPanelType === 'component' && !rightPanelComponent">
            <!-- <div class="coming-soon-container">
              <div class="coming-soon-icon">
                <i class="pi pi-cog pi-spin"></i>
              </div>
              <h4>Coming Soon</h4>
              <p>{{ rightPanelEntityType | titlecase }} details panel is under development.</p>
              <div class="entity-info">
                <div class="info-item">
                  <strong>Entity Type:</strong> {{ rightPanelEntityType }}
                </div>
                <div class="info-item">
                  <strong>Entity ID:</strong> {{ rightPanelEntityId }}
                </div>
              </div>
            </div> -->
            <!-- TAD: Defaulting to partner for now -->
            <app-partner-view [recordId]="rightPanelEntityId || ''" [showAiPanel]="false"></app-partner-view>
          </ng-container>
          
          <ng-container *ngIf="rightPanelType === 'url'">
            <iframe [src]="rightPanelUrl" width="100%" height="100%" frameborder="0"></iframe>
          </ng-container>
        </div>
        
        <div class="resize-handle" (mousedown)="startResizing($event)"></div>
      </div>
    </div>

    <p-confirmDialog></p-confirmDialog>
  `,
  styles: [`
    .ai-content-wrapper {
      position: relative;
      background: var(--surface-ground);
      height: 100%;
    }

    .ai-main-area {
      transition: margin-right 0.3s ease;
    }

    .ai-right-panel {
      position: fixed;
      top: 105px; /* Account for topbar height */
      right: 0;
      bottom: 0;
      flex-direction: column;
      background: var(--surface-card);
      border-left: 1px solid var(--surface-border);
      transition: all 0.3s ease;
      z-index: 1000;
    }

    .ai-right-panel.panel-visible {
      transform: translateX(0);
    }

    .ai-right-panel:not(.panel-visible) {
      transform: translateX(100%);
    }

    .right-panel-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }

    .right-panel-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      border-bottom: 1px solid var(--surface-border);
      background: var(--surface-section);
    }

    .panel-title h4 {
      margin: 0;
      font-size: 1.2rem;
      font-weight: 600;
      color: var(--text-color);
    }

    .panel-subtitle {
      font-size: 0.875rem;
      color: var(--text-color-secondary);
      margin-top: 0.25rem;
    }

    .panel-actions {
      display: flex;
      gap: 0.5rem;
    }

    .panel-action-btn {
      background: transparent;
      border: 1px solid var(--surface-border);
      border-radius: var(--border-radius);
      padding: 0.5rem;
      cursor: pointer;
      color: var(--text-color);
      transition: all 0.2s;
    }

    .panel-action-btn:hover {
      background: var(--surface-hover);
      border-color: var(--primary-color);
    }

    .resize-handle {
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 4px;
      background: var(--surface-border);
      cursor: ew-resize;
      transition: background 0.2s;
    }

    .resize-handle:hover,
    .ai-right-panel.resizing .resize-handle {
      background: var(--primary-color);
    }

    .coming-soon-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100%;
      padding: 2rem;
      text-align: center;
    }

    .coming-soon-icon {
      font-size: 3rem;
      color: var(--primary-color);
      margin-bottom: 1rem;
    }

    .entity-info {
      margin-top: 1rem;
      text-align: left;
    }

    .info-item {
      margin-bottom: 0.5rem;
      padding: 0.5rem;
      background: var(--surface-section);
      border-radius: var(--border-radius);
    }

    :host ::ng-deep .ai-main-area app-ai-assistant-panel {
      height: 100%;
      width: 100%;
      display: flex;
      flex-direction: column;
    }
    
    /* Ensure the parent container uses full height */
    :host {
      display: block;
      height: 100%;
    }
  `]
})
export class AiContentComponent implements OnInit, OnDestroy {
  router = inject(Router);
  route = inject(ActivatedRoute);
  http = inject(HttpClient);
  viewContainerRef = inject(ViewContainerRef);
  aiAssistantData = inject(AiAssistantData);
  confirmationService = inject(ConfirmationService);
  private cdr = inject(ChangeDetectorRef);
  private injector = inject(Injector);

  // Right panel state
  rightPanelVisible = false;
  rightPanelWidth = 600;
  rightPanelType: 'component' | 'url' | null = null;
  rightPanelComponent: Type<any> | null = null;
  rightPanelUrl: string | null = null;
  resizing = false;
  private startX = 0;
  private startWidth = 600;
  private document = window.document;

  private entityComponentMap: Record<string, any> = {
    partner: PartnerViewComponent,
    contact: ContactViewComponent,
    interaction: InteractionModalComponent
  };
  
  rightPanelEntityType: string | null = null;
  rightPanelEntityId: string | null = null;
  rightPanelRowData: any = null;

  // Listen for current session changes to update URL - must be in injection context
  private sessionUrlEffect = effect(() => {
    const currentSessionId = this.aiAssistantData.currentSessionId();
    const currentRoute = this.router.url;
    
    // Only update URL if we're on an AI route
    if (currentRoute.startsWith('/ai')) {
      if (currentSessionId) {
        // Navigate to session-specific URL
        if (currentRoute !== `/ai/${currentSessionId}`) {
          this.router.navigate(['/ai', currentSessionId], { replaceUrl: true });
        }
      } else {
        // Navigate to general AI URL when no session
        if (currentRoute !== '/ai') {
          this.router.navigate(['/ai'], { replaceUrl: true });
        }
      }
    }
  });

  ngOnInit() {
    // Set the ViewContainerRef for the AI assistant data service
    this.aiAssistantData.setViewContainerRef(this.viewContainerRef);
    
    // Handle sessionId from route parameter
    this.route.params.subscribe(params => {
      const sessionId = params['sessionId'];
      if (sessionId) {
        // Load the specific session
        this.aiAssistantData.switchToSession(sessionId).subscribe({
          error: (error) => {
            console.error('Failed to load session from URL:', error);
            // Redirect to /ai without sessionId if session doesn't exist
            this.router.navigate(['/ai'], { replaceUrl: true });
          }
        });
      }
    });
  }

  ngOnDestroy() {
    // Clean up any subscriptions if needed
    this.rightPanelVisible = false;
    this.rightPanelType = null;
    this.rightPanelComponent = null;
    this.rightPanelUrl = null;
    this.rightPanelEntityType = null;
    this.rightPanelEntityId = null;
    this.rightPanelRowData = null;
  }

  // Right panel methods
  closeRightPanel() {
    this.rightPanelVisible = false;
    this.cdr.detectChanges();
    
    setTimeout(() => {
      this.rightPanelComponent = null;
      this.rightPanelUrl = null;
      this.rightPanelType = null;
      this.rightPanelEntityType = null;
      this.rightPanelEntityId = null;
      this.rightPanelRowData = null;
    }, 300);
  }

  startResizing(event: MouseEvent) {
    this.resizing = true;
    this.startX = event.clientX;
    this.startWidth = this.rightPanelWidth;
    this.document.addEventListener('mousemove', this.onResizing);
    this.document.addEventListener('mouseup', this.stopResizing);
    this.document.body.classList.add('resizing');
    event.preventDefault();
  }

  onResizing = (event: MouseEvent) => {
    if (!this.resizing) return;
    const dx = event.clientX - this.startX;
    let newWidth = this.startWidth - dx;
    newWidth = Math.max(500, Math.min(newWidth, window.innerWidth * 0.6));
    this.rightPanelWidth = newWidth;
    this.cdr.detectChanges();
  };

  stopResizing = () => {
    this.resizing = false;
    this.document.removeEventListener('mousemove', this.onResizing);
    this.document.removeEventListener('mouseup', this.stopResizing);
    this.document.body.classList.remove('resizing');
  };

  onCardClicked(event: { entityType: string, entityId: string, rowData: any }) {
    
    const isDifferentEntity = this.rightPanelEntityType !== event.entityType || 
                             this.rightPanelEntityId !== event.entityId;

    if (isDifferentEntity) {
      
      this.rightPanelVisible = false;
      this.cdr.detectChanges();
      
      setTimeout(() => {
        this.loadEntityInPanel(event);
      }, 0);
    } else {
    }
  }

  private loadEntityInPanel(event: { entityType: string, entityId: string, rowData: any }) {
    
    this.rightPanelEntityType = event.entityType;
    this.rightPanelEntityId = event.entityId;
    this.rightPanelRowData = event.rowData;
    
    const componentKey = event.entityType.toLowerCase();
    const component = this.entityComponentMap[componentKey];
    
    if (component) {
      this.rightPanelType = 'component';
      this.rightPanelComponent = component;
      this.rightPanelVisible = true;
      this.cdr.detectChanges();
    } else {
      this.rightPanelType = 'component';
      this.rightPanelComponent = null;
      this.rightPanelVisible = true;
      this.cdr.detectChanges();
    }
  }

  onUrlClicked(url: string | Event) {
    if (typeof url === 'string') {
      this.rightPanelType = 'url';
      this.rightPanelUrl = url;
      this.rightPanelVisible = true;
    }
  }

  openRightPanelInNewTab() {
    if (this.rightPanelEntityType && this.rightPanelEntityId) {
      const route = this.buildEntityRoute(this.rightPanelEntityType, this.rightPanelEntityId, this.rightPanelRowData);
      if (route) {
        const fullUrl = `${window.location.origin}/#${route}`;
        window.open(fullUrl, '_blank');
      }
    } else if (this.rightPanelUrl) {
      window.open(this.rightPanelUrl, '_blank');
    }
  }

  private buildEntityRoute(entityType: string, entityId: number | string, rowData: any): string | null {
    switch (entityType?.toLowerCase()) {
      case 'partner':
        return `/partnerships/partners/${entityId}`;
      case 'contact':
        if (rowData?.partnerId) {
          return `/partnerships/partners/${rowData.partnerId}/contacts/${entityId}`;
        }
        return `/contacts/${entityId}`;
      case 'interaction':
        return `/interactions/${entityId}`;
      case 'partneragreement':
      case 'partnership':
        return `/partnerships/agreements/${entityId}`;
      default:
        const routeSegment = entityType.toLowerCase().replace(/\s+/g, '-');
        return `/${routeSegment}s/${entityId}`;
    }
  }

  getEntityDisplayName(): string {
    if (!this.rightPanelEntityType || !this.rightPanelRowData) {
      return 'Entity Details';
    }

    const data = this.rightPanelRowData;
    const nameFields = ['name', 'title', 'displayName', 'fullName', 'firstName', 'lastName'];
    
    for (const field of nameFields) {
      if (data[field] && typeof data[field] === 'string') {
        return data[field];
      }
    }
    
    if (data.firstName && data.lastName) {
      return `${data.firstName} ${data.lastName}`;
    }
    
    return this.rightPanelEntityType;
  }
}

