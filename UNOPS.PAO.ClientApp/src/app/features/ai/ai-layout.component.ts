import { Component, inject, signal, OnInit, computed, ViewContainerRef, effect, ViewChildren, QueryList, Type, Injector, ChangeDetectorRef, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { LayoutService } from '../../common/layouts/services/layout.service';
import { TopbarComponent } from '../../common/layouts/components/topbar/topbar.component';
import { AiAssistantPanelComponent } from '../../common/reusables/widgets/ai-assistant/ai-assistant-panel.component';
import { AiAssistantData } from '../../common/reusables/widgets/ai-assistant/ai-assistant.data';
import { MenuModule } from 'primeng/menu';
import { Menu } from 'primeng/menu';
import { DialogModule } from 'primeng/dialog';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService } from 'primeng/api';
import { PartnerViewComponent } from '../../features/internal/components/partner/view/partner-view.component';
import { ContactViewComponent } from '../../features/internal/components/contact/view/contact-view.component';
import { InteractionModalComponent } from '../../features/internal/components/interaction/modal/interaction-modal.component';

import { GlobalFilterService } from '../../services/global-filter.service';
import { TranslateModule } from '@ngx-translate/core';

/**
 * @uiEntity AiAssistant
 * @route /ai
 * @description Full-featured AI Assistant interface with chat sessions management, smart sidebars, and context-aware assistance. Provides comprehensive AI support for navigating and using the application effectively.
 * @capabilities ai_chat, session_management, context_awareness, entity_preview, smart_assistance, chat_history, fullscreen_mode
 * @synonyms ai_chat, virtual_assistant, smart_help, ai_support, intelligent_assistant
 * @mandatoryFields None
 * @help_when_stuck Start by typing your question in the chat input. The AI can help you navigate the app, understand features, find data, or perform tasks. Use the sidebar to manage chat sessions, search previous conversations, or access entity previews. Click the fullscreen icon for an expanded view.
 * @common_tasks
 *   - Getting help: Type questions about how to use features or find information
 *   - Managing sessions: Create new chats, rename sessions, or search chat history
 *   - Entity assistance: Ask about partners, contacts, or interactions for contextual help
 *   - Navigation help: Get guidance on where to find specific features or data
 *   - Task automation: Request help with complex workflows or data entry
 */

interface ChatSession {
  id: string;
  title: string;
  lastUpdated: string;
  status: string;
  starred: boolean;
  archived: boolean;
}

@Component({
  selector: 'app-ai-layout',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    HttpClientModule,
    TopbarComponent,
    AiAssistantPanelComponent,
    MenuModule,
    DialogModule,
    ConfirmDialogModule,
    ButtonModule,
    InputTextModule,
    TooltipModule,
    PartnerViewComponent,
    ContactViewComponent,
    InteractionModalComponent,

    TranslateModule
  ],
  templateUrl: './ai-layout.component.html',
  styleUrls: ['./ai-layout.component.css']
})
export class AiLayoutComponent implements OnInit, OnDestroy {
  layoutService = inject(LayoutService);
  router = inject(Router);
  route = inject(ActivatedRoute);
  http = inject(HttpClient);
  viewContainerRef = inject(ViewContainerRef);
  aiAssistantData = inject(AiAssistantData);
  confirmationService = inject(ConfirmationService);
  private cdr = inject(ChangeDetectorRef);
  private globalFilterService = inject(GlobalFilterService);

  searchQuery = signal('');
  chatSessions = signal<ChatSession[]>([]);
  isLoadingSessions = signal(false);

  // Mobile responsiveness
  isMobile = signal(false);
  sidebarOpen = signal(false);
  
  // Add a signal for hover state
  sidebarHovered = signal(false);

  showRenameDialog = false;
  renameTitle = '';
  currentChatBeingRenamed: ChatSession | null = null;
  confirmAction: null | (() => void) = null;

