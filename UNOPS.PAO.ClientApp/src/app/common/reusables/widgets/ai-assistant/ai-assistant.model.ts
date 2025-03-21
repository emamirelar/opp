export interface ChatMessage {
  text: string;
  isUser: boolean;
  timestamp: Date;
  files?: ChatFile[];
}

export interface ChatFile {
  name: string;
  content: string;
}

export interface AiResponse {
  Entity?: string;
  Intent?: string;
  Message: string;
  Type?: string;
  Summary?: string;
  Forward?: string;
  Category?: string;
  ResponseType?: string;
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
