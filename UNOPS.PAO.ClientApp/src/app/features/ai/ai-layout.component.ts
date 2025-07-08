import { Component, inject, signal, OnInit, computed, ViewContainerRef, effect, ViewChildren, QueryList, Type, Injector, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
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
    TooltipModule
  ],
  template: `
    <div class="layout-wrapper ai-layout-wrapper">
      <app-topbar></app-topbar>
      <div class="ai-layout-container" [ngStyle]="{ 'display': 'flex', 'flexDirection': 'row', 'height': '100%' }">
        <!-- Sidebar -->
        <div class="ai-sidebar" 
          [class.sidebar-collapsed]="layoutService.aiAssistantSidebarCollapsed() && !sidebarHovered()"
          (mouseenter)="sidebarHovered.set(true)"
          (mouseleave)="sidebarHovered.set(false)">
          <div class="sidebar-header">
            <button 
              (click)="layoutService.onAiSidebarToggle()"
              class="sidebar-toggle-btn"
              type="button">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
            <span class="debug-text" *ngIf="!layoutService.aiAssistantSidebarCollapsed() || sidebarHovered()">Sidebar: {{ layoutService.aiAssistantSidebarCollapsed() ? 'Collapsed' : 'Expanded' }}</span>
          </div>
          
          <div class="sidebar-content" [style.display]="(layoutService.aiAssistantSidebarCollapsed() && !sidebarHovered()) ? 'block' : 'block'" [ngClass]="{'sidebar-content-collapsed': layoutService.aiAssistantSidebarCollapsed() && !sidebarHovered()}" >
            <!-- New Chat Button (collapsed: icon only, top-aligned) -->
            <div class="p-4 border-b border-[var(--surface-border)] flex items-center" *ngIf="layoutService.aiAssistantSidebarCollapsed() && !sidebarHovered()">
              <button 
                (click)="startNewChat()"
                class="new-chat-button rounded-full p-3 flex items-center justify-center font-bold new-chat-collapsed"
                [disabled]="selectedChatId() === null"
                [title]="'New Chat'">
                <i class="pi pi-plus text-3xl font-extrabold text-[var(--primary-color)]"></i>
              </button>
            </div>
            <!-- Full sidebar content when expanded -->
            <ng-container *ngIf="!layoutService.aiAssistantSidebarCollapsed() || sidebarHovered()">
              <!-- New Chat Button -->
              <div class="p-4 border-b border-[var(--surface-border)]">
                <button 
                  (click)="startNewChat()"
                  class="new-chat-button w-full flex items-center gap-3 px-4 py-3 rounded-lg transition-colors duration-400 text-left font-bold"
                  [disabled]="selectedChatId() === null">
                  <i class="pi pi-plus text-2xl font-extrabold text-[var(--primary-color)]"></i>
                  <span class="font-bold text-lg">New Chat</span>
                </button>
              </div>
              <!-- Search Section -->
              <div class="sidebar-search-container">
                <div class="search-input-wrapper">
                  <input 
                    type="text" 
                    [(ngModel)]="searchQuery"
                    (input)="onSearchInput($event)"
                    placeholder="Search chats..."
                    class="search-input"
                  />
                  <i class="pi pi-search search-icon"></i>
                  <button 
                    *ngIf="searchQuery()"
                    (click)="clearSearch()"
                    class="clear-search-btn"
                    type="button">
                    <i class="pi pi-times"></i>
                  </button>
                </div>
              </div>

              <!-- Recent Chats Section -->
              <div class="sidebar-section">
                <div class="section-title">
                  <span>Recent Chats</span>
                  <span *ngIf="searchQuery()" class="search-results-count">
                    ({{ filteredChatSessions().length }} of {{ chatSessions().length }})
                  </span>
                </div>
                <div class="chat-list">
                  <div *ngIf="isLoadingSessions()" class="loading-message">
                    <div class="loading-spinner"></div>
                    <span>Loading chats...</span>
                  </div>
                  
                  <div *ngIf="!isLoadingSessions() && chatSessions().length === 0" class="no-chats-message">
                    <span class="text-sm text-gray-500">No chat history yet</span>
                  </div>
                  
                  <div *ngFor="let chat of filteredChatSessions(); let i = index" 
                       class="chat-item"
                       (click)="openChat(chat)"
                       [class.active]="selectedChatId() === chat.id">
                    <div class="chat-item-content">
                      <div class="chat-title">{{ chat.title }}</div>
                      <div class="chat-date">{{ formatDate(chat.lastUpdated) }}</div>
                    </div>
                    <div class="chat-status" [class.starred]="chat.starred">
                      <svg *ngIf="chat.starred" xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="currentColor" viewBox="0 0 24 24">
                        <path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/>
                      </svg>
                    </div>
                    <div class="chat-actions">
                      <button class="chat-menu-btn" (click)="openChatMenu($event, chat, i)">
                        <i class="pi pi-ellipsis-v"></i>
                      </button>
                      <p-menu #chatMenu [popup]="true" [model]="getChatMenuItems(chat)"></p-menu>
                    </div>
                  </div>
                </div>
              </div>
            </ng-container>
          </div>
        </div>
        
        <!-- AI Assistant Panel (Fullscreen Mode) -->
        <div class="ai-content-area">
          <app-ai-assistant-panel 
            [hideHeader]="true"
            [viewContainerRef]="viewContainerRef"
            (cardClicked)="onCardClicked($event)"
            (urlClicked)="onUrlClicked($event)">
          </app-ai-assistant-panel>
        </div>

        <!-- Right Panel (Third Panel) -->
        <div class="ai-right-panel" 
             [class.panel-visible]="rightPanelVisible" 
             [style.flex-basis]="rightPanelVisible ? rightPanelWidth + 'px' : '0px'"
             [style.min-width]="rightPanelVisible ? '320px' : '0px'"
             [style.max-width]="rightPanelVisible ? '50vw' : '0px'"
             [style.display]="rightPanelVisible ? 'flex' : 'none'">
        <div class="right-panel-header" *ngIf="rightPanelVisible">
          <div class="panel-title">
            <h4>{{ rightPanelEntityType | titlecase }} Details</h4>
            <span class="panel-subtitle">ID: {{ rightPanelEntityId }}</span>
          </div>
        </div>
        <div class="right-panel-content">
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
          
          <ng-container *ngIf="rightPanelType === 'component' && rightPanelComponent">
            <ng-container *ngComponentOutlet="rightPanelComponent; injector: rightPanelInjector || undefined"></ng-container>
          </ng-container>
          <ng-container *ngIf="rightPanelType === 'component' && !rightPanelComponent">
            <div class="coming-soon-container">
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
            </div>
          </ng-container>
          <ng-container *ngIf="rightPanelType === 'url'">
            <iframe [src]="rightPanelUrl" width="100%" height="100%" frameborder="0"></iframe>
          </ng-container>
        </div>
        <div class="resize-handle" (mousedown)="startResizing($event)"></div>
      </div>
      </div>
    </div>
    <p-dialog header="Rename Chat" [(visible)]="showRenameDialog" [modal]="true" [closable]="true" [style]="{width: '350px'}">
      <div class="flex flex-col gap-4">
        <input pInputText [(ngModel)]="renameTitle" placeholder="Enter new title" class="w-full" />
        <div class="flex justify-end gap-2 mt-2">
          <button pButton type="button" label="Cancel" (click)="showRenameDialog = false"></button>
          <button pButton type="button" label="Save" [disabled]="!renameTitle.trim()" (click)="saveRename()" class="p-button-primary"></button>
        </div>
      </div>
    </p-dialog>
    <p-confirmDialog></p-confirmDialog>
  `,
  styles: [`
    .layout-wrapper {
      height: 100vh;
      display: flex;
      flex-direction: column;
      background-color: var(--surface-ground);
    }
    .ai-layout-container {
      flex: 1;
      display: flex;
      height: calc(100vh - 105px);
      overflow: hidden;
    }
    
    /* Sidebar styles */
    .ai-sidebar {
      width: 320px;
      background: var(--surface-card);
      border-right: 1px solid var(--surface-border);
      display: flex;
      flex-direction: column;
      transition: width 0.5s cubic-bezier(0.4, 0, 0.2, 1), background 0.5s, box-shadow 0.5s;
      z-index: 2;
    }
    
    .ai-sidebar.sidebar-collapsed {
      width: 60px;
    }
    
    .sidebar-header {
      padding: 1rem;
      display: flex;
      align-items: center;
      justify-content: flex-start;
      gap: 0.75rem;
      min-height: 60px;
    }
    
    .sidebar-toggle-btn {
      background: var(--primary-color);
      border: none;
      cursor: pointer;
      padding: 0.75rem;
      border-radius: 0.5rem;
      color: white;
      transition: all 0.2s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      min-width: 44px;
      min-height: 44px;
    }
    
    .sidebar-toggle-btn:hover {
      background: var(--primary-color-dark, #0056b3);
      transform: scale(1.05);
    }
    
    .sidebar-title {
      font-weight: 600;
      color: var(--text-color);
      font-size: 1.1rem;
    }
    
    .sidebar-content {
      flex: 1;
      padding: 0.5rem 1rem 1rem 1rem;
      overflow-y: auto;
      margin-top: -1px; /* Move up to eliminate double border */
    }
    
    .sidebar-content-collapsed {
      display: block !important;
      padding: 0;
    }
    
    .sidebar-section {
      margin-bottom: 1.5rem;
    }
    
    .section-title {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-color);
      margin-bottom: 0.75rem;
      padding-left: 0.5rem;
    }
    
    .new-chat-btn {
      width: 100%;
      background: var(--primary-color);
      color: white;
      border: none;
      padding: 0.75rem 1rem;
      border-radius: 0.5rem;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-weight: 500;
      transition: all 0.2s ease;
    }
    
    .new-chat-btn:hover {
      background: var(--primary-color-dark, #0056b3);
      transform: translateY(-1px);
    }

    .new-chat-button {
      background: var(--surface-card);
      border: none;
      color: var(--text-color);
      transition: all 0.2s ease;
      font-weight: bold;
    }

    .new-chat-button .pi-plus {
      font-weight: 900;
    }

    .new-chat-button:hover:not(:disabled) {
      background: var(--surface-hover);
      border: none;
      box-shadow: none;
    }

    .new-chat-button:active {
      transform: translateY(1px);
    }

    .new-chat-button:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .new-chat-collapsed {
      width: 44px;
      height: 44px;
      font-size: 1.7rem;
      align-items: center;
      justify-content: center;
    }

    .new-chat-button span {
      font-weight: bold;
      font-size: 1.15rem;
    }
    
    .sidebar-search-container {
      /* No additional margin since section handles it */
    }
    
    .chat-list {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
    }
    
    .chat-item {
      display: flex;
      align-items: center;
      padding: 0.75rem;
      border-radius: 0.5rem;
      cursor: pointer;
      transition: all 0.2s ease;
      border: 1px solid transparent;
    }
    
    .chat-item:hover {
      background: var(--surface-hover);
      border-color: var(--surface-border);
    }
    
    .chat-item.active {
      background: var(--primary-color);
      color: white;
    }
    
    .chat-item-content {
      flex: 1;
      min-width: 0;
    }
    
    .chat-title {
      font-weight: 500;
      font-size: 0.875rem;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      margin-bottom: 0.25rem;
    }
    
    .chat-date {
      font-size: 0.75rem;
      opacity: 0.7;
    }
    
    .chat-status {
      margin-left: 0.5rem;
      opacity: 0.5;
    }
    
    .chat-status.starred {
      color: #fbbf24;
      opacity: 1;
    }
    
    .loading-message {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 1rem;
      justify-content: center;
      color: var(--text-color-secondary);
    }
    
    .loading-spinner {
      width: 16px;
      height: 16px;
      border: 2px solid var(--surface-border);
      border-top: 2px solid var(--primary-color);
      border-radius: 50%;
      animation: spin 1s linear infinite;
    }
    
    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
    
    .no-chats-message {
      padding: 1rem;
      text-align: center;
      color: var(--text-color-secondary);
    }
    
    .ai-content-area {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: stretch;
      justify-content: stretch;
      background: var(--surface-ground);
      padding: 0;
      min-height: 0;
      overflow: hidden;
    }
    
    .blank-content {
      text-align: center;
      color: var(--text-color);
    }
    
    .blank-content h1 {
      font-size: 2.5rem;
      margin-bottom: 1rem;
      color: var(--text-color);
    }
    
    .blank-content p {
      font-size: 1.2rem;
      color: var(--text-color-secondary);
    }
    
    .debug-info {
      margin-top: 1rem;
      font-size: 0.875rem;
      opacity: 0.7;
    }

    .search-input-wrapper {
      position: relative;
      margin-bottom: 1rem;
    }

    .search-input {
      width: 100%;
      padding: 0.75rem 1rem 0.75rem 2.5rem;
      border: 1px solid var(--surface-border);
      border-radius: 0.5rem;
      background-color: var(--surface-input);
      color: var(--text-color);
      font-size: 0.875rem;
      transition: all 0.2s ease;
    }

    .search-input:focus {
      border-color: var(--primary-color);
      box-shadow: 0 0 0 2px var(--primary-color-light);
      outline: none;
    }

    .search-icon {
      position: absolute;
      left: 0.75rem;
      top: 50%;
      transform: translateY(-50%);
      color: var(--text-color-secondary);
      font-size: 1rem;
    }

    .clear-search-btn {
      position: absolute;
      right: 8px;
      top: 50%;
      transform: translateY(-50%);
      background: none;
      border: none;
      color: var(--text-color-secondary);
      cursor: pointer;
      padding: 4px;
      border-radius: 50%;
      transition: all 0.2s ease;
    }

    .clear-search-btn:hover {
      background: var(--surface-hover);
      color: var(--text-color);
    }

    .search-results-count {
      font-size: 0.75rem;
      color: var(--text-color-secondary);
      font-weight: normal;
    }

    .section-title {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.75rem 1rem;
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-color);
      background: var(--surface-ground);
      border-bottom: 1px solid var(--surface-border);
    }
    .ai-layout-container { position: relative; width: 100%; height: 100%; display: flex; }
     .ai-assistant-panel { min-width: 0; height: 100%; overflow: hidden; }
     .ai-right-panel { 
       background: #fff; 
       border-left: 1px solid #e5e7eb; 
       height: 100%; 
       display: flex; 
       flex-direction: column; 
       box-shadow: -2px 0 8px rgba(0,0,0,0.04); 
       overflow: hidden;
       transition: flex-basis 0.3s ease, min-width 0.3s ease, max-width 0.3s ease;
       flex-shrink: 0;
       z-index: 1;
     }
     
     .ai-right-panel.panel-visible {
       /* Panel is visible - flex properties are set via style bindings */
     }
     .right-panel-header { 
       padding: 1rem 1.5rem; 
       border-bottom: 1px solid #e5e7eb; 
       display: flex; 
       justify-content: space-between; 
       align-items: center; 
       background: #f9fafb;
       min-height: 60px;
       position: relative;
       z-index: 1;
       flex-shrink: 0;
     }
     
     .panel-title h4 {
       margin: 0;
       font-size: 1.125rem;
       font-weight: 600;
       color: #111827;
       line-height: 1.4;
     }
     
     .panel-subtitle {
       font-size: 0.875rem;
       color: #6b7280;
       margin-top: 2px;
       display: block;
     }
     
     .panel-actions {
       display: flex;
       gap: 0.5rem;
       align-items: center;
       justify-content: flex-end;
       padding: 1rem 1.5rem;
       border-bottom: 1px solid #e5e7eb;
       background: #f9fafb;
     }
     
     .panel-action-btn {
       width: 36px;
       height: 36px;
       border: 1px solid transparent;
       border-radius: 50%;
       background: transparent;
       color: #6b7280;
       cursor: pointer;
       display: flex;
       align-items: center;
       justify-content: center;
       transition: all 0.2s ease;
       font-size: 16px;
     }
     
     .panel-action-btn:hover {
       color: #374151;
       background-color: #f3f4f6;
       border-color: #e5e7eb;
       transform: scale(1.05);
     }
     
     .panel-action-btn:active {
       transform: scale(0.95);
     }
     
     .panel-action-btn i {
       font-size: 16px;
     }
     .right-panel-content { flex: 1 1 0; overflow: auto; }
     .resize-handle { width: 6px; cursor: ew-resize; position: absolute; left: 0; top: 0; bottom: 0; z-index: 20; background: transparent; }
     
     .coming-soon-container { text-align: center; padding: 2rem; }
     .coming-soon-icon { font-size: 3rem; color: #6b7280; margin-bottom: 1rem; }
     .coming-soon-container h4 { margin: 0 0 0.5rem 0; font-size: 1.5rem; color: #111827; }
     .coming-soon-container p { margin: 0 0 2rem 0; color: #6b7280; line-height: 1.6; }
     .entity-info { background: #f9fafb; border-radius: 0.5rem; padding: 1rem; text-align: left; }
     .info-item { margin-bottom: 0.75rem; }
     .info-item:last-child { margin-bottom: 0; }
     .info-item strong { color: #374151; display: block; margin-bottom: 0.25rem; }
  `]
})
export class AiLayoutComponent implements OnInit {
  layoutService = inject(LayoutService);
  router = inject(Router);
  http = inject(HttpClient);
  viewContainerRef = inject(ViewContainerRef);
  aiAssistantData = inject(AiAssistantData);
  confirmationService = inject(ConfirmationService);
  private cdr = inject(ChangeDetectorRef);

