export interface AiAssistantSessionRequest {
    sessionId?: string;
  }

  export interface AiAssistantRequest {
    file?: File,
    message?: string;
    sessionId?: string;
  }

  // Enhanced file upload interfaces
  export interface FileUpload {
    file: File;
    id: string;
    name: string;
    size: number;
    type: string;
    uploadProgress?: number;
    status?: 'pending' | 'uploading' | 'completed' | 'error';
    preview?: string; // For image previews
  }

  export interface AiAssistantRequestWithFiles extends AiAssistantRequest {
    files?: FileUpload[];
    state?: any;
  }

  export interface FileValidationResult {
    valid: File[];
    invalid: { file: File; error: string }[];
  }

  export interface ChatRequestData {
    message: string;
    sessionId?: string;
    files?: File[];
    gcsFiles?: { gcsPath: string; name: string; mimeType?: string }[]; // GCS-uploaded files
    state?: any;
  }

  export interface SessionResponse {
    sessionId?: string;
  }

  export interface SessionData {
    id?: string;
    userId?: number;
    startTime?: string;
    endTime?: string | null;
    status?: string;
    chats?: ChatHistoryItem[];
    lastUpdated?: string;
    title?: string;
    starred?: boolean;
    archived?: boolean;
  }

  export type Sender = 'model' | 'user';

  export interface ChatHistoryItem {
    id?: number;
    sessionId?: string;
    sender?: Sender;
    message?: string;
    timestamp?: string;
    entity?: string;
    intent?: string;
    // Enhanced with file attachment support
    attachments?: FileAttachment[];
  }

  export interface FileAttachment {
    name: string;
    type: string;
    size: number;
    processed: boolean;
    url?: string;
  }
