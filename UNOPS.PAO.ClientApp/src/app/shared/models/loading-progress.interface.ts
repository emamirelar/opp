/**
 * @fileoverview Loading Progress Interfaces for Opportunity View
 * @author UNOPS Opportunity+ System Development Team
 */

/**
 * @interface LoadingSectionStatus
 * @description Status tracking for individual section loading state
 */
export interface LoadingSectionStatus {
  /** Current loading status */
  status: 'pending' | 'loading' | 'completed' | 'error';
  /** Human-readable label for the section */
  label: string;
  /** Optional error message if loading failed */
  error?: string;
  /** Timestamp when loading started */
  startTime?: number;
  /** Timestamp when loading completed */
  endTime?: number;
}

/**
 * @interface LoadingProgress
 * @description Overall loading progress tracking for opportunity view
 */
export interface LoadingProgress {
  /** Total number of sections to load */
  total: number;
  /** Number of sections completed loading */
  completed: number;
  /** Currently loading section name */
  currentSection: string;
  /** Detailed status for each section */
  sections: {
    opportunity: LoadingSectionStatus;
    insights: LoadingSectionStatus;
    analysis: LoadingSectionStatus;
    dstRisks: LoadingSectionStatus;
    dstRecommendations: LoadingSectionStatus;
    dstSimilarOpportunities: LoadingSectionStatus;
    dstSimilarProjects: LoadingSectionStatus;
    dstRelevantPeople: LoadingSectionStatus;
    relatedItems: LoadingSectionStatus;
    documents: LoadingSectionStatus;
  };
}

/**
 * @type LoadingSectionKey
 * @description Union type of all section keys for type safety
 */
export type LoadingSectionKey = keyof LoadingProgress['sections'];

/**
 * @const DEFAULT_LOADING_PROGRESS
 * @description Default loading progress state
 */
export const DEFAULT_LOADING_PROGRESS: LoadingProgress = {
  total: 10,
  completed: 0,
  currentSection: '',
  sections: {
    opportunity: { status: 'pending', label: 'section.opportunityData' },
    insights: { status: 'pending', label: 'section.aiInsights' },
    analysis: { status: 'pending', label: 'section.analysis' },
    dstRisks: { status: 'pending', label: 'section.riskAssessment' },
    dstRecommendations: { status: 'pending', label: 'section.recommendations' },
    dstSimilarOpportunities: { status: 'pending', label: 'section.similarOpportunities' },
    dstSimilarProjects: { status: 'pending', label: 'section.similarProjects' },
    dstRelevantPeople: { status: 'pending', label: 'section.relevantPeople' },
    relatedItems: { status: 'pending', label: 'section.relatedItems' },
    documents: { status: 'pending', label: 'section.documents' },
  },
};

