/**
 * @fileoverview WHO section component for opportunity partners and team management
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  input,
  output,
  signal,
  computed,
  inject,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  effect,
  OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { BadgeModule } from 'primeng/badge';
import { AvatarModule } from 'primeng/avatar';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { MessageModule } from 'primeng/message';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TooltipModule } from 'primeng/tooltip';
import { TextareaModule } from 'primeng/textarea';
import { CheckboxModule } from 'primeng/checkbox';
import { Opportunity, OpportunityFundingPartner, OpportunityClientPartner, OpportunityExternalStakeholder, DocumentDetail, PartnerAgreementInfo } from '@shared/models/opportunity.model';
import { OpportunityService } from '@features/partnerships/opportunities/services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { Router } from '@angular/router';
import { ValuesService, SimpleValue } from '@shared/services/api/values.service';
import { DocumentService } from '@shared/services/api/document.service';

/**
 * @class OpportunityWhoSectionComponent
 * @description Component for managing opportunity partners, clients, and team members
 * 
 * @example
 * ```html
 * <app-opportunity-who-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-who-section',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    TranslateModule,
    PanelModule,
    ButtonModule,
    DividerModule,
    BadgeModule,
    AvatarModule,
    DialogModule,
    SelectModule,
    InputTextModule,
    InputNumberModule,
    MessageModule,
    FloatLabelModule,
    TooltipModule,
    TextareaModule,
    CheckboxModule
  ],
  templateUrl: './opportunity-who-section.component.html',
  styleUrls: ['./opportunity-who-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhoSectionComponent implements OnInit {
  private readonly opportunityService = inject(OpportunityService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translateService = inject(TranslateService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly valuesService = inject(ValuesService);

  // Inputs
  readonly opportunity = input.required<Opportunity>();
  readonly suggestions = input<any[]>([]);
  
  /**
   * @description Input signal for update permission - controls visibility of edit button
   */
  readonly canUpdate = input<boolean>(false);

  // Outputs
  readonly opportunityUpdated = output<Opportunity>();
  readonly changesDetected = output<void>();
  readonly changesSavedOrDiscarded = output<void>();

  // State signals
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  private hasUnsavedChanges = false;
  private originalData: {
    fundingPartners?: any[];
    clientPartners?: any[];
    externalStakeholders?: any[];
    miscExternalStakeholders?: string | null;
    externalStakeholderNotes?: string | null;
    isPooledFunding?: boolean;
  } | null = null;
  
  // Funding Partner dialog state
  readonly showFundingPartnerDialog = signal(false);
  readonly showFundingValidationError = signal(false);
  readonly isEditingFundingPartner = signal(false);
  readonly editingFundingPartnerIndex = signal(-1);
  readonly partnerControl = new FormControl<SimpleValue | null>(null);
  readonly currencyControl = new FormControl<number | null>(null); // Currency ID
  readonly amountControl = new FormControl<number | null>(null);

  // Client Partner dialog state
  readonly showClientPartnerDialog = signal(false);
  readonly showClientValidationError = signal(false);
  readonly isEditingClientPartner = signal(false);
  readonly editingClientPartnerIndex = signal(-1);
  readonly clientPartnerControl = new FormControl<SimpleValue | null>(null);
  
  // External Stakeholder dialog state
  readonly showExternalStakeholderDialog = signal(false);
  readonly showExternalStakeholderValidationError = signal(false);
  readonly contactControl = new FormControl<SimpleValue | null>(null);
  readonly miscExternalStakeholdersControl = new FormControl<string | null>(null);
  readonly externalStakeholderNotesControl = new FormControl<string | null>(null);

  // Available partners and currencies from API
  readonly availablePartners = signal<SimpleValue[]>([]);
  readonly availableCurrencies = signal<SimpleValue[]>([]);
  readonly availableContacts = signal<SimpleValue[]>([]);

  // Pooled funding state
  isPooledFunding = false;

  // Computed counts
  readonly fundingPartnerCount = computed(() => {
    return this.opportunity().fundingPartners?.length || 0;
  });

  readonly clientPartnerCount = computed(() => {
    return this.opportunity().clientPartners?.length || 0;
  });
  
  readonly externalStakeholderCount = computed(() => {
    return this.opportunity().externalStakeholders?.length || 0;
  });

  ngOnInit(): void {
    this.loadPartners();
    this.loadCurrencies();
    this.loadContacts();
    
    // Initialize pooled funding state
    this.isPooledFunding = this.opportunity().isPooledFunding || false;
    
    // Initialize external stakeholder controls
    this.miscExternalStakeholdersControl.setValue(this.opportunity().miscExternalStakeholders);
    this.externalStakeholderNotesControl.setValue(this.opportunity().externalStakeholderNotes);
  }

  /**
   * @description Load available partners from API (excludes pooled fund partners)
   */
  loadPartners(): void {
    this.valuesService.getPartners().subscribe({
      next: (partners) => {
        // Filter out pooled funding partners for funding partner selection
        const eligiblePartners = partners.filter(p => !p.pooledFund);
        this.availablePartners.set(eligiblePartners);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Load available currencies
   */
  loadCurrencies(): void {
    this.valuesService.getCurrencies().subscribe({
      next: (currencies) => {
        this.availableCurrencies.set(currencies);
        this.cdr.detectChanges();
      }
    });
  }
  
  /**
   * @description Load available contacts - only from partners in this opportunity
   */
  loadContacts(): void {
    this.valuesService.getContacts().subscribe({
      next: (allContacts) => {
        // Get all partner IDs from the opportunity (funding + client partners)
        const opp = this.opportunity();
        const partnerIds = new Set<number>();
        
        // Add funding partner IDs
        opp.fundingPartners?.forEach(fp => {
          if (fp.partnerId) {
            partnerIds.add(fp.partnerId);
          }
        });
        
        // Add client partner IDs
        opp.clientPartners?.forEach(cp => {
          if (cp.partnerId) {
            partnerIds.add(cp.partnerId);
          }
        });
        
        // Filter contacts to only those belonging to the opportunity's partners
        const filteredContacts = allContacts.filter(contact => 
          contact.partnerId && partnerIds.has(contact.partnerId)
        );
        
        this.availableContacts.set(filteredContacts);
        this.cdr.detectChanges();
      }
    });
  }

  /**
   * @description Get selected currency code from currency control
   */
  getSelectedCurrencyCode(): string | null {
    const currencyId = this.currencyControl.value;
    if (!currencyId) return null;
    
    const selected = this.availableCurrencies().find(c => c.id === currencyId);
    return selected?.code || null;
  }

  /**
   * @description Start editing mode - backs up original data for cancel operation
   */
  startEditing(): void {
    const opp = this.opportunity();
    
    // Backup original data for cancel
    // Note: Internal stakeholders are now managed in the Team section
    this.originalData = {
      fundingPartners: opp.fundingPartners ? [...opp.fundingPartners] : [],
      clientPartners: opp.clientPartners ? [...opp.clientPartners] : [],
      externalStakeholders: opp.externalStakeholders ? [...opp.externalStakeholders] : [],
      miscExternalStakeholders: opp.miscExternalStakeholders ?? null,
      externalStakeholderNotes: opp.externalStakeholderNotes ?? null,
      isPooledFunding: opp.isPooledFunding ?? false
    };
    
    this.isEditing.set(true);
  }

  /**
   * @description Cancel editing mode - restores original data and exits edit mode
   */
  cancelEditing(): void {
    const opp = this.opportunity();
    
    // Restore original data if available
    // Note: Internal stakeholders are now managed in the Team section
    if (this.originalData) {
      const updatedOpportunity = {
        ...opp,
        fundingPartners: this.originalData.fundingPartners ? [...this.originalData.fundingPartners] : [],
        clientPartners: this.originalData.clientPartners ? [...this.originalData.clientPartners] : [],
        externalStakeholders: this.originalData.externalStakeholders ? [...this.originalData.externalStakeholders] : [],
        miscExternalStakeholders: this.originalData.miscExternalStakeholders ?? null,
        externalStakeholderNotes: this.originalData.externalStakeholderNotes ?? null,
        isPooledFunding: this.originalData.isPooledFunding ?? false
      };
      
      // Emit the reverted opportunity to parent
      this.opportunityUpdated.emit(updatedOpportunity);
    }
    
    this.isEditing.set(false);
    this.originalData = null;
    this.hasUnsavedChanges = false;
    this.changesSavedOrDiscarded.emit();
  }

  /**
   * @description Mark section as having unsaved changes
   * @private
   */
  private markAsChanged(): void {
    if (!this.hasUnsavedChanges) {
      this.hasUnsavedChanges = true;
      this.changesDetected.emit();
    }
  }

  /**
   * @description Handle pooled funding checkbox change
   */
  onPooledFundingChange(): void {
    if (this.isEditing()) {
      this.markAsChanged();
    }
  }

  /**
   * @description Handle individual partner pooled contribution checkbox change
   */
  onPooledContributionChange(): void {
    if (this.isEditing()) {
      this.markAsChanged();
    }
  }

  /**
   * @description Save WHO section
   */
  saveSection(): void {
    const opp = this.opportunity();
    if (!opp || !opp.id) return;

    // Note: Internal stakeholders are now managed in the Team section
    const whoData = {
      isPooledFunding: this.isPooledFunding,
      fundingPartners: opp.fundingPartners?.map(fp => ({
        partnerId: fp.partnerId,
        amount: fp.amount,
        currencyId: fp.currencyId,
        percentage: fp.percentage,
        feePercentage: fp.feePercentage,
        feeAmount: fp.feeAmount,
        feeAmountUSD: fp.feeAmountUSD,
        isAmountBasedFee: fp.isAmountBasedFee,
        documentId: fp.documentId, // Include document ID if set
        isPooledContribution: fp.isPooledContribution || false,
        selectedPartnerAgreementNumber: fp.selectedPartnerAgreementNumber 
      })),
      clientPartners: opp.clientPartners?.map(cp => ({
        partnerId: cp.partnerId,
        documentId: cp.documentId, // Include document ID if set
        selectedPartnerAgreementNumber: cp.selectedPartnerAgreementNumber 
      })),
      externalStakeholders: opp.externalStakeholders?.map(es => ({
        contactId: es.contactId
      })),
      miscExternalStakeholders: this.miscExternalStakeholdersControl.value,
      externalStakeholderNotes: this.externalStakeholderNotesControl.value
    };

    this.isSaving.set(true);
    this.opportunityService.updateOpportunityWho(opp.id, whoData).subscribe({
      next: (fullUpdatedOpportunity: Opportunity) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.hasUnsavedChanges = false;
        this.originalData = null;
        
        this.opportunityUpdated.emit(fullUpdatedOpportunity);
        this.changesSavedOrDiscarded.emit();
        
        this.feedbackService.showSuccessToast({
          detail: this.translateService.instant('message.opportunity.updatedSuccessfully'),
          summary: this.translateService.instant('message.success')
        });
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isSaving.set(false);
        // Keep editing mode active so user can fix the issue
        
        // Display specific error message to user
        const errorMessage = error?.error?.detail || error?.error?.message || error?.message || 
                            this.translateService.instant('message.error.unexpectedError');
        
        this.feedbackService.showErrorToast({
          summary: this.translateService.instant('message.error'),
          detail: errorMessage
        });
        
        console.error('Error saving WHO section:', error);
        this.cdr.detectChanges();
      }
    });
  }

  // ========================================================================
  // FUNDING PARTNER MANAGEMENT
  // ========================================================================

  /**
   * @description Open dialog to add funding partner
   */
  openAddFundingPartnerDialog(): void {
    this.partnerControl.setValue(null);
    // Find USD currency from available currencies (code 'USD')
    const usdCurrency = this.availableCurrencies().find(c => c.code === 'USD');
    this.currencyControl.setValue(usdCurrency?.id || 141); // Default to USD
    this.amountControl.setValue(null);
    this.isEditingFundingPartner.set(false);
    this.editingFundingPartnerIndex.set(-1);
    this.showFundingValidationError.set(false);
    this.showFundingPartnerDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing funding partner
   */
  editFundingPartner(index: number): void {
    const opp = this.opportunity();
    const partner = opp.fundingPartners?.[index];
    
    if (!partner) return;

    // Find the partner in the master list
    const masterPartner = this.availablePartners().find(p => p.id === partner.partnerId);
    
    // Find USD currency as default fallback
    const usdCurrency = this.availableCurrencies().find(c => c.code === 'USD');
    
    this.isEditingFundingPartner.set(true);
    this.editingFundingPartnerIndex.set(index);
    this.partnerControl.setValue(masterPartner || null);
    this.currencyControl.setValue(partner.currencyId || usdCurrency?.id || 141); // Set currency or default to USD
    this.amountControl.setValue(partner.amount);
    this.showFundingValidationError.set(false);
    this.showFundingPartnerDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel funding partner dialog
   */
  cancelFundingPartnerDialog(): void {
    this.showFundingPartnerDialog.set(false);
    this.partnerControl.setValue(null);
    this.currencyControl.setValue(null);
    this.amountControl.setValue(null);
    this.isEditingFundingPartner.set(false);
    this.editingFundingPartnerIndex.set(-1);
    this.showFundingValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Confirm funding partner dialog
   */
  confirmFundingPartnerDialog(): void {
    const partner = this.partnerControl.value;
    const currency = this.currencyControl.value;
    const amount = this.amountControl.value;

    if (!partner || !currency || amount === null) {
      this.showFundingValidationError.set(true);
      return;
    }

    // Check for duplicate funding partner (both when adding and editing)
    const opp = this.opportunity();
    const currentEditingIndex = this.editingFundingPartnerIndex();
    const isDuplicate = opp.fundingPartners?.some((fp, index) => {
      // Skip the partner we're currently editing
      if (this.isEditingFundingPartner() && index === currentEditingIndex) {
        return false;
      }
      return fp.partnerId === partner.id;
    });

    if (isDuplicate) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.fundingPartnerAlreadyAdded')
      });
      return;
    }

    if (this.isEditingFundingPartner()) {
      this.updateFundingPartner(partner, amount);
    } else {
      this.addFundingPartner(partner, amount);
    }
  }

  /**
   * @description Add new funding partner
   */
  addFundingPartner(partner: SimpleValue, amount: number): void {
    const opp = this.opportunity();
    const currentPartners = [...(opp.fundingPartners || [])];

    // Get currency ID from control, or find USD as default
    const usdCurrency = this.availableCurrencies().find(c => c.code === 'USD');
    const currencyId = this.currencyControl.value || usdCurrency?.id || 141; // Default to USD
    const currencyCode = this.getSelectedCurrencyCode() || 'USD';

    const newPartner: OpportunityFundingPartner = {
      id: 0,
      opportunityId: opp.id!,
      partnerId: partner.id,
      partnerName: partner.name || '',
      partnerLogoUrl: partner.logoUrl || undefined,
      amount: amount,
      currencyId: currencyId,
      currencyCode: currencyCode,
      percentage: null,
      feePercentage: null,
      feeAmount: null,
      feeAmountUSD: null,
      isAmountBasedFee: true,
      partnershipAgreementReference: null,
      commitmentStatus: null,
      documentId: null,
      documentName: null,
      associatedDocuments: null,
      partnerStatus: null,
      partnerApprovalStatus: null,
      ddApproval: null,
      ddApprovalDate: null,
      ddExpiryDate: null,
      ddStatus: null,
      ddExpiresBeforeOpportunityEnd: null,
      partnerPreferredCurrency: null,
      amountUSD: null,
      exchangeRate: null,
      exchangeRateDate: null,
      exchangeRateDisplay: null, // Backend will calculate
      isPooledContribution: false,
      selectedPartnerAgreementNumber: null, 
      availableAgreements: null 
    };

    currentPartners.push(newPartner);

    const updatedOpportunity = {
      ...opp,
      fundingPartners: currentPartners
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelFundingPartnerDialog();
  }

  /**
   * @description Update existing funding partner
   */
  updateFundingPartner(partner: SimpleValue, amount: number): void {
    const opp = this.opportunity();
    const currentPartners = [...(opp.fundingPartners || [])];
    const index = this.editingFundingPartnerIndex();

    if (index < 0 || index >= currentPartners.length) {
      return;
    }

    // Get currency ID from control, or find USD as default
    const usdCurrency = this.availableCurrencies().find(c => c.code === 'USD');
    const currencyId = this.currencyControl.value || usdCurrency?.id || 141; // Default to USD
    const currencyCode = this.getSelectedCurrencyCode() || 'USD';

    currentPartners[index] = {
      ...currentPartners[index],
      partnerId: partner.id,
      partnerName: partner.name || '',
      partnerLogoUrl: partner.logoUrl || undefined,
      amount: amount,
      currencyId: currencyId,
      currencyCode: currencyCode,
      percentage: null,
      feePercentage: null,
      feeAmount: null,
      feeAmountUSD: null,
      isAmountBasedFee: true,
      partnershipAgreementReference: null,
      // Reset USD conversion fields - backend will recalculate
      amountUSD: null,
      exchangeRate: null,
      exchangeRateDate: null,
      exchangeRateDisplay: null
    };

    const updatedOpportunity = {
      ...opp,
      fundingPartners: currentPartners
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelFundingPartnerDialog();
  }

  /**
   * @description Remove funding partner
   */
  removeFundingPartner(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeFundingPartner'),
        detail: this.translateService.instant('message.confirmRemoveFundingPartner')
      },
      () => {
        const opp = this.opportunity();
        const currentPartners = [...(opp.fundingPartners || [])];
        currentPartners.splice(index, 1);

        const updatedOpportunity = {
          ...opp,
          fundingPartners: currentPartners
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.markAsChanged();
      }
    );
  }

  // ========================================================================
  // CLIENT PARTNER MANAGEMENT
  // ========================================================================

  /**
   * @description Open dialog to add client partner
   */
  openAddClientPartnerDialog(): void {
    this.clientPartnerControl.setValue(null);
    this.isEditingClientPartner.set(false);
    this.editingClientPartnerIndex.set(-1);
    this.showClientValidationError.set(false);
    this.showClientPartnerDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing client partner
   */
  editClientPartner(index: number): void {
    const opp = this.opportunity();
    const client = opp.clientPartners?.[index];
    
    if (!client) return;

    const masterPartner = this.availablePartners().find(p => p.id === client.partnerId);
    
    this.isEditingClientPartner.set(true);
    this.editingClientPartnerIndex.set(index);
    this.clientPartnerControl.setValue(masterPartner || null);
    this.showClientValidationError.set(false);
    this.showClientPartnerDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel client partner dialog
   */
  cancelClientPartnerDialog(): void {
    this.showClientPartnerDialog.set(false);
    this.clientPartnerControl.setValue(null);
    this.isEditingClientPartner.set(false);
    this.editingClientPartnerIndex.set(-1);
    this.showClientValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Confirm client partner dialog
   */
  confirmClientPartnerDialog(): void {
    const partner = this.clientPartnerControl.value;

    if (!partner) {
      this.showClientValidationError.set(true);
      return;
    }

    // Check for duplicate client partner (both when adding and editing)
    const opp = this.opportunity();
    const currentEditingIndex = this.editingClientPartnerIndex();
    const isDuplicate = opp.clientPartners?.some((cp, index) => {
      // Skip the partner we're currently editing
      if (this.isEditingClientPartner() && index === currentEditingIndex) {
        return false;
      }
      return cp.partnerId === partner.id;
    });

    if (isDuplicate) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.clientPartnerAlreadyAdded')
      });
      return;
    }

    if (this.isEditingClientPartner()) {
      this.updateClientPartner(partner);
    } else {
      this.addClientPartner(partner);
    }
  }

  /**
   * @description Add new client partner
   */
  addClientPartner(partner: SimpleValue): void {
    const opp = this.opportunity();
    const currentClients = [...(opp.clientPartners || [])];

    const newClient: OpportunityClientPartner = {
      id: 0,
      opportunityId: opp.id!,
      partnerId: partner.id,
      partnerName: partner.name || '',
      partnerLogoUrl: partner.logoUrl || undefined,
      documentId: null,
      documentName: null,
      associatedDocuments: null,
      partnerStatus: null,
      partnerApprovalStatus: null,
      ddApproval: null,
      ddApprovalDate: null,
      ddExpiryDate: null,
      ddStatus: null,
      ddExpiresBeforeOpportunityEnd: null,
      selectedPartnerAgreementNumber: null, 
      availableAgreements: null 
    };

    currentClients.push(newClient);

    const updatedOpportunity = {
      ...opp,
      clientPartners: currentClients
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelClientPartnerDialog();
  }

  /**
   * @description Update existing client partner
   */
  updateClientPartner(partner: SimpleValue): void {
    const opp = this.opportunity();
    const currentClients = [...(opp.clientPartners || [])];
    const index = this.editingClientPartnerIndex();

    if (index < 0 || index >= currentClients.length) {
      return;
    }

    currentClients[index] = {
      ...currentClients[index],
      partnerId: partner.id,
      partnerName: partner.name || '',
      partnerLogoUrl: partner.logoUrl || undefined
    };

    const updatedOpportunity = {
      ...opp,
      clientPartners: currentClients
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelClientPartnerDialog();
  }

  /**
   * @description Remove client partner
   */
  removeClientPartner(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeClientPartner'),
        detail: this.translateService.instant('message.confirmRemoveClientPartner')
      },
      () => {
        const opp = this.opportunity();
        const currentClients = [...(opp.clientPartners || [])];
        currentClients.splice(index, 1);

        const updatedOpportunity = {
          ...opp,
          clientPartners: currentClients
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.markAsChanged();
      }
    );
  }

  /**
   * @description Navigate to partner detail page
   */
  navigateToPartner(partnerId: number): void {
    this.router.navigate(['/#/partnerships/partners', partnerId]);
  }

  /**
   * @description Calculate percentage allocation for a funding partner
   */
  calculatePercentage(partner: OpportunityFundingPartner): number {
    const opp = this.opportunity();
    const totalAmount = (opp.fundingPartners || []).reduce((sum, p) => sum + (p.amount || 0), 0);
    if (totalAmount === 0) return 0;
    return Math.round(((partner.amount || 0) / totalAmount) * 100);
  }

  /**
   * @description Format currency for display
   */
  formatCurrency(value: number | null | undefined): string {
    if (!value) return '$0';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(value);
  }
  
  /**
   * @description Open document in new tab or download
   */
  openDocument(doc: DocumentDetail): void {
    if (!doc.id) return;
    
    const documentService = inject(DocumentService);
    const translateService = inject(TranslateService);
    const feedbackService = inject(FeedbackDialogService);
    
    // First try to get the view URL
    documentService.getDocumentViewUrl(doc.id).subscribe({
      next: (response) => {
        if (response && response.url) {
          // Open in new tab
          window.open(response.url, '_blank');
        } else if (doc.storagePath) {
          // If we have storagePath (GCS path), try to open directly
          window.open(doc.storagePath, '_blank');
        } else {
          // Fallback to download
          this.downloadDocument(doc.id!);
        }
      },
      error: (error) => {
        console.error('View error:', error);
        // Try download as fallback
        if (doc.storagePath) {
          window.open(doc.storagePath, '_blank');
        } else {
          this.downloadDocument(doc.id!);
        }
      }
    });
  }
  
  /**
   * @description Open Partnership Agreement document in new tab
   */
  openAgreementDocument(agreement: PartnerAgreementInfo): void {
    if (!agreement.documentId) return;
    
    const documentService = inject(DocumentService);
    
    // Use document service to get view URL
    documentService.getDocumentViewUrl(agreement.documentId).subscribe({
      next: (response) => {
        if (response && response.url) {
          window.open(response.url, '_blank');
        } else if (agreement.documentStoragePath) {
          window.open(agreement.documentStoragePath, '_blank');
        }
      },
      error: (error) => {
        console.error('Error opening agreement document:', error);
        // Try storage path as fallback
        if (agreement.documentStoragePath) {
          window.open(agreement.documentStoragePath, '_blank');
        }
      }
    });
  }
  
  /**
   * @description Download document
   */
  private downloadDocument(documentId: number): void {
    const documentService = inject(DocumentService);
    const feedbackService = inject(FeedbackDialogService);
    const translateService = inject(TranslateService);
    
    documentService.downloadDocument(documentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'document';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Download error:', error);
        feedbackService.showErrorToast({
          summary: translateService.instant('message.error'),
          detail: translateService.instant('message.document.viewFailed')
        });
      }
    });
  }

  // Note: Internal stakeholder management has been moved to the Team section
  
  // ==================================================================
  // External Stakeholder Management Methods
  // ==================================================================
  
  /**
   * @description Open dialog to add external stakeholder
   */
  openAddExternalStakeholderDialog(): void {
    this.contactControl.reset();
    this.showExternalStakeholderValidationError.set(false);
    this.showExternalStakeholderDialog.set(true);
  }
  
  /**
   * @description Cancel external stakeholder dialog
   */
  cancelExternalStakeholderDialog(): void {
    this.showExternalStakeholderDialog.set(false);
    this.contactControl.reset();
    this.showExternalStakeholderValidationError.set(false);
  }
  
  /**
   * @description Confirm external stakeholder dialog
   */
  confirmExternalStakeholderDialog(): void {
    const contact = this.contactControl.value;
    
    if (!contact) {
      this.showExternalStakeholderValidationError.set(true);
      return;
    }
    
    this.addExternalStakeholder(contact);
  }
  
  /**
   * @description Add external stakeholder
   */
  addExternalStakeholder(contact: SimpleValue): void {
    const opp = this.opportunity();
    const currentExternalStakeholders = [...(opp.externalStakeholders || [])];
    
    // Check for duplicates
    const isDuplicate = currentExternalStakeholders.some(es => es.contactId === contact.id);
    if (isDuplicate) {
      this.feedbackService.showWarningToast({
        summary: this.translateService.instant('message.warning'),
        detail: this.translateService.instant('message.validation.externalStakeholderAlreadyAdded')
      });
      return;
    }
    
    const newExternalStakeholder: OpportunityExternalStakeholder = {
      id: 0,
      opportunityId: opp.id!,
      contactId: contact.id,
      contactName: contact.name || '',
      contactEmail: contact.email || null,
      contactOrganization: null
    };
    
    currentExternalStakeholders.push(newExternalStakeholder);
    
    const updatedOpportunity = {
      ...opp,
      externalStakeholders: currentExternalStakeholders
    };
    
    this.opportunityUpdated.emit(updatedOpportunity);
    this.markAsChanged();
    this.cancelExternalStakeholderDialog();
  }
  
  /**
   * @description Remove external stakeholder
   */
  removeExternalStakeholder(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeExternalStakeholder'),
        detail: this.translateService.instant('message.confirmRemoveExternalStakeholder')
      },
      () => {
        const opp = this.opportunity();
        const currentExternalStakeholders = [...(opp.externalStakeholders || [])];
        currentExternalStakeholders.splice(index, 1);

        const updatedOpportunity = {
          ...opp,
          externalStakeholders: currentExternalStakeholders
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.markAsChanged();
        this.cdr.detectChanges();
      }
    );
  }

  /**
   * @description Get severity for partner status badge
   */
  getPartnerStatusSeverity(status: string | null): 'success' | 'warn' | 'danger' | 'info' {
    switch (status) {
      case 'Active': return 'success';
      case 'Draft': return 'warn';
      case 'Closed': return 'danger';
      case 'Archived': return 'info';
      default: return 'info';
    }
  }

  /**
   * Get severity for DD status badge
   */
  getDDStatusSeverity(status: string | null): 'success' | 'warn' | 'danger' | 'info' {
    switch (status) {
      case 'Valid': return 'success';
      case 'Approved': return 'success';
      case 'Expiring Soon': return 'warn';
      case 'Expired': return 'danger';
      case 'Pending': return 'info';
      case 'Not Required': return 'info';
      default: return 'info';
    }
  }
}

