/**
 * @fileoverview Comment Service - Handles API calls for comment management
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Comment, CommentRequest, UpdateCommentRequest } from '@shared/models/comment.model';

/**
 * @class CommentService
 * @description Service for managing comments on any entity in the system
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/comment';

  /**
   * Get all comments for a specific entity
   */
  getCommentsByEntity(entityType: string, entityId: number, includeReplies: boolean = true): Observable<Comment[]> {
    return this.http.get<Comment[]>(`${this.baseUrl}/${entityType}/${entityId}?includeReplies=${includeReplies}`);
  }

  /**
   * Get a specific comment by ID
   */
  getCommentById(id: number): Observable<Comment> {
    return this.http.get<Comment>(`${this.baseUrl}/${id}`);
  }

  /**
   * Create a new comment
   */
  createComment(request: CommentRequest): Observable<Comment> {
    return this.http.post<Comment>(this.baseUrl, request);
  }

  /**
   * Update an existing comment
   */
  updateComment(request: UpdateCommentRequest): Observable<Comment> {
    return this.http.put<Comment>(`${this.baseUrl}/${request.id}`, request);
  }

  /**
   * Delete a comment
   */
  deleteComment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * Toggle pin status of a comment
   */
  togglePin(id: number): Observable<{ isPinned: boolean }> {
    return this.http.post<{ isPinned: boolean }>(`${this.baseUrl}/${id}/toggle-pin`, {});
  }

  /**
   * Get comment count for an entity
   */
  getCommentCount(entityType: string, entityId: number): Observable<{ count: number }> {
    return this.http.get<{ count: number }>(`${this.baseUrl}/${entityType}/${entityId}/count`);
  }
}

