/**
 * @fileoverview Comment Component - Reusable collaboration component for any entity
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, input, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

// PrimeNG imports
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { AvatarModule } from 'primeng/avatar';
import { ChipModule } from 'primeng/chip';
import { TooltipModule } from 'primeng/tooltip';

// Services
import { CommentService } from '@shared/services/api/comment.service';
import { FeedbackDialogService } from '@shared/services/ui';
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
    TooltipModule
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

  // State
  loading = signal<boolean>(true);
  comments = signal<Comment[]>([]);
  newCommentContent = signal<string>('');
  replyingToId = signal<number | null>(null);
  editingCommentId = signal<number | null>(null);
  editingContent = signal<string>('');

  ngOnInit(): void {
    this.loadComments();
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
   * Add a new comment
   */
  addComment(): void {
    const content = this.newCommentContent().trim();
    if (!content) return;

    const request: CommentRequest = {
      entityType: this.entityType(),
      entityId: this.entityId(),
      content: content,
      parentCommentId: this.replyingToId() || undefined
    };

    this.commentService.createComment(request).subscribe({
      next: () => {
        this.newCommentContent.set('');
        this.replyingToId.set(null);
        this.loadComments();
        this.feedbackService.showSuccessToast({
          summary: 'Success',
          detail: 'Comment added successfully'
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
}