  // Computed property for filtered chat sessions
  filteredChatSessions = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const sessions = this.chatSessions();
    if (!query) return sessions;
    return sessions.filter(session =>
      (session.title ?? '').toLowerCase().includes(query)
    );
  });

  // New Chat button should be disabled when already in new chat state
  isNewChatDisabled = computed(() => !this.aiAssistantData.currentSessionId());

  selectedChatId = signal<string | null>(null);

  // Remove the static chatMenuItems array
  // menuChat: any = null; // This line is removed as per the new_code

  @ViewChildren('chatMenu') chatMenus!: QueryList<Menu>;

  rightPanelVisible = false;
  rightPanelWidth = 600;
  rightPanelType: 'component' | 'url' | null = null;
  rightPanelComponent: Type<any> | null = null;
  rightPanelUrl: string | null = null;
  resizing = false;
  private startX = 0;
  private startWidth = 600;
  private document = window.document;
  private injector = inject(Injector);

  private entityComponentMap: Record<string, any> = {
    partner: PartnerViewComponent,
    contact: ContactViewComponent,
    interaction: InteractionModalComponent
  };
  rightPanelEntityType: string | null = null;
  rightPanelEntityId: string | null = null;
  rightPanelRowData: any = null;

  // Global filter state
  globalFilterEnabled = signal<boolean>(true);

  ngOnInit() {
    this.loadUserSessions();
    
    // Initialize mobile detection
    this.detectMobile();
    
    // Initialize global filter state from service
    this.globalFilterEnabled.set(this.globalFilterService.isFilterEnabled());
    
    // Set the ViewContainerRef for the AI assistant data service
    this.aiAssistantData.setViewContainerRef(this.viewContainerRef);
    
    // Handle sessionId from route parameter
    this.route.params.subscribe(params => {
      const sessionId = params['sessionId'];
      if (sessionId) {
        // Set selected chat ID immediately for highlighting
        this.selectedChatId.set(sessionId);
        
        // Load the specific session
        this.aiAssistantData.switchToSession(sessionId).subscribe({
          next: () => {
            // Ensure we reload sessions to have the latest list for highlighting
            this.loadUserSessions();
          },
          error: (error) => {
            console.error('Failed to load session from URL:', error);
            // Reset selected chat ID on error
            this.selectedChatId.set(null);
            // Optionally redirect to /ai without sessionId if session doesn't exist
            this.router.navigate(['/ai'], { replaceUrl: true });
          }
        });
      } else {
        // No session ID in URL, clear selection
        this.selectedChatId.set(null);
      }
    });
    
    // Listen for current session changes to update selected chat
    effect(() => {
      const currentSessionId = this.aiAssistantData.currentSessionId();
      this.selectedChatId.set(currentSessionId);
    });
    
    // Listen for current session changes to update URL
    effect(() => {
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
    // Listen for the first model message in a new session to refresh recent chats
    effect(() => {
      const chatHistory = this.aiAssistantData.chatHistory();
      if (chatHistory.length === 1 && !chatHistory[0].isUser) {
        // First model message received in a new session
        this.loadUserSessions();
      }
    });

    // Listen for layout service sidebar changes to sync mobile state
    effect(() => {
      const sidebarCollapsed = this.layoutService.aiAssistantSidebarCollapsed();
      if (this.isMobile()) {
        // On mobile, when sidebar is "uncollapsed" it means it should be open
        this.sidebarOpen.set(!sidebarCollapsed);
      }
    });
  }

  ngOnDestroy() {
    // Clean up any subscriptions if needed
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: any) {
    this.detectMobile();
  }

  private detectMobile() {
    const isMobileDevice = window.innerWidth <= 768;
    this.isMobile.set(isMobileDevice);
    
    // Auto-collapse sidebar on mobile
    if (isMobileDevice) {
      if (!this.layoutService.aiAssistantSidebarCollapsed()) {
        this.layoutService.onAiSidebarToggle();
      }
      this.sidebarOpen.set(false);
    }
  }

  async loadUserSessions(): Promise<void> {
    this.isLoadingSessions.set(true);
    try {
      const response = await this.http.post<ChatSession[]>('/api/ai-assistant/get-user-sessions', {}).toPromise();
      if (response) {
        // Sort by date (newest first)
        const sortedSessions = response.sort((a, b) => 
          new Date(b.lastUpdated).getTime() - new Date(a.lastUpdated).getTime()
        );
        this.chatSessions.set(sortedSessions);
      }
    } catch (error) {
      console.error('Error loading user sessions:', error);
      this.chatSessions.set([]);
    } finally {
      this.isLoadingSessions.set(false);
    }
  }

  startNewChat() {
    this.aiAssistantData.clearConversation();
    this.selectedChatId.set(null);
    
    // Update URL to /ai when starting a new chat
    const currentRoute = this.router.url;
    if (currentRoute.startsWith('/ai') && currentRoute !== '/ai') {
      this.router.navigate(['/ai'], { replaceUrl: true });
    }
    
    // Close sidebar on mobile after starting new chat
    if (this.isMobile()) {
      this.closeMobileSidebar();
    }
  }

  closeMobileSidebar() {
    if (this.isMobile()) {
      // Close sidebar by making it collapsed
      if (!this.layoutService.aiAssistantSidebarCollapsed()) {
        this.layoutService.onAiSidebarToggle();
      }
    }
  }

  openChat(chat: ChatSession) {
    this.selectedChatId.set(chat.id);
    
    // Update URL immediately when opening a chat
    const currentRoute = this.router.url;
    if (currentRoute.startsWith('/ai') && currentRoute !== `/ai/${chat.id}`) {
      this.router.navigate(['/ai', chat.id], { replaceUrl: true });
    }
    
    this.aiAssistantData.switchToSession(chat.id).subscribe({
      error: (error) => console.error('Failed to switch session:', error)
    });
    
    // Close sidebar on mobile after opening chat
    if (this.isMobile()) {
      this.closeMobileSidebar();
    }
  }

  // Helper method to check if a chat is selected (handles type mismatches)
  isSelectedChat(chat: ChatSession): boolean {
    const selected = this.selectedChatId();
    if (!selected || !chat.id) {
      return false;
    }
    
    // Direct comparison first
    if (selected === chat.id) {
      return true;
    }
    
    // String comparison as fallback for type mismatches
    const result = String(selected) === String(chat.id);
    if (result) {
      console.log('🔍 [AI Layout] Chat matched via string conversion:', selected, '===', chat.id);
    }
    return result;
  }



  onSearchInput(event: any): void {
    const query = event.target.value;
    this.searchQuery.set(query);
  }

  clearSearch(): void {
    this.searchQuery.set('');
  }

  // TrackBy function for chat sessions to improve performance
  trackByChatId(index: number, chat: any): string {
    return chat.id || index;
  }

  performSearch(): void {
    const query = this.searchQuery();
    if (query.trim()) {
      console.log('Performing search for:', query);
      // Add your search logic here
    }
  }

  openChatMenu(event: MouseEvent, chat: any, index: number) {
    event.stopPropagation();
    event.preventDefault();
    
    console.log('Opening chat menu for:', chat.title, 'at index:', index);
    
    // Set menu items for the current chat before showing menu
    this.currentChatMenuItems = this.createChatMenuItems(chat);
    
    const menu = this.chatMenus?.toArray()[index];
    if (menu && menu.toggle) {
      try {
        menu.toggle(event);
        console.log('Menu toggled successfully');
      } catch (error) {
        console.error('Error toggling menu:', error);
      }
    } else {
      console.warn('Menu not found at index:', index, 'Available menus:', this.chatMenus?.length);
    }
  }

  // Store current menu items to avoid infinite re-rendering
  currentChatMenuItems: any[] = [];

  private createChatMenuItems(chat: any): any[] {
    console.log('Creating menu items for chat:', chat.title, 'starred:', chat.starred, 'archived:', chat.archived);
    return [
      { 
        label: 'Rename', 
        icon: 'pi pi-pencil', 
        command: () => {
          console.log('Rename menu item clicked for:', chat.title);
          this.renameChat(chat);
        }
      },
      { 
        label: chat.starred ? 'Unstar' : 'Star', 
        icon: chat.starred ? 'pi pi-star-fill' : 'pi pi-star', 
        command: () => {
          console.log('Star/Unstar menu item clicked for:', chat.title);
          this.toggleStarChat(chat);
        }
      },
      { 
        label: chat.archived ? 'Unarchive' : 'Archive', 
        icon: chat.archived ? 'pi pi-folder-open' : 'pi pi-archive', 
        command: () => {
          console.log('Archive/Unarchive menu item clicked for:', chat.title);
          this.archiveChat(chat);
        }
      }
    ];
  }

  renameChat(chat: any) {
    console.log('renameChat called for:', chat.title);
    this.currentChatBeingRenamed = chat;
    this.renameTitle = chat.title;
    this.showRenameDialog = true;
  }

  saveRename() {
    if (this.renameTitle.trim() && this.currentChatBeingRenamed) {
      // Update the specific chat by ID
      const updated = this.chatSessions().map(c => 
        c.id === this.currentChatBeingRenamed!.id 
          ? { ...c, title: this.renameTitle.trim() } 
          : c
      );
      this.chatSessions.set(updated);
      this.showRenameDialog = false;
      this.currentChatBeingRenamed = null;
      this.renameTitle = '';
      
      // TODO: Call API to save the title to the backend
      console.log('Chat renamed to:', this.renameTitle.trim());
    }
  }

  cancelRename() {
    this.showRenameDialog = false;
    this.currentChatBeingRenamed = null;
    this.renameTitle = '';
  }
  toggleStarChat(chat: any) {
    console.log('toggleStarChat called for:', chat.title, 'current starred:', chat.starred);
    this.confirmAction = () => {
      console.log('Executing star toggle for:', chat.title);
      const updated = this.chatSessions().map(c => c.id === chat.id ? { ...c, starred: !c.starred } : c);
      this.chatSessions.set(updated);
    };
    this.confirmationService.confirm({
      message: chat.starred ? 'Remove star from this chat?' : 'Star this chat?',
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      accept: () => { if (this.confirmAction) this.confirmAction(); },
      reject: () => { this.confirmAction = null; }
    });
  }
  
  archiveChat(chat: any) {
    console.log('archiveChat called for:', chat.title, 'current archived:', chat.archived);
    this.confirmAction = () => {
      console.log('Executing archive toggle for:', chat.title);
      const updated = this.chatSessions().map(c => c.id === chat.id ? { ...c, archived: !c.archived } : c);
      this.chatSessions.set(updated);
    };
    this.confirmationService.confirm({
      message: chat.archived ? 'Unarchive this chat?' : 'Archive this chat?',
      header: 'Confirm',
      icon: 'pi pi-exclamation-triangle',
      accept: () => { if (this.confirmAction) this.confirmAction(); },
      reject: () => { this.confirmAction = null; }
    });
  }

  openRightPanelWithComponent(component: Type<any>, data: any) {
    this.rightPanelType = 'component';
    this.rightPanelComponent = component;
    this.rightPanelVisible = true;
  }
  openRightPanelWithUrl(url: string) {
    this.rightPanelType = 'url';
    this.rightPanelUrl = url;
    this.rightPanelVisible = true;
  }
  closeRightPanel() {
    this.rightPanelVisible = false;
    
    // Force change detection to update the layout
    this.cdr.detectChanges();
    
    // Clean up after animation completes
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
    // Force change detection to update the chat area width
    this.cdr.detectChanges();
  };

  stopResizing = () => {
    this.resizing = false;
    this.document.removeEventListener('mousemove', this.onResizing);
    this.document.removeEventListener('mouseup', this.stopResizing);
    this.document.body.classList.remove('resizing');
  };

  onCardClicked(event: { entityType: string, entityId: string, rowData: any }) {
    console.log('🔗 AiLayout - Card clicked:', event);
    
    // Check if different entity (type or ID) is clicked
    const isDifferentEntity = this.rightPanelEntityType !== event.entityType || 
                             this.rightPanelEntityId !== event.entityId;

    if (isDifferentEntity) {
      console.log('🔗 AiLayout - Different entity detected, will reload component');
      
      // Temporarily close the panel to trigger component destruction
      this.rightPanelVisible = false;
      this.cdr.detectChanges();
      
      // Use setTimeout to ensure the component is fully destroyed before recreating
      setTimeout(() => {
        this.loadEntityInPanel(event);
      }, 0);
    } else {
      console.log('🔗 AiLayout - Same entity, keeping existing panel');
    }
  }

  private loadEntityInPanel(event: { entityType: string, entityId: string, rowData: any }) {
    console.log('🔗 AiLayout - Loading entity in panel:', event);
    
    // Store entity information
    this.rightPanelEntityType = event.entityType;
    this.rightPanelEntityId = event.entityId;
    this.rightPanelRowData = event.rowData;
    
    // Check if we have a component for this entity type
    const componentKey = event.entityType.toLowerCase();
    const component = this.entityComponentMap[componentKey];
    
    if (component) {
      console.log('🔗 AiLayout - Found component for', event.entityType);
      this.rightPanelType = 'component';
      this.rightPanelComponent = component;
      this.rightPanelVisible = true;
      this.cdr.detectChanges();
    } else {
      console.log('🔗 AiLayout - No component found for', event.entityType, ', available:', Object.keys(this.entityComponentMap));
      this.rightPanelType = 'component';
      this.rightPanelComponent = null; // This will show the "Coming Soon" placeholder
      this.rightPanelVisible = true;
    this.cdr.detectChanges();
    }
  }

  onUrlClicked(url: string | Event) {
    console.log('🔗 AiLayout - URL clicked:', url);
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
        // Construct full URL with hash for Angular routing
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

  toggleGlobalFilter() {
    const newState = !this.globalFilterEnabled();
    this.globalFilterEnabled.set(newState);
    this.globalFilterService.setFilterEnabled(newState);
    console.log('Global filter toggled:', newState);
  }

  getEntityDisplayName(): string {
    if (!this.rightPanelEntityType || !this.rightPanelRowData) {
      return 'Entity Details';
    }

    // Try to get a meaningful name from the row data
    const data = this.rightPanelRowData;
    
    // Common name fields to check
    const nameFields = ['name', 'title', 'displayName', 'fullName', 'firstName', 'lastName'];
    
    for (const field of nameFields) {
      if (data[field] && typeof data[field] === 'string') {
        return data[field];
      }
    }
    
    // If firstName and lastName exist separately, combine them
    if (data.firstName && data.lastName) {
      return `${data.firstName} ${data.lastName}`;
    }
    
    // Fallback to entity type
    return this.rightPanelEntityType;
  }
} 