// UNIFIED MODEL STRUCTURE - Single model for all AI Assistant data operations
// Whether processing new messages, handling streaming responses, or loading session history

export interface ChatSession {
  session: {
    id: string;
    timestamp: number; // Numeric timestamp
    userId: number;
    status: string;
    title: string;
    starred: boolean;
    archived: boolean;
  };
  chatMessages: ChatMessage[];
}

export interface ChatMessage {
  // Core message properties
  id: string;
  timestamp: number; // Numeric timestamp
  invocationId: string;
  role: "user" | "model"; // Primary role identifier
  
  // Content structure (matches streaming response format)
  content: {
    parts: ContentPart[];
    role: "user" | "model";
  };
  
  // Actions and metadata
  actions: {
    stateDelta: any;
    artifactDelta: any;
    requestedAuthConfigs: any;
  };
  longRunningToolIds: string[];
  
  // UI-specific properties (computed from role)
  isUser?: boolean; // Computed from role === "user"
  files?: ChatFile[];
  sources?: Source[];
  suggestedUserResponses?: string[];
}

export interface ContentPart {
  text?: string;
  thought?: boolean;
  thoughtSignature?: string;
  functionCall?: {
    id: string;
    name: string;
    args: any;
  };
  functionResponse?: {
    id: string;
    name: string;
    response: any;
  };
  content?: any; // Content data for processing
  timestamp?: number; // Timestamp for ordering
  
  // Dynamic content properties (populated by dynamic content service)
  type?: 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card' | 'chartjs' | 'thought' | 'thoughts' | 'functionCall' | 'functionResponse' | 'chart' | 'user-message';
  entity?: any; // Entity data for grid/card components
  completed?: boolean; // Flag to indicate if content is complete (for progressive rendering)
  partial?: boolean; // Flag to indicate if this part is still streaming/partial
  
  // Additional properties needed for dynamic content processing
  entityType?: string; // Entity type for grid/card components
  invocationId?: string; // Invocation ID for component tracking
  renderingId?: string; // Rendering ID for component management
  isUserMessage?: boolean; // Flag to indicate if this is a user message
  files?: ChatFile[]; // Attached files for user messages
  
  // NOTE: ContentPart does NOT have a role - only the content object has a role
}

// Supporting interfaces
export interface ChatFile {
  file?: File;
  name?: string;
  content?: string;
  mediaUrl?: string;
  mediaType?: string;
  gcsPath?: string; // GCS storage path for uploaded files
}

export interface Source {
  title: string;
  url: string;
  description?: string;
}
