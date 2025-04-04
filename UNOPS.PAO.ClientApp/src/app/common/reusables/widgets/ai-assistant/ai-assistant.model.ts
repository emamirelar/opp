export interface ChatMessage {
  text: string;
  isUser: boolean;
  timestamp: Date;
  files?: ChatFile[];
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
