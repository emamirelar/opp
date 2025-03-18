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
}
