export interface ChatMessage {
  text?: string;
  isUser: boolean;
  timestamp: Date;
  files: ChatFile[];
  // New properties for structured content
  result?: ResultItem[];
  entity?: string;
  followUps?: string[];
}

export interface ResultItem {
  type: 'markdown' | 'mermaid' | 'code' | 'text' | 'grid' | 'card';
  message: string | any[]; // string for text/markdown/code, array for grid/card data
  language?: string; // for code blocks
  entity?: string; // for grid/card data
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
