import { EntityPermissionSet } from './shared-types';

export interface BaseEngagement {
  id: number;
  // Primary identifier
  engagementNumber: string;
  
  // Date fields
  engagementImplementationStartDate?: Date;
  engagementImplementationEndDate?: Date;
  engagementSignedDate?: Date;
  
  // Financial information
  engagementAmount?: number;
  
  // Stage and status information
  engagementStage?: string;
  engagementStageDescription?: string;
  
  // Business developer information
  businessDeveloper?: string;
  businessDeveloperName?: string;
  businessDeveloperEmailAddress?: string;
  
  // Project executive information
  engagementProjectExecutive?: string;
  engagementProjectExecutiveName?: string;
  
  // Implementation details
  implementationCountriesList?: string;
  outputsList?: string;
  sdgList?: string;
  
  // Descriptions
  engagementDescription?: string;
  engagementLongDescription?: string;
  
  // Related data
  partners: BaseEngagementPartner[];
  partnerCount: number;
  
  // Audit fields
  createdBy: number;
  createdDate: Date;
  lastModifiedBy?: number;
  lastModifiedDate?: Date;
  
  // RBAC permissions
  permissions?: EntityPermissionSet;
  
  // Display helpers
  displayName: string;
  stageDisplay: string;
  durationDisplay: string;
  budgetDisplay: string;
  businessDeveloperDisplay: string;
}

export interface BaseEngagementPartner {
  id: number;
  // Primary identifier
  key: string;
  
  // Engagement reference
  engagementNumber: string;
  
  // Partner information (from source)
  partnerType?: string;
  partner?: string;
  partnerDescription?: string;
  
  // Resolved foreign key IDs
  partnerId?: number;
  baseEngagementId?: number;
  
  // Related data
  engagementDescription: string;
  partnerName: string;
  
  // Audit fields
  createdBy: number;
  createdDate: Date;
  lastModifiedBy?: number;
  lastModifiedDate?: Date;
  
  // RBAC permissions
  permissions?: EntityPermissionSet;
  
  // Display helpers
  partnerTypeDisplay: string;
  partnerDisplayName: string;
}

// Helper interface for engagement stage severity (aligned with PrimeNG p-tag severity values)
export type StageSeverity = 'success' | 'warn' | 'danger' | 'info';

// Helper interfaces for filtering and search
export interface BaseEngagementFilterParams {
  partnerId?: number;
  engagementStage?: string;
  businessDeveloper?: string;
  startDateFrom?: Date;
  startDateTo?: Date;
  endDateFrom?: Date;
  endDateTo?: Date;
  search?: string;
}

// Summary interface for display in lists
export interface BaseEngagementSummary {
  id: number;
  engagementNumber: string;
  displayName: string;
  engagementStage?: string;
  partnerCount: number;
  budgetDisplay: string;
  durationDisplay: string;
}
