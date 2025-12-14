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
 * @interface SectionLoadEvent
 * @description Event emitted by child components when their loading state changes
 */
export interface SectionLoadEvent {
  /** Section identifier */
  sectionKey: keyof LoadingProgress['sections'];
  /** New loading status */
  status: LoadingSectionStatus['status'];
  /** Optional message */
  message?: string;
  /** Optional error details */
  error?: string;
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
    opportunity: { status: 'pending', label: 'Opportunity Data' },
    insights: { status: 'pending', label: 'AI Insights' },
    analysis: { status: 'pending', label: 'Analysis' },
    dstRisks: { status: 'pending', label: 'Risk Assessment' },
    dstRecommendations: { status: 'pending', label: 'AI Recommendations' },
    dstSimilarOpportunities: { status: 'pending', label: 'Similar Opportunities' },
    dstSimilarProjects: { status: 'pending', label: 'Similar Projects' },
    dstRelevantPeople: { status: 'pending', label: 'Relevant People' },
    relatedItems: { status: 'pending', label: 'Related Items' },
    documents: { status: 'pending', label: 'Documents' },
  },
};

