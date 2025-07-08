import { Component, OnInit, OnDestroy, inject, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';
import { LayoutService } from '../../../common/layouts/services/layout.service';
import { AiAssistantData } from '../../../common/reusables/widgets/ai-assistant/ai-assistant.data';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-ai-sidebar',
  templateUrl: './ai-sidebar.component.html',
  styleUrls: ['./ai-sidebar.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    InputTextModule,
    TooltipModule,
    TranslatePipe
  ]
})
export class AiSidebarComponent implements OnInit, OnDestroy {
  public layoutService = inject(LayoutService);
  public aiAssistantData = inject(AiAssistantData);

  // Search functionality
  searchQuery = signal('');
  searchQueryValue = '';
  
  // Sessions
  sessions = signal<any[]>([]);
  selectedSessionId = signal<string | null>(null);
  
  // Computed properties
  isAiAssistantActive = computed(() => this.layoutService.layoutState().aiAssistantActive ?? true);
  isSidebarCollapsed = computed(() => this.layoutService.aiAssistantSidebarCollapsed() ?? false);
  
  filteredSessions = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const allSessions = this.sessions();
    
    if (!query) {
      return allSessions;
    }
    
    return allSessions.filter(session => 
      session.title?.toLowerCase().includes(query) || 
      session.sessionId?.toLowerCase().includes(query)
    );
  });

  constructor() {
    // Watch user sessions
    effect(() => {
      const userSessions = this.aiAssistantData.userSessions();
      this.sessions.set(userSessions);
    });
  }

  ngOnInit(): void {
    this.loadSessions();
  }

  ngOnDestroy(): void {
    // Cleanup if needed
  }

  loadSessions(): void {
    this.aiAssistantData.loadUserSessions().subscribe({
      next: () => {
        // Sessions will be updated through the effect watching userSessions()
      },
      error: (error) => {
        console.error('Error loading sessions:', error);
      }
    });
  }

  onSearchChange(event: any): void {
    const value = event.target.value;
    this.searchQueryValue = value;
    this.searchQuery.set(value);
  }

  selectSession(session: any): void {
    this.selectedSessionId.set(session.sessionId);
    // Use the correct method to switch to session
    this.aiAssistantData.switchToSession(session.sessionId).subscribe({
      error: (error) => console.error('Failed to switch session:', error)
    });
  }

  getSessionDate(session: any): string {
    if (!session.createdAt) return 'No date';
    return new Date(session.createdAt).toLocaleDateString();
  }

  getStarClasses(session: any): string {
    return session.isStarred ? 'pi-star-fill text-yellow-500' : 'pi-star text-gray-400';
  }

  toggleSessionStar(session: any, event: Event): void {
    event.stopPropagation();
    // Use the correct method to update session star
    this.aiAssistantData.updateSessionStar(session.sessionId, !session.isStarred).subscribe({
      error: (error) => console.error('Failed to toggle star:', error)
    });
  }

  toggleSessionArchive(session: any, event: Event): void {
    event.stopPropagation();
    // Use the correct method to update session archive
    this.aiAssistantData.updateSessionArchive(session.sessionId, !session.isArchived).subscribe({
      error: (error) => console.error('Failed to toggle archive:', error)
    });
  }

  // Track by function for ngFor
  trackBySessionId(index: number, session: any): string {
    return session.sessionId || session.id || index.toString();
  }
} 