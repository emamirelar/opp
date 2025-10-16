export interface TourStep {
  element?: string;
  popover: {
    title: string;
    description: string;
    side: 'top' | 'right' | 'bottom' | 'left' | 'over';
    align: 'start' | 'center' | 'end';
  };
  options?: {
    selectors: string[];
    stepNumber: number;
  };
}

export interface TourConfig {
  tourId: string;
  title: string;
  description: string;
  entity: string;
  route: string;
  showButtons: string[];
  allowClose: boolean;
  overlayClickNext: boolean;
  popoverOffset: number;
  steps: TourStep[];
  generatedAt?: string;
  version?: string;
}

export interface TourProgress {
  tourId: string;
  completed: boolean;
  currentStep: number;
  completedAt?: Date;
  skipped?: boolean;
}

export interface TourPreferences {
  autoStart: boolean;
  completedTours: string[];
  skippedTours: string[];
  showOnboarding: boolean;
  tourProgress: Record<string, TourProgress>;
}

export enum TourTrigger {
  AUTO = 'auto',
  MANUAL = 'manual',
  ONBOARDING = 'onboarding',
  FEATURE_ANNOUNCEMENT = 'feature_announcement',
  HELP_REQUEST = 'help_request'
}

export interface TourEvent {
  tourId: string;
  trigger: TourTrigger;
  stepIndex: number;
  action: 'start' | 'next' | 'previous' | 'skip' | 'complete' | 'close';
  timestamp: Date;
}