  searchQuery = signal('');
  chatSessions = signal<ChatSession[]>([]);
  isLoadingSessions = signal(false);

  // Add a signal for hover state
  sidebarHovered = signal(false);

  showRenameDialog = false;
  renameTitle = '';
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

  selectedChatId = signal<string | null>(null);

  // Remove the static chatMenuItems array
  // menuChat: any = null; // This line is removed as per the new_code

  @ViewChildren('chatMenu') chatMenus!: QueryList<Menu>;

  rightPanelVisible = false;
  rightPanelWidth = 400;
  rightPanelType: 'component' | 'url' | null = null;
  rightPanelComponent: Type<any> | null = null;
  rightPanelInjector: Injector | null = null;
  rightPanelUrl: string | null = null;
  private resizing = false;
  private startX = 0;
  private startWidth = 400;
  private document = window.document;
  private injector = inject(Injector);

  private entityComponentMap: Record<string, any> = {
    partner: PartnerViewComponent,
    contact: ContactViewComponent,
    interaction: InteractionModalComponent
  };
  rightPanelEntityType: string | null = null;
  rightPanelEntityId: string | null = null;

  ngOnInit() {
    this.loadUserSessions();
    
    // Set the ViewContainerRef for the AI assistant data service
    this.aiAssistantData.setViewContainerRef(this.viewContainerRef);
    
    // Listen for current session changes to update selected chat
    effect(() => {
      this.selectedChatId.set(this.aiAssistantData.currentSessionId());
    });
    // Listen for the first model message in a new session to refresh recent chats
    effect(() => {
      const chatHistory = this.aiAssistantData.chatHistory();
      if (chatHistory.length === 1 && !chatHistory[0].isUser) {
        // First model message received in a new session
        this.loadUserSessions();
      }
    });
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
    console.log('Starting new chat...');
    this.aiAssistantData.clearConversation();
    this.selectedChatId.set(null);
  }

