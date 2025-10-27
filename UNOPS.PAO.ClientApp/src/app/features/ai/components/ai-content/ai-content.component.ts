import { Component, inject, signal, OnInit, computed, ViewContainerRef, effect, ChangeDetectorRef, OnDestroy, Type, Injector } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { AiAssistantPanelComponent } from '@features/ai/widgets/ai-assistant/ai-assistant-panel.component';
import { AiAssistantService } from '@ai/services/ai-assistant.service';

import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { PartnerViewComponent } from '@partnerships/partners/components/partner/view/partner-view.component';
import { ContactViewComponent } from '@partnerships/contacts/components/contact/view/contact-view.component';
import { InteractionModalComponent } from '@partnerships/interactions/components/interaction/modal/interaction-modal.component';

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
  templateUrl: './ai-content.component.html',
  styleUrl: './ai-content.component.scss'
})
export class AiContentComponent implements OnInit, OnDestroy {
  router = inject(Router);
  route = inject(ActivatedRoute);
  http = inject(HttpClient);
  viewContainerRef = inject(ViewContainerRef);
  aiAssistantService = inject(AiAssistantService);
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
  private animationFrameId?: number;
  private pendingWidth?: number;

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
    const currentSessionId = this.aiAssistantService.currentSessionId();
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
    this.aiAssistantService.setViewContainerRef(this.viewContainerRef);
    
    // CRITICAL: Check for router state first (used when navigating from sidebar to fullscreen)
    // This preserves session data for new sessions that don't have a server ID yet
    const navigation = this.router.getCurrentNavigation();
    const routerState = navigation?.extras?.state || (window.history.state as any);
    
    if (routerState?.preserveData && routerState?.chatSession) {
      // Session data was passed via router state - use it directly
      // This handles the case where we're switching modes with a new session
      console.log('📦 Using session data from router state');
      this.aiAssistantService.currentChatSession.set(routerState.chatSession);
      
      // Update related signals from the session data
      if (routerState.chatSession.session) {
        if (routerState.chatSession.session.id) {
          this.aiAssistantService.currentSessionId.set(routerState.chatSession.session.id);
        }
        if (routerState.chatSession.session.title) {
          this.aiAssistantService.sessionTitle.set(routerState.chatSession.session.title);
        }
        this.aiAssistantService.sessionStarred.set(routerState.chatSession.session.starred || false);
        this.aiAssistantService.sessionArchived.set(routerState.chatSession.session.archived || false);
      }
      
      // Don't load from server - we already have the data
      return;
    }
    
    // Handle sessionId from route parameter (for direct URL navigation or browser refresh)
    this.route.params.subscribe(params => {
      const sessionId = params['sessionId'];
      if (sessionId && sessionId !== this.aiAssistantService.currentSessionId()) {
        // Load the specific session from server
        this.aiAssistantService.switchToSession(sessionId).subscribe({
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
    // Clean up resize handling
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
    this.document.removeEventListener('mousemove', this.onResizing);
    this.document.removeEventListener('mouseup', this.stopResizing);
    this.document.body.classList.remove('resizing');
    
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
    
    // Store the pending width and schedule an update
    this.pendingWidth = newWidth;
    
    if (!this.animationFrameId) {
      this.animationFrameId = requestAnimationFrame(() => {
        if (this.pendingWidth !== undefined) {
          this.rightPanelWidth = this.pendingWidth;
          this.pendingWidth = undefined;
          this.cdr.detectChanges();
        }
        this.animationFrameId = undefined;
      });
    }
  };

  stopResizing = () => {
    this.resizing = false;
    this.document.removeEventListener('mousemove', this.onResizing);
    this.document.removeEventListener('mouseup', this.stopResizing);
    this.document.body.classList.remove('resizing');
    
    // Cancel any pending animation frame and apply final width
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = undefined;
    }
    
    // Apply any pending width change
    if (this.pendingWidth !== undefined) {
      this.rightPanelWidth = this.pendingWidth;
      this.pendingWidth = undefined;
      this.cdr.detectChanges();
    }
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

