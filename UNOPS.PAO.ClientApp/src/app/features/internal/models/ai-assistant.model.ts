export interface AiAssistantSessionRequest {
    sessionId?: string;
  }

  export interface AiAssistantRequest {
    message?: string;
    sessionId?: string;
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
  }
