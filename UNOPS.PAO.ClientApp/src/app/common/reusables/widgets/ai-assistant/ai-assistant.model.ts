export interface ChatMessage {
  text?: string;
  isUser: boolean;
  timestamp: Date;
  files: ChatFile[];
  // New properties for structured content
  result?: ResultItem[];
  entity?: string;
  suggestedUserResponses?: string[];
  sources?: Source[];
  isFromHistory?: boolean; // Flag to indicate if message is loaded from history
  inlineData?: InlineData[]; // Support for inline data like images
  // Streaming support - separate arrays for each chunk type
  streamingTypes?: {
    thoughts: any[];
    functionCall: any[];
    functionResponse: any[];
    markdown: any[];
    mermaid: any[];
    chart: any[];
    [key: string]: any[]; // Allow for additional types
  };
}

export interface InlineData {
  data: string; // Base64 encoded data
  mimeType: string; // MIME type (e.g., 'image/png', 'image/jpeg')
}

export interface Source {
  title: string;
  url: string;
  description?: string;
}

export interface ResultItem {
  type: 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card' | 'chartjs' | 'thought' | 'functionCall' | 'functionResponse' | 'chart';
  message: string | any[] | any; // string for text/markdown/code/thought, array for grid/card data, object for chartjs
  language?: string; // for code blocks
  entity?: string; // for grid/card data
  chartType?: string; // for chartjs: pie, bar, line, doughnut, etc.
  partial?: boolean; // for streaming support - indicates if this is a partial chunk that should be updated
  invocationId?: string; // unique identifier for the streaming session
  renderingId?: string; // unique identifier for rendering tracking
  completed?: boolean; // indicates if this stream item is completed (no more updates)
  timestamp?: number; // timestamp for change detection
  arrivalOrder?: number; // order in which this chunk type first appeared
}

export interface ChatFile {
  file?: File;
  name?: string;
  content?: string;
  mediaUrl?: string;
  mediaType?: string;
}

export interface AiResponse {
  entity?: string;
  intent?: string;
  message: string;
  type?: string;
  summary?: string;
  forward?: string;
  mediaUrl?: string;
  mediaType?: string;
  rawMessage?: string;
  files?: any[];   
  url?: string;
}

export interface SuggestionsResponse {
  suggestions: string[];
  user_id: number;
  total_actions_found: number;
}

export interface SuggestionItem {
  text: string;
  icon: string;
  action: () => void;
}

export enum ScreenToOpenByAiActionCategory {
  "Contact" = "contacts",
  "Partner" = "partners",
  "Interaction" = "interactions",
  "PartnerTree" = "partner-tree"
}

export function getUrlPageByAiResponseCategory(category: string | undefined): string | null {
  if (!category || !(category in ScreenToOpenByAiActionCategory)) {
    console.error("URL not found for category : " + category)
    return null;
  }
  return ScreenToOpenByAiActionCategory[category as keyof typeof ScreenToOpenByAiActionCategory];
}
