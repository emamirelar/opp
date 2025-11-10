/**
 * @fileoverview Demo component for AI comparison visual design
 * @author UNOPS Opportunity+ System Development Team
 */

import { Component, signal, input, output, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { CheckboxModule } from 'primeng/checkbox';
import { FormsModule } from '@angular/forms';
import { BadgeModule } from 'primeng/badge';

/**
 * @class AiComparisonDemoComponent
 * @description Demo component showing how AI-extracted data comparison will look
 * 
 * @example
 * ```html
 * <app-ai-comparison-demo
 *   [showDialog]="showDialog()"
 *   (closeDialog)="handleClose()">
 * </app-ai-comparison-demo>
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-ai-comparison-demo',
  standalone: true,
  imports: [
    CommonModule,
    ButtonModule,
    DialogModule,
    CheckboxModule,
    FormsModule,
    BadgeModule
  ],
  templateUrl: './ai-comparison-demo.component.html',
  styleUrls: ['./ai-comparison-demo.component.scss']
})
export class AiComparisonDemoComponent {
  /**
   * @description Whether the dialog is visible
   * @type {Signal<boolean>}
   * @default false
   */
  readonly showDialog = input<boolean>(false);

  /**
   * @description Event emitted when dialog should close
   * @type {OutputEmitterRef<void>}
   */
  readonly closeDialog = output<void>();
  
  // Internal signal for dialog visibility (synced with input)
  dialogVisible = signal<boolean>(false);

  constructor() {
    // Sync showDialog input with internal dialogVisible signal
    effect(() => {
      this.dialogVisible.set(this.showDialog());
    });
  }
  
  // Demo data - simulating current opportunity data
  currentData = {
    name: 'Education Infrastructure Development',
    description: 'A project to improve educational facilities',
    estimatedBudget: 1000000,
    currency: { code: 'USD', name: 'US Dollar' },
    expectedStartDate: '2024-01-15',
    expectedEndDate: '2024-12-31',
    beneficiaries: 'Local communities',
    outcomes: 'Improved access to education',
    countries: [
      { name: 'Kenya', iso2: 'KE' }
    ],
    fundingPartners: [
      { partner: { name: 'World Bank' }, amount: 500000 }
    ]
  };

  // Demo data - simulating AI-extracted data with differences
  aiData = {
    name: 'Education Infrastructure Development Program', // Changed
    description: 'A comprehensive project to improve educational facilities and teacher training', // Changed
    estimatedBudget: 1500000, // Changed
    currency: { code: 'USD', name: 'US Dollar' },
    expectedStartDate: '2024-02-01', // Changed
    expectedEndDate: '2024-12-31',
    beneficiaries: 'Local communities and vulnerable populations', // Changed
    outcomes: 'Improved access to quality education, enhanced teaching capacity', // Changed
    countries: [
      { name: 'Kenya', iso2: 'KE' },
      { name: 'Tanzania', iso2: 'TZ' } // Added
    ],
    fundingPartners: [
      { partner: { name: 'World Bank' }, amount: 800000 }, // Changed
      { partner: { name: 'UNICEF' }, amount: 700000 } // Added
    ]
  };

  // Track which fields are selected
  selectedFields = new Map<string, boolean>([
    ['name', false],
    ['description', false],
    ['deliverables', false],
    ['fundingPartners', false],
    ['clientPartners', false],
    ['sdGs', false],
    ['countries', false],
    ['stakeholders', false]
  ]);

  // Differences to show - with complex field examples
  differences = [
    {
      field: 'name',
      label: 'Opportunity Name',
      currentValue: 'Education Infrastructure Development',
      aiValue: 'Education Infrastructure Development Program',
      isArray: false
    },
    {
      field: 'description',
      label: 'Description',
      currentValue: 'A project to improve educational facilities',
      aiValue: 'A comprehensive project to improve educational facilities and teacher training',
      isArray: false
    },
    {
      field: 'deliverables',
      label: 'Deliverables',
      currentValue: [
        {
          outputName: 'School Construction',
          outputDescription: 'Build 10 new classrooms with modern facilities',
          outputGroup: 'Infrastructure',
          outputSubGroup: 'Education',
          outputServiceLine: 'Construction Management',
          quantity: 10,
          unitCode: 'units'
        }
      ],
      aiValue: [
        {
          outputName: 'School Construction',
          outputDescription: 'Build 10 new classrooms with modern facilities',
          outputGroup: 'Infrastructure',
          outputSubGroup: 'Education',
          outputServiceLine: 'Construction Management',
          quantity: 10,
          unitCode: 'units'
        },
        {
          outputName: 'Teacher Training Program',
          outputDescription: 'Comprehensive training for 100 teachers on modern teaching methods',
          outputGroup: 'Capacity Building',
          outputSubGroup: 'Education',
          outputServiceLine: 'Training & Development',
          quantity: 100,
          unitCode: 'teachers'
        }
      ],
      isArray: true
    },
    {
      field: 'fundingPartners',
      label: 'Funding Partners',
      currentValue: [
        {
          partnerId: 1,
          partnerName: 'World Bank',
          partnerLogoUrl: 'assets/images/Partner.png',
          amount: 500000,
          feeAmount: 25000,
          partnershipAgreementReference: 'WB-2024-001'
        }
      ],
      aiValue: [
        {
          partnerId: 1,
          partnerName: 'World Bank',
          partnerLogoUrl: 'assets/images/Partner.png',
          amount: 800000,
          feeAmount: 40000,
          partnershipAgreementReference: 'WB-2024-001'
        },
        {
          partnerId: 2,
          partnerName: 'UNICEF',
          partnerLogoUrl: 'assets/images/Partner.png',
          amount: 700000,
          feeAmount: 35000,
          partnershipAgreementReference: 'UNICEF-2024-015'
        }
      ],
      isArray: true
    },
    {
      field: 'clientPartners',
      label: 'Client Partners',
      currentValue: [
        {
          partnerId: 10,
          partnerName: 'Ministry of Education - Kenya',
          partnerLogoUrl: 'assets/images/Partner.png'
        }
      ],
      aiValue: [
        {
          partnerId: 10,
          partnerName: 'Ministry of Education - Kenya',
          partnerLogoUrl: 'assets/images/Partner.png'
        },
        {
          partnerId: 11,
          partnerName: 'Ministry of Education - Tanzania',
          partnerLogoUrl: 'assets/images/Partner.png'
        }
      ],
      isArray: true
    },
    {
      field: 'sdGs',
      label: 'Sustainable Development Goals (SDGs)',
      currentValue: [
        {
          sdgId: '4',
          sdgNumber: 'Goal 4',
          sdgName: 'Quality Education',
          isPrimary: true
        }
      ],
      aiValue: [
        {
          sdgId: '4',
          sdgNumber: 'Goal 4',
          sdgName: 'Quality Education',
          isPrimary: true
        },
        {
          sdgId: '10',
          sdgNumber: 'Goal 10',
          sdgName: 'Reduced Inequalities',
          isPrimary: false
        },
        {
          sdgId: '17',
          sdgNumber: 'Goal 17',
          sdgName: 'Partnerships for the Goals',
          isPrimary: false
        }
      ],
      isArray: true
    },
    {
      field: 'countries',
      label: 'Target Countries',
      currentValue: [
        {
          countryId: 1,
          countryName: 'Kenya',
          countryCode: 'KE'
        }
      ],
      aiValue: [
        {
          countryId: 1,
          countryName: 'Kenya',
          countryCode: 'KE'
        },
        {
          countryId: 2,
          countryName: 'Tanzania',
          countryCode: 'TZ'
        },
        {
          countryId: 3,
          countryName: 'Uganda',
          countryCode: 'UG'
        }
      ],
      isArray: true
    },
    {
      field: 'stakeholders',
      label: 'Stakeholders',
      currentValue: [
        {
          userId: 1,
          userName: 'John Smith',
          entityRoleName: 'Project Manager'
        }
      ],
      aiValue: [
        {
          userId: 1,
          userName: 'John Smith',
          entityRoleName: 'Project Manager'
        },
        {
          userId: 2,
          userName: 'Sarah Johnson',
          entityRoleName: 'Technical Advisor'
        },
        {
          userId: 3,
          userName: 'Michael Brown',
          entityRoleName: 'Financial Expert'
        }
      ],
      isArray: true
    }
  ];

  openDialog(): void {
    this.dialogVisible.set(true);
  }

  closeDialogInternal(): void {
    this.dialogVisible.set(false);
    this.selectedFields.clear();
    this.closeDialog.emit();
  }

  toggleField(field: string): void {
    const current = this.selectedFields.get(field) || false;
    this.selectedFields.set(field, !current);
  }

  toggleAll(): void {
    const allSelected = this.isAllSelected();
    this.differences.forEach(diff => {
      this.selectedFields.set(diff.field, !allSelected);
    });
  }

  isAllSelected(): boolean {
    return this.differences.every(diff => this.selectedFields.get(diff.field));
  }

  getSelectedCount(): number {
    return Array.from(this.selectedFields.values()).filter(v => v).length;
  }

  applyChanges(): void {
    const selected = Array.from(this.selectedFields.entries())
      .filter(([_, value]) => value)
      .map(([key, _]) => key);
    
    alert(`Would apply changes to: ${selected.join(', ')}`);
    this.closeDialogInternal();
  }

  formatArrayValue(value: any[]): string {
    if (!value || value.length === 0) return '[]';
    return JSON.stringify(value, null, 2);
  }

  /**
   * @description Check if field is a deliverable field
   */
  isDeliverableField(field: string): boolean {
    return field === 'deliverables';
  }

  /**
   * @description Check if field is a funding partner field
   */
  isFundingPartnerField(field: string): boolean {
    return field === 'fundingPartners';
  }

  /**
   * @description Check if field is a client partner field
   */
  isClientPartnerField(field: string): boolean {
    return field === 'clientPartners';
  }

  /**
   * @description Check if field is an SDG field
   */
  isSDGField(field: string): boolean {
    return field === 'sdGs';
  }

  /**
   * @description Check if field is a country field
   */
  isCountryField(field: string): boolean {
    return field === 'countries';
  }

  /**
   * @description Check if field is a stakeholder field
   */
  isStakeholderField(field: string): boolean {
    return field === 'stakeholders';
  }

  /**
   * @description Format currency amount
   */
  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }

  /**
   * @description Get SDG logo URL
   */
  getSDGLogo(sdgId: string): string {
    // Use actual SDG logo URLs (from United Nations)
    const sdgNumber = parseInt(sdgId);
    return `https://sdgs.un.org/sites/default/files/goals/E_SDG_Icons-${String(sdgNumber).padStart(2, '0')}.jpg`;
  }

  /**
   * @description Get country flag emoji
   */
  getCountryFlag(countryCode: string): string {
    if (!countryCode || countryCode.length !== 2) return '🏳️';
    
    // Convert ISO 2-letter country code to flag emoji
    const codePoints = countryCode
      .toUpperCase()
      .split('')
      .map(char => 127397 + char.charCodeAt(0));
    return String.fromCodePoint(...codePoints);
  }

  /**
   * @description Get initials from a name
   */
  getInitials(name: string): string {
    if (!name) return '??';
    const parts = name.split(' ').filter(p => p.length > 0);
    if (parts.length === 0) return '??';
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  }

  /**
   * @description Cast value to any type for template access (type-safe workaround)
   */
  asAny(value: any): any {
    return value;
  }
}

