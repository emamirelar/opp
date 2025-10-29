/**
 * Model for displaying a comment
 */
export interface Comment {
  id: number;
  entityType: string;
  entityId: number;
  content: string;
  parentCommentId?: number;
  mentionedUserNames?: string[];
  isEdited: boolean;
  isPinned: boolean;
  
  // Audit fields
  createdDate: string;
  createdBy: number;
  createdByName?: string;
  lastModifiedDate?: string;
  lastModifiedBy?: number;
  lastModifiedByName?: string;
  
  // Navigation properties
  replies?: Comment[];
}

/**
 * Request model for creating a new comment
 */
export interface CommentRequest {
  entityType: string;
  entityId: number;
  content: string;
  parentCommentId?: number;
  mentionedUserIds?: number[];
}

/**
 * Request model for updating an existing comment
 */
export interface UpdateCommentRequest {
  id: number;
  content: string;
  mentionedUserIds?: number[];
}