  openChat(chat: ChatSession) {
    console.log('Opening chat:', chat);
    this.selectedChatId.set(chat.id);
    this.aiAssistantData.switchToSession(chat.id).subscribe({
      error: (error) => console.error('Failed to switch session:', error)
    });
  }

  formatDate(dateString: string): string {
    if (!dateString) return 'Unknown';
    if (dateString === 'Invalid Date') return 'Unknown';
    let date: Date;
    try {
      date = new Date(dateString);
      if (isNaN(date.getTime())) return 'Unknown';
    } catch {
      return 'Unknown';
    }
    const now = new Date();
    const diffTime = Math.abs(now.getTime() - date.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    if (diffDays === 1) {
      return 'Today';
    } else if (diffDays === 2) {
      return 'Yesterday';
    } else if (diffDays <= 7) {
      return `${diffDays - 1} days ago`;
    } else {
      return date.toLocaleDateString();
    }
  }

  onSearchInput(event: any): void {
    const query = event.target.value;
    this.searchQuery.set(query);
  }

  clearSearch(): void {
    this.searchQuery.set('');
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
    // this.menuChat = chat; // This line is removed as per the new_code
    const menu = this.chatMenus?.toArray()[index];
    if (menu && menu.toggle) {
      menu.toggle(event);
    }
  }

  getChatMenuItems(chat: any) {
    return [
      { label: 'Rename', icon: 'pi pi-pencil', command: () => this.renameChat(chat) },
      { label: chat.starred ? 'Unstar' : 'Star', icon: chat.starred ? 'pi pi-star-fill' : 'pi pi-star', command: () => this.toggleStarChat(chat) },
      { label: chat.archived ? 'Unarchive' : 'Archive', icon: chat.archived ? 'pi pi-folder-open' : 'pi pi-archive', command: () => this.archiveChat(chat) }
    ];
  }

  renameChat(chat: any) {
    this.renameTitle = chat.title;
    this.showRenameDialog = true;
  }
  saveRename() {
    if (this.renameTitle.trim()) {
      // Simulate save: update local chatSessions
      const updated = this.chatSessions().map(c => c.title === this.renameTitle.trim() ? { ...c, title: this.renameTitle.trim() } : c);
      this.chatSessions.set(updated);
      this.showRenameDialog = false;
    }
  }
  toggleStarChat(chat: any) {
    this.confirmAction = () => {
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
    this.confirmAction = () => {
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
    this.rightPanelInjector = Injector.create({
      providers: [{ provide: 'panelData', useValue: data }],
      parent: this.injector
    });
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
      this.rightPanelInjector = null;
      this.rightPanelUrl = null;
      this.rightPanelType = null;
    }, 300);
  }
  startResizing(event: MouseEvent) {
    this.resizing = true;
    this.startX = event.clientX;
    this.startWidth = this.rightPanelWidth;
    this.document.addEventListener('mousemove', this.onResizing);
    this.document.addEventListener('mouseup', this.stopResizing);
    event.preventDefault();
  }
  onResizing = (event: MouseEvent) => {
    if (!this.resizing) return;
    const dx = event.clientX - this.startX;
    let newWidth = this.startWidth - dx;
    newWidth = Math.max(320, Math.min(newWidth, window.innerWidth * 0.7));
    this.rightPanelWidth = newWidth;
    // Force change detection to update the chat area width
    this.cdr.detectChanges();
  };


  stopResizing = () => {
    this.resizing = false;
    this.document.removeEventListener('mousemove', this.onResizing);
    this.document.removeEventListener('mouseup', this.stopResizing);
  };
  // Placeholder handlers for cardClicked/urlClicked events
  onCardClicked(event: { entityType: string, entityId: string, rowData: any }) {
    console.log('Card clicked:', event);
    this.rightPanelEntityType = event.entityType;
    this.rightPanelEntityId = event.entityId;
    this.rightPanelType = 'component';
    // For now, show coming soon instead of actual components
    this.rightPanelComponent = null;
    this.rightPanelInjector = null;
    
    // Open panel with animation
    this.rightPanelVisible = true;
    
    // Force change detection to update the layout
    this.cdr.detectChanges();
  }
  onUrlClicked(url: string | Event) {
    if (typeof url === 'string') {
      this.openRightPanelWithUrl(url);
    } else if (url && (url as any).detail) {
      this.openRightPanelWithUrl((url as any).detail);
    }
  }
  openRightPanelInNewTab() {
    if (!this.rightPanelEntityType || !this.rightPanelEntityId) return;
    let route = '';
    switch (this.rightPanelEntityType) {
      case 'partner':
        route = `/partnerships/partners/${this.rightPanelEntityId}`;
        break;
      case 'contact':
        route = `/contacts/${this.rightPanelEntityId}`;
        break;
      case 'interaction':
        route = `/interactions/${this.rightPanelEntityId}`;
        break;
      default:
        return;
    }
    window.open(route, '_blank');
  }
} 