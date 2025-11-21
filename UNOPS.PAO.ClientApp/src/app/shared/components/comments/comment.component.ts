/**
 * @fileoverview Comment Component - Reusable collaboration component for any entity
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, OnInit, signal, inject, computed, effect, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { OverlayPanelModule } from 'primeng/overlaypanel';

// Services
import { CommentService } from '@shared/services/api/comment.service';
import { FeedbackDialogService } from '@shared/services/ui';
import { ValuesService, SimpleValue } from '@shared/services/api/values.service';
import { Comment, CommentRequest } from '@shared/models/comment.model';

/**
 * @class CommentComponent
 * @description Reusable comment/collaboration component that can be attached to any entity.
 * Supports threaded replies, @mentions, editing, and pinning.
 * 
 * @example
 * ```html
 * <app-comment 
 *   entityType="Opportunity" 
 *   [entityId]="opportunityId()">
 * </app-comment>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-comment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    AvatarModule,
    ChipModule,
    TooltipModule,
    AutoCompleteModule,
    OverlayPanelModule
  ],
  templateUrl: './comment.component.html',
  styleUrls: ['./comment.component.scss']
})
export class CommentComponent implements OnInit {
  // Inputs
  readonly entityType = input.required<string>();
  readonly entityId = input.required<number>();
  readonly panelHeader = input<string>('💬 Collaboration & Comments');

  // Services
  private readonly commentService = inject(CommentService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly valuesService = inject(ValuesService);
  private readonly translateService = inject(TranslateService);

  // State
  loading = signal<boolean>(true);
  comments = signal<Comment[]>([]);
  newCommentContent = signal<string>('');
  replyingToId = signal<number | null>(null);
  editingCommentId = signal<number | null>(null);
  editingContent = signal<string>('');
  
  // Mention functionality
  allUsers = signal<SimpleValue[]>([]);
  filteredUsers = signal<SimpleValue[]>([]);
  mentionedUserIds = signal<number[]>([]);
  showMentionSuggestions = signal<boolean>(false);
  mentionSearchTerm = signal<string>('');
  cursorPosition = signal<number>(0);

  ngOnInit(): void {
    this.loadComments();
    this.loadUsers();
  }

  /**
   * Load all comments for the entity
   */
  loadComments(): void {
    this.loading.set(true);
    this.commentService.getCommentsByEntity(this.entityType(), this.entityId(), true).subscribe({
      next: (data) => {
        this.comments.set(data);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  /**
   * Load all internal users for mentions
   */
  loadUsers(): void {
    this.valuesService.getInternalUsers().subscribe({
      next: (users) => {
        this.allUsers.set(users);
      },
      error: (error) => {
        console.error('Error loading users for mentions:', error);
      }
    });
  }

  /**
   * Handle input in comment textarea for @ mentions
   */
  onCommentInput(event: Event, isReply: boolean = false): void {
    const textarea = event.target as HTMLTextAreaElement;
    const content = textarea.value;
    const cursorPos = textarea.selectionStart;
    
    this.cursorPosition.set(cursorPos);
    
    // Check if user just typed @
    const textBeforeCursor = content.substring(0, cursorPos);
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');
    
    if (lastAtIndex !== -1) {
      const textAfterAt = textBeforeCursor.substring(lastAtIndex + 1);
      
      // Check if there's a space after @ (means they finished the mention)
      if (textAfterAt.includes(' ')) {
        this.showMentionSuggestions.set(false);
        return;
      }
      
      // Show suggestions and filter
      this.mentionSearchTerm.set(textAfterAt.toLowerCase());
      this.filterUsers(textAfterAt);
      this.showMentionSuggestions.set(true);
    } else {
      this.showMentionSuggestions.set(false);
    }
    
    // Update content based on context
    if (isReply) {
      // For reply context, we'd need to track which reply is being edited
      // This is a simplified version
    } else {
      this.newCommentContent.set(content);
    }
  }

  /**
   * Filter users based on search term
   */
  filterUsers(searchTerm: string): void {
    if (!searchTerm) {
      this.filteredUsers.set(this.allUsers().slice(0, 10)); // Show first 10
      return;
    }
    
    const filtered = this.allUsers().filter(user => 
      user.name.toLowerCase().includes(searchTerm.toLowerCase())
    ).slice(0, 10);
    
    this.filteredUsers.set(filtered);
  }

  /**
   * Select a user from mention suggestions
   */
  selectMentionUser(user: SimpleValue, textarea: HTMLTextAreaElement): void {
    const content = textarea.value;
    const cursorPos = this.cursorPosition();
    const textBeforeCursor = content.substring(0, cursorPos);
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');
    
    if (lastAtIndex !== -1) {
      const beforeAt = content.substring(0, lastAtIndex);
      const afterCursor = content.substring(cursorPos);
      // Use email (from code field) instead of name
      const userEmail = user.code || user.email || user.name;
      const newContent = `${beforeAt}@${userEmail} ${afterCursor}`;
      
      this.newCommentContent.set(newContent);
      textarea.value = newContent;
      
      // Add to mentioned users
      const currentMentions = this.mentionedUserIds();
      if (!currentMentions.includes(user.id)) {
        this.mentionedUserIds.set([...currentMentions, user.id]);
      }
      
      // Hide suggestions
      this.showMentionSuggestions.set(false);
      
      // Set cursor position after the mention
      const newCursorPos = beforeAt.length + userEmail.length + 2; // +2 for @ and space
      setTimeout(() => {
        textarea.focus();
        textarea.setSelectionRange(newCursorPos, newCursorPos);
      }, 0);
    }
  }

  /**
   * Add a new comment
   */
  addComment(): void {
    const content = this.newCommentContent().trim();
    if (!content) return;

    const request: CommentRequest = {
      entityType: this.entityType(),
      entityId: this.entityId(),
      content: content,
      parentCommentId: this.replyingToId() || undefined,
      mentionedUserIds: this.mentionedUserIds().length > 0 ? this.mentionedUserIds() : undefined
    };

    this.commentService.createComment(request).subscribe({
      next: () => {
        this.newCommentContent.set('');
        this.replyingToId.set(null);
        this.mentionedUserIds.set([]);
        this.loadComments();
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.commentAdded')
        });
      }
    });
  }

  /**
   * Start replying to a comment
   */
  startReply(commentId: number): void {
    this.replyingToId.set(commentId);
    this.editingCommentId.set(null);
  }

  /**
   * Cancel reply
   */
  cancelReply(): void {
    this.replyingToId.set(null);
    this.newCommentContent.set('');
  }

  /**
   * Start editing a comment
   */
  startEdit(comment: Comment): void {
    this.editingCommentId.set(comment.id);
    this.editingContent.set(comment.content);
    this.replyingToId.set(null);
  }

  /**
   * Save edited comment
   */
  saveEdit(commentId: number): void {
    const content = this.editingContent().trim();
    if (!content) return;

    this.commentService.updateComment({
      id: commentId,
      content: content
    }).subscribe({
      next: () => {
        this.editingCommentId.set(null);
        this.editingContent.set('');
        this.loadComments();
        this.feedbackService.showSuccessToast({
          summary: 'Success',
          detail: 'Comment updated successfully'
        });
      }
    });
  }

  /**
   * Cancel editing
   */
  cancelEdit(): void {
    this.editingCommentId.set(null);
    this.editingContent.set('');
  }

  /**
   * Delete a comment
   */
  deleteComment(commentId: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: 'Delete Comment',
        detail: 'Are you sure you want to delete this comment? This action cannot be undone.'
      },
      () => {
        this.commentService.deleteComment(commentId).subscribe({
          next: () => {
            this.loadComments();
            this.feedbackService.showSuccessToast({
              summary: 'Success',
              detail: 'Comment deleted successfully'
            });
          }
        });
      }
    );
  }

  /**
   * Toggle pin status
   */
  togglePin(commentId: number): void {
    this.commentService.togglePin(commentId).subscribe({
      next: () => {
        this.loadComments();
      }
    });
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string | undefined): string {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Get relative time (e.g., "2 hours ago")
   */
  getRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} min${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
    return this.formatDate(dateString);
  }

  /**
   * Get user by ID from the list
   */
  getUserById(userId: number): SimpleValue | undefined {
    return this.allUsers().find(u => u.id === userId);
  }

  /**
   * Remove a mentioned user
   */
  removeMentionedUser(userId: number): void {
    this.mentionedUserIds.set(this.mentionedUserIds().filter(id => id !== userId));
  }

  /**
   * Parse comment content and convert @mentions to chips
   * Returns array of text segments and mention objects
   */
  parseCommentContent(content: string): Array<{ type: 'text' | 'mention', value: string }> {
    const segments: Array<{ type: 'text' | 'mention', value: string }> = [];
    // Updated pattern to match email addresses: @email@domain.com or @FirstName.LastName@domain.com
    const mentionPattern = /@([\w\.\-]+@[\w\.\-]+)/g;
    let lastIndex = 0;
    let match;

    while ((match = mentionPattern.exec(content)) !== null) {
      // Add text before the mention
      if (match.index > lastIndex) {
        segments.push({
          type: 'text',
          value: content.substring(lastIndex, match.index)
        });
      }

      // Add the mention (email without the leading @)
      segments.push({
        type: 'mention',
        value: match[1] // The email without leading @
      });

      lastIndex = match.index + match[0].length;
    }

    // Add remaining text
    if (lastIndex < content.length) {
      segments.push({
        type: 'text',
        value: content.substring(lastIndex)
      });
    }

    return segments;
  }
}

