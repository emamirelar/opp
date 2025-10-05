export type GeminiType = 'contacts_summary' | 'partner_interactions_summary' | 'partner_risk_profile' | 'partner_news';

export interface GeminiPart {
  text: string;
}

export interface GeminiContent {
  role: string;
  parts: GeminiPart[];
}

export interface SafetyRating {
  category: string;
  probability: string;
  probabilityScore: number;
  severity: string;
  severityScore: number;
}

export interface GeminiCandidate {
  content: GeminiContent;
  finishReason: string;
  safetyRatings: SafetyRating[];
  avgLogprobs: number;
}

export interface GeminiResponse {
  candidates: GeminiCandidate[];
  usageMetadata: {
    promptTokenCount: number;
    candidatesTokenCount: number;
    totalTokenCount: number;
  };
  modelVersion: string;
  responseId: string;
}

