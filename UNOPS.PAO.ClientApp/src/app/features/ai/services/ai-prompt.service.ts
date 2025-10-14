import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaginationParams } from '@shared/models/pagination-params.model';
import { PaginationResponse } from '@shared/models/pagination-response.model';

export interface AiPrompt {
  id: number;
  type: string;
  
  // NEW: Enhanced structure
  dataRetrievalMethod?: string;
  systemInstructions?: string;
  userPrompt?: string;
  feature?: string;
  
  // Existing fields
  description?: string;
  name?: string;
  createdAt: Date;
  generationConfig: string;
  contentConfig: string;
  toolsConfig?: string;
  safetySettings?: string;
  project: string;
  location: string;
  model: string;
  
  // NEW: Caching configuration
  useCache?: boolean;
  cacheInvalidationMinutes?: number;
  
  // LEGACY: Keep for backward compatibility
  promptFunction?: string;
  prompt?: string;
}

export interface GeminiModel {
  value: string;
  label: string;
  location: string;
  maxTokens: number;
}

export interface GenerationConfig {
  temperature?: number;
  top_p?: number;
  max_output_tokens?: number;
}

export interface ToolsConfig {
  googleSearch?: boolean;
}

export interface AiPromptFilterRequest extends PaginationParams {
  searchText?: string;
}

export interface TestPromptRequest {
  type: string;
  id?: number;
  testData?: string;
  // NEW: Enhanced structure
  dataRetrievalMethod?: string;
  systemInstructions?: string;
  userPrompt?: string;
  model?: string;
  project?: string;
  location?: string;
  temperature?: number;
  topP?: number;
  maxOutputTokens?: number;
  googleSearch?: boolean;
  safetySettings?: string;
  // LEGACY: Keep for backward compatibility
  prompt?: string;
}

export interface TestPromptResponse {
  success: boolean;
  response?: string;
  error?: string;
  dataRetrievalResult?: string; // JSON data retrieved by the data retrieval method
}

export interface GeminiModelUpgradeResult {
  success: boolean;
  updatedCount: number;
  message: string;
  latestModel?: string;
  alreadyLatest: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AiPromptService {
  private baseUrl = '/api/ai-prompt-management';

  constructor(private http: HttpClient) {}

  /**
   * Gets paginated list of AI prompts
   */
  getPrompts(request: any): Observable<HttpResponse<PaginationResponse<AiPrompt>>> {
    return this.http.post<PaginationResponse<AiPrompt>>(`${this.baseUrl}/list`, request, {
      observe: 'response'
    });
  }

  /**
   * Gets a specific AI prompt by ID
   */
  getPromptById(id: number): Observable<HttpResponse<AiPrompt>> {
    return this.http.get<AiPrompt>(`${this.baseUrl}/${id}`, {
      observe: 'response'
    });
  }

  /**
   * Creates a new AI prompt
   */
  createPrompt(prompt: AiPrompt): Observable<HttpResponse<AiPrompt>> {
    return this.http.post<AiPrompt>(this.baseUrl, prompt, {
      observe: 'response'
    });
  }

  /**
   * Updates an existing AI prompt
   */
  updatePrompt(id: number, prompt: AiPrompt): Observable<HttpResponse<AiPrompt>> {
    return this.http.put<AiPrompt>(`${this.baseUrl}/${id}`, prompt, {
      observe: 'response'
    });
  }

  /**
   * Deletes an AI prompt
   */
  deletePrompt(id: number): Observable<HttpResponse<any>> {
    return this.http.delete(`${this.baseUrl}/${id}`, {
      observe: 'response'
    });
  }

  /**
   * Gets prompts by type
   */
  getPromptsByType(type: string): Observable<HttpResponse<AiPrompt[]>> {
    return this.http.get<AiPrompt[]>(`${this.baseUrl}/type/${type}`, {
      observe: 'response'
    });
  }

  /**
   * Gets unique prompt types for dropdown/filter
   */
  getPromptTypes(): Observable<HttpResponse<string[]>> {
    return this.http.get<string[]>(`${this.baseUrl}/types`, {
      observe: 'response'
    });
  }

  /**
   * Gets unique models for dropdown/filter
   */
  getModels(): Observable<HttpResponse<string[]>> {
    return this.http.get<string[]>(`${this.baseUrl}/models`, {
      observe: 'response'
    });
  }

  /**
   * Gets unique projects for dropdown/filter
   */
  getProjects(): Observable<HttpResponse<string[]>> {
    return this.http.get<string[]>(`${this.baseUrl}/projects`, {
      observe: 'response'
    });
  }

  /**
   * Gets unique locations for dropdown/filter
   */
  getLocations(): Observable<HttpResponse<string[]>> {
    return this.http.get<string[]>(`${this.baseUrl}/locations`, {
      observe: 'response'
    });
  }

  /**
   * Gets available Gemini models
   */
  getGeminiModels(): Observable<HttpResponse<GeminiModel[]>> {
    return this.http.get<GeminiModel[]>('/api/values/gemini-models', {
      observe: 'response'
    });
  }

  /**
   * Tests an AI prompt with provided test data
   */
  testPrompt(request: TestPromptRequest): Observable<HttpResponse<TestPromptResponse>> {
    return this.http.post<TestPromptResponse>(`${this.baseUrl}/test`, request, {
      observe: 'response'
    });
  }

  /**
   * Upgrades all AI prompts to the latest available Gemini model
   */
  upgradeGeminiModel(): Observable<HttpResponse<GeminiModelUpgradeResult>> {
    return this.http.post<GeminiModelUpgradeResult>(`${this.baseUrl}/upgrade-model`, {}, {
      observe: 'response'
    });
  }

  /**
   * Exports all AI prompts as a SQL script file for seeding
   */
  exportAiPromptsAsSql(): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/export-sql`, {
      responseType: 'blob'
    });
  }
} 
