/**
 * @fileoverview Demo service providing dummy opportunity data for UI option testing
 * @author UNOPS Opportunity+ System Development Team
 */

import { Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

/**
 * Complete opportunity model for demo purposes
 */
export interface DemoOpportunity {
  id: number;
  name: string;
  description: string;
  partnerReference: string;
  status: string;
  workflowStage: string;
  responsibleOrgUnit: string;
  initiativeBudgetUSD: number;
  proposedInitiativeType: 'Project' | 'Programme' | 'Portfolio';
  targetSigningDate: string;
  targetDeliveryDate: string;

  // Deliverables
  deliverables: DemoDeliverable[];

  // Partners
  fundingPartners: DemoFundingPartner[];
  clientPartners: DemoClientPartner[];

  // Team & Stakeholders
  opportunityManager: DemoTeamMember;
  internalTeam: DemoTeamMember[];
  externalStakeholders: DemoExternalStakeholder[];

  // Impact
  sdgAlignment: DemoSDGAlignment[];
  expectedBeneficiaries: string;
  expectedOutcomes: string;

  // Geography
  implementationCountries: DemoCountry[];

  // Documents
  documents: DemoDocument[];

  // Related Items
  relatedContacts: DemoRelatedContact[];
  relatedPartners: DemoRelatedPartner[];
  relatedInteractions: DemoRelatedInteraction[];

  // DST Analysis
  dstAnalysis: DemoDSTAnalysis;

  // Activity
  comments: DemoComment[];

  // Metadata
  createdDate: string;
  createdBy: string;
  lastModifiedDate: string;
  lastModifiedBy: string;
  completeness: number;
}

export interface DemoDeliverable {
  id: number;
  name: string;
  description: string;
  serviceLine: string;
}

export interface DemoFundingPartner {
  id: number;
  name: string;
  amount: number;
  currency: string;
  percentage: number;
  feePercentage: number;
  feeAmount: number;
  partnershipAgreement?: string;
  commitmentStatus: string;
}

export interface DemoClientPartner {
  id: number;
  name: string;
  type: string;
  country: string;
}

export interface DemoTeamMember {
  id: number;
  name: string;
  role: string;
  email: string;
}

export interface DemoExternalStakeholder {
  id: number;
  name: string;
  organization: string;
  role: string;
  email: string;
}

export interface DemoSDGAlignment {
  sdgNumber: number;
  sdgName: string;
  priority: 'Primary' | 'Secondary';
  contribution: string;
}

export interface DemoCountry {
  id: number;
  name: string;
  code: string;
  flag: string;
  specificAreas: string;
  contextWarning?: string;
  riskScore?: number;
}

export interface DemoDocument {
  id: number;
  name: string;
  type: 'user-uploaded' | 'system-generated';
  category: string;
  uploadDate: string;
  size?: string;
  aiProcessed?: boolean;
  fileType?: string;
}

export interface DemoRelatedContact {
  id: number;
  name: string;
  email: string;
  organization: string;
  role: string;
  lastContact: string;
}

export interface DemoRelatedPartner {
  id: number;
  name: string;
  type: string;
  relationship: string;
  engagementLevel: string;
}

export interface DemoRelatedInteraction {
  id: number;
  type: 'meeting' | 'call' | 'email' | 'visit';
  title: string;
  date: string;
  participants: string;
}

export interface DemoDSTAnalysis {
  overallScore: number;
  complexity: string;
  lastUpdated: string;
  risks: DemoRisk[];
  opportunities: string[];
  recommendations: DemoRecommendation[];
  similarOpportunities: DemoSimilarOpportunity[];
}

export interface DemoRisk {
  id: number;
  severity: 'High' | 'Medium' | 'Low';
  title: string;
  description: string;
  source: string;
  recommendation: string;
}

export interface DemoRecommendation {
  id: number;
  title: string;
  rationale: string;
  accepted: boolean;
}

export interface DemoSimilarOpportunity {
  id: number;
  name: string;
  relevance: number;
  budget: number;
  duration: string;
  status: string;
  keyLessons: string;
}

export interface DemoComment {
  id: number;
  author: string;
  timestamp: string;
  content: string;
  mentions?: string[];
}

/**
 * @class OpportunityDemoService
 * @description Service providing comprehensive dummy data for testing opportunity UI options
 *
 * @example
 * ```typescript
 * export class MyComponent {
 *   private demoService = inject(OpportunityDemoService);
 *   opportunity = signal<DemoOpportunity | null>(null);
 *
 *   ngOnInit() {
 *     this.demoService.getDemoOpportunity().subscribe(opp => {
 *       this.opportunity.set(opp);
 *     });
 *   }
 * }
 * ```
 *
 * @since 1.0.0
 */
@Injectable({
  providedIn: 'root',
})
export class OpportunityDemoService {
  private demoOpportunity: DemoOpportunity = {
    id: 12345,
    name: 'Water Infrastructure Initiative - South Asia',
    description:
      'Comprehensive water infrastructure development program targeting rural communities in South Asia. Focus on sustainable water access, local capacity building, and long-term maintenance systems. Includes construction of 50 water points, training of 200 community technicians, and establishment of monitoring systems.',
    partnerReference: 'ABC-2024-001',
    status: 'Draft',
    workflowStage: 'Profiling',
    responsibleOrgUnit: 'Regional Office - Asia Pacific',
    initiativeBudgetUSD: 2500000,
    proposedInitiativeType: 'Project',
    targetSigningDate: '2025-09-15',
    targetDeliveryDate: '2027-12-31',

    deliverables: [
      {
        id: 1,
        name: 'Infrastructure Development',
        description:
          'Construction of 50 water points across implementation regions with sustainable design and community ownership model.',
        serviceLine: 'Infrastructure / Water & Sanitation',
      },
      {
        id: 2,
        name: 'Capacity Building',
        description:
          'Training of 200 community technicians in water system operation, maintenance, and basic repairs. Includes certification program and ongoing support.',
        serviceLine: 'Capacity Development / Technical Training',
      },
      {
        id: 3,
        name: 'Monitoring & Evaluation',
        description:
          'Establishment of quarterly assessment system for water quality, system functionality, and community satisfaction tracking.',
        serviceLine: 'Project Management / M&E',
      },
    ],

    fundingPartners: [
      {
        id: 1,
        name: 'World Bank',
        amount: 1800000,
        currency: 'USD',
        percentage: 72,
        feePercentage: 7,
        feeAmount: 126000,
        partnershipAgreement: 'UNOPS-WorldBank-MOU-2023',
        commitmentStatus: 'Confirmed',
      },
      {
        id: 2,
        name: 'European Commission',
        amount: 700000,
        currency: 'EUR',
        percentage: 28,
        feePercentage: 5,
        feeAmount: 35000,
        commitmentStatus: 'Confirmed',
      },
    ],

    clientPartners: [
      {
        id: 1,
        name: 'Ministry of Water Resources',
        type: 'Government Ministry',
        country: 'Bangladesh',
      },
    ],

    opportunityManager: {
      id: 1,
      name: 'Sarah Chen',
      role: 'Partnerships Lead',
      email: 'sarah.chen@unops.org',
    },

    internalTeam: [
      {
        id: 2,
        name: 'James Wilson',
        role: 'Infrastructure Specialist',
        email: 'james.wilson@unops.org',
      },
      {
        id: 3,
        name: 'Maria Garcia',
        role: 'Budget & Reporting',
        email: 'maria.garcia@unops.org',
      },
    ],

    externalStakeholders: [
      {
        id: 1,
        name: 'Dr. Ahmed Hassan',
        organization: 'Ministry of Water Resources',
        role: 'Government Director',
        email: 'ahmed.hassan@gov.bd',
      },
      {
        id: 2,
        name: 'Lisa Park',
        organization: 'WaterAid NGO',
        role: 'NGO Director',
        email: 'lisa.park@wateraid.org',
      },
    ],

    sdgAlignment: [
      {
        sdgNumber: 6,
        sdgName: 'Clean Water & Sanitation',
        priority: 'Primary',
        contribution: 'High contribution',
      },
      {
        sdgNumber: 13,
        sdgName: 'Climate Action',
        priority: 'Secondary',
        contribution: 'Medium contribution',
      },
      {
        sdgNumber: 17,
        sdgName: 'Partnerships for the Goals',
        priority: 'Secondary',
        contribution: 'Medium contribution',
      },
    ],

    expectedBeneficiaries: '500,000 people in rural communities',
    expectedOutcomes:
      'Improved access to clean water, enhanced local capacity for maintenance, sustainable water management systems',

    implementationCountries: [
      {
        id: 1,
        name: 'Bangladesh',
        code: 'BD',
        flag: '🇧🇩',
        specificAreas: 'Chittagong, Sylhet',
        contextWarning: 'Fragile state context',
        riskScore: 6.2,
      },
      {
        id: 2,
        name: 'Nepal',
        code: 'NP',
        flag: '🇳🇵',
        specificAreas: 'Kathmandu Valley',
        contextWarning: 'Post-disaster recovery',
      },
      {
        id: 3,
        name: 'Myanmar',
        code: 'MM',
        flag: '🇲🇲',
        specificAreas: 'Yangon, Mandalay',
        contextWarning: 'High complexity context',
        riskScore: 7.8,
      },
    ],

    documents: [
      {
        id: 1,
        name: 'Concept Note v2.pdf',
        type: 'user-uploaded',
        category: 'Planning',
        uploadDate: '2025-01-10',
        size: '2.3 MB',
        aiProcessed: true,
        fileType: 'pdf',
      },
      {
        id: 2,
        name: 'Partner Correspondence.docx',
        type: 'user-uploaded',
        category: 'Communication',
        uploadDate: '2025-01-12',
        size: '145 KB',
        aiProcessed: true,
        fileType: 'docx',
      },
      {
        id: 3,
        name: 'Budget Template.xlsx',
        type: 'user-uploaded',
        category: 'Financial',
        uploadDate: '2025-01-15',
        size: '892 KB',
        aiProcessed: false,
        fileType: 'xlsx',
      },
      {
        id: 4,
        name: 'Risk Assessment.pdf',
        type: 'user-uploaded',
        category: 'Risk Management',
        uploadDate: '2025-01-18',
        size: '1.7 MB',
        aiProcessed: true,
        fileType: 'pdf',
      },
      {
        id: 5,
        name: 'Strategic Plan 2024.pdf',
        type: 'user-uploaded',
        category: 'Strategic',
        uploadDate: '2025-01-20',
        size: '5.2 MB',
        aiProcessed: false,
        fileType: 'pdf',
      },
      {
        id: 6,
        name: 'DST Profile Report.pdf',
        type: 'system-generated',
        category: 'Analysis',
        uploadDate: '2025-01-22',
        size: '423 KB',
        aiProcessed: false,
        fileType: 'pdf',
      },
      {
        id: 7,
        name: 'Draft Budget v1.0.xlsx',
        type: 'system-generated',
        category: 'Financial',
        uploadDate: '2025-01-22',
        size: '1.1 MB',
        aiProcessed: false,
        fileType: 'xlsx',
      },
      {
        id: 8,
        name: 'Draft Risk Register.xlsx',
        type: 'system-generated',
        category: 'Risk Management',
        uploadDate: '2025-01-22',
        size: '678 KB',
        aiProcessed: false,
        fileType: 'xlsx',
      },
    ],

    relatedContacts: [
      {
        id: 1,
        name: 'Dr. Ahmed Hassan',
        email: 'ahmed.hassan@gov.bd',
        organization: 'Ministry of Water Resources - Bangladesh',
        role: 'Director of Planning',
        lastContact: '2025-01-20',
      },
      {
        id: 2,
        name: 'Lisa Park',
        email: 'lisa.park@wateraid.org',
        organization: 'WaterAid International',
        role: 'Program Director',
        lastContact: '2025-01-18',
      },
      {
        id: 3,
        name: 'Michael Chen',
        email: 'mchen@worldbank.org',
        organization: 'World Bank',
        role: 'Senior Water Specialist',
        lastContact: '2025-01-15',
      },
      {
        id: 4,
        name: 'Priya Sharma',
        email: 'priya.sharma@nepalwater.gov.np',
        organization: 'Department of Water Supply - Nepal',
        role: 'Technical Advisor',
        lastContact: '2025-01-10',
      },
      {
        id: 5,
        name: 'Thomas Mueller',
        email: 'thomas.mueller@ec.europa.eu',
        organization: 'European Commission',
        role: 'Development Cooperation Officer',
        lastContact: '2025-01-08',
      },
      {
        id: 6,
        name: 'Fatima Al-Rashid',
        email: 'fatima@unicef.org',
        organization: 'UNICEF Regional Office',
        role: 'WASH Specialist',
        lastContact: '2025-01-05',
      },
      {
        id: 7,
        name: 'Kofi Mensah',
        email: 'k.mensah@africandevbank.org',
        organization: 'African Development Bank',
        role: 'Infrastructure Lead',
        lastContact: '2024-12-20',
      },
      {
        id: 8,
        name: 'Maria Rodriguez',
        email: 'maria.rodriguez@iadb.org',
        organization: 'Inter-American Development Bank',
        role: 'Water Sector Specialist',
        lastContact: '2024-12-15',
      },
    ],

    relatedPartners: [
      {
        id: 1,
        name: 'World Bank',
        type: 'Funding Partner',
        relationship: 'Active Partnership',
        engagementLevel: 'High',
      },
      {
        id: 2,
        name: 'European Commission',
        type: 'Funding Partner',
        relationship: 'Active Partnership',
        engagementLevel: 'Medium',
      },
      {
        id: 3,
        name: 'Ministry of Water Resources - Bangladesh',
        type: 'Client Partner',
        relationship: 'Government Partner',
        engagementLevel: 'High',
      },
      {
        id: 4,
        name: 'WaterAid International',
        type: 'Implementing Partner',
        relationship: 'NGO Partnership',
        engagementLevel: 'High',
      },
      {
        id: 5,
        name: 'Department of Water Supply - Nepal',
        type: 'Client Partner',
        relationship: 'Government Partner',
        engagementLevel: 'Medium',
      },
    ],

    relatedInteractions: [
      {
        id: 1,
        type: 'meeting',
        title: 'Partnership Discussion with World Bank',
        date: '2025-01-15',
        participants: 'Michael Chen, Sarah Chen, James Wilson',
      },
      {
        id: 2,
        type: 'call',
        title: 'Technical Review Call - Nepal Implementation',
        date: '2025-01-10',
        participants: 'Priya Sharma, James Wilson',
      },
      {
        id: 3,
        type: 'email',
        title: 'Budget Clarification Request',
        date: '2025-01-08',
        participants: 'Thomas Mueller, Maria Garcia',
      },
      {
        id: 4,
        type: 'meeting',
        title: 'Stakeholder Coordination Meeting',
        date: '2025-01-05',
        participants: 'Multiple stakeholders (8 participants)',
      },
      {
        id: 5,
        type: 'visit',
        title: 'Field Visit - Bangladesh Sites',
        date: '2024-12-20',
        participants: 'Dr. Ahmed Hassan, Sarah Chen, site team',
      },
    ],

    dstAnalysis: {
      overallScore: 7.2,
      complexity: 'Medium-High',
      lastUpdated: '2 hours ago',
      risks: [
        {
          id: 1,
          severity: 'High',
          title: 'Political instability in Myanmar',
          description:
            'Implementation areas affected by ongoing political tensions and conflict',
          source: 'Country risk profile, Recent conflict analysis',
          recommendation: 'Develop contingency plan and early warning system',
        },
        {
          id: 2,
          severity: 'Medium',
          title: 'Limited local technical capacity',
          description:
            'Insufficient trained personnel for long-term system maintenance',
          source: 'Similar project lessons learned (Nepal 2023)',
          recommendation: 'Extend capacity building timeline by 3 months',
        },
        {
          id: 3,
          severity: 'Medium',
          title: 'Monsoon season constraints',
          description:
            'Construction activities limited during peak monsoon (June-Sept)',
          source: 'Weather pattern analysis for Bangladesh, Nepal',
          recommendation: 'Adjust timeline to avoid peak monsoon period',
        },
        {
          id: 4,
          severity: 'Medium',
          title: 'EUR currency fluctuation risk',
          description: 'Exchange rate volatility for €700K commitment',
          source: 'Financial risk assessment, Historical volatility data',
          recommendation:
            'Include currency hedging clause in partner agreement',
        },
      ],
      opportunities: [
        'Leverage existing partnership with World Bank',
        'Strong government commitment demonstrated',
        'Proven technology solutions available',
      ],
      recommendations: [
        {
          id: 1,
          title: 'Add gender advisor to development team',
          rationale:
            'Required for water infrastructure projects in these contexts',
          accepted: false,
        },
        {
          id: 2,
          title: 'Consider phased implementation approach',
          rationale:
            'Reduce risk by starting with Bangladesh, then Nepal, Myanmar',
          accepted: false,
        },
        {
          id: 3,
          title: 'Conduct early environmental and social impact assessment',
          rationale: 'Required for infrastructure projects of this scale',
          accepted: false,
        },
      ],
      similarOpportunities: [
        {
          id: 1001,
          name: 'Water Infrastructure Development - Nepal 2023',
          relevance: 89,
          budget: 2100000,
          duration: '18 months',
          status: 'Completed Successfully',
          keyLessons:
            'Early community engagement critical; extend training period',
        },
        {
          id: 1002,
          name: 'Rural Water Supply Program - Bangladesh 2022',
          relevance: 85,
          budget: 3200000,
          duration: '24 months',
          status: 'Completed Successfully',
          keyLessons: 'Monsoon delays significant; budget 20% contingency',
        },
        {
          id: 1003,
          name: 'Community WASH Initiative - Myanmar 2021',
          relevance: 82,
          budget: 1800000,
          duration: '20 months',
          status: 'Completed with Challenges',
          keyLessons:
            'Political context requires flexible approach and contingency planning',
        },
      ],
    },

    comments: [
      {
        id: 1,
        author: 'Sarah Chen',
        timestamp: '2 hours ago',
        content:
          'Updated budget estimates based on latest partner feedback. Please review new deliverable structure.',
        mentions: ['James Wilson', 'Maria Garcia'],
      },
      {
        id: 2,
        author: 'James Wilson',
        timestamp: '1 hour ago',
        content:
          'Looks good, but we may need to adjust timeline for monsoon season.',
        mentions: [],
      },
      {
        id: 3,
        author: 'Maria Garcia',
        timestamp: '30 min ago',
        content:
          'Fee calculations need verification against new partnership agreement terms.',
        mentions: [],
      },
      {
        id: 4,
        author: 'AI Assistant',
        timestamp: '3 hours ago',
        content:
          'Analysis complete: 4 new risks identified from country context data. Recommend review.',
        mentions: [],
      },
    ],

    createdDate: '2025-01-08',
    createdBy: 'Sarah Chen',
    lastModifiedDate: '2025-01-24',
    lastModifiedBy: 'Sarah Chen',
    completeness: 78,
  };

  /**
   * @description Get the demo opportunity data
   * @returns {Observable<DemoOpportunity>} Observable of demo opportunity
   * @example
   * ```typescript
   * this.demoService.getDemoOpportunity().subscribe(opp => {
   *   console.log(opp.name);
   * });
   * ```
   * @since 1.0.0
   */
  getDemoOpportunity(): Observable<DemoOpportunity> {
    return of(this.demoOpportunity).pipe(delay(500)); // Simulate API delay
  }

  /**
   * @description Get AI suggestions for current context
   * @returns {Observable<string[]>} Observable of AI suggestions
   * @since 1.0.0
   */
  getAISuggestions(): Observable<string[]> {
    return of([
      'Target Signing Date is missing - this is required for Go/No-Go decision',
      'Based on deliverables, consider adding SDG 13 (Climate Action)',
      'Found 3 similar opportunities with relevant lessons learned',
    ]).pipe(delay(300));
  }

  /**
   * @description Update opportunity completeness
   * @param {number} completeness Percentage complete (0-100)
   * @returns {Observable<void>}
   * @since 1.0.0
   */
  updateCompleteness(completeness: number): Observable<void> {
    this.demoOpportunity.completeness = completeness;
    return of(void 0).pipe(delay(200));
  }
}
