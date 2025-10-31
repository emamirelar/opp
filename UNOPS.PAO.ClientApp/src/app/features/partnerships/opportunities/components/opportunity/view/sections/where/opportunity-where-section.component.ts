/**
 * @fileoverview WHERE section component for opportunity geographic implementation management
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
  OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { DividerModule } from 'primeng/divider';
import { BadgeModule } from 'primeng/badge';
import { DialogModule } from 'primeng/dialog';
import { SelectModule } from 'primeng/select';
import { MessageModule } from 'primeng/message';
import { FloatLabelModule } from 'primeng/floatlabel';
import { TooltipModule } from 'primeng/tooltip';
import { Opportunity, OpportunityCountry } from '@shared/models/opportunity.model';
import { OpportunityService } from '@features/partnerships/opportunities/services/opportunity.service';
import { FeedbackDialogService } from '@shared/services/ui/feedback-dialog.service';
import { Router } from '@angular/router';
import { ValuesService, SimpleValue } from '@shared/services/api/values.service';

/**
 * @class OpportunityWhereSectionComponent
 * @description Component for managing opportunity implementation countries
 * 
 * @example
 * ```html
 * <app-opportunity-where-section
 *   [opportunity]="opportunity()"
 *   (opportunityUpdated)="handleOpportunityUpdate($event)"
 * />
 * ```
 * 
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-where-section',
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
    DialogModule,
    SelectModule,
    MessageModule,
    FloatLabelModule,
    TooltipModule
  ],
  templateUrl: './opportunity-where-section.component.html',
  styleUrls: ['./opportunity-where-section.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OpportunityWhereSectionComponent implements OnInit {
  private readonly opportunityService = inject(OpportunityService);
  private readonly feedbackService = inject(FeedbackDialogService);
  private readonly translateService = inject(TranslateService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  private readonly valuesService = inject(ValuesService);

  // Inputs
  readonly opportunity = input.required<Opportunity>();

  // Outputs
  readonly opportunityUpdated = output<Opportunity>();

  // State signals
  readonly isEditing = signal(false);
  readonly isSaving = signal(false);
  
  // Country dialog state
  readonly showCountryDialog = signal(false);
  readonly showValidationError = signal(false);
  readonly isEditingCountry = signal(false);
  readonly editingCountryIndex = signal(-1);
  readonly countryControl = new FormControl<SimpleValue | null>(null);

  // Available countries from API
  readonly availableCountries = signal<SimpleValue[]>([]);

  // Computed count
  readonly countryCount = computed(() => {
    return this.opportunity().countries?.length || 0;
  });

  ngOnInit(): void {
    this.loadCountries();
  }

  /**
   * @description Load available countries from API
   */
  loadCountries(): void {
    this.valuesService.getCountries().subscribe({
      next: (countries) => {
        this.availableCountries.set(countries);
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Error loading countries:', error);
      }
    });
  }

  // ========================================================================
  // Edit Mode Methods
  // ========================================================================

  /**
   * @description Enable edit mode
   */
  startEditing(): void {
    this.isEditing.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel edit mode
   */
  cancelEditing(): void {
    this.isEditing.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Save changes to WHERE section
   */
  saveSection(): void {
    const opportunityId = this.opportunity().id;
    if (!opportunityId) return;

    this.isSaving.set(true);

    const whereData = {
      countries: this.opportunity().countries?.map(country => ({
        countryId: country.countryId,
        specificAreas: country.specificAreas
      })) || []
    };

    this.opportunityService.updateOpportunityWhere(opportunityId, whereData).subscribe({
      next: (updated) => {
        this.isSaving.set(false);
        this.isEditing.set(false);
        this.opportunityUpdated.emit(updated);
        this.feedbackService.showSuccessToast({
          summary: this.translateService.instant('message.success'),
          detail: this.translateService.instant('message.opportunity.whereSectionUpdated')
        });
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isSaving.set(false);
        console.error('Error saving WHERE section:', error);
        this.cdr.detectChanges();
      }
    });
  }

  // ========================================================================
  // Country Management Methods
  // ========================================================================

  /**
   * @description Open dialog to add country
   */
  openAddCountryDialog(): void {
    this.countryControl.setValue(null);
    this.isEditingCountry.set(false);
    this.editingCountryIndex.set(-1);
    this.showValidationError.set(false);
    this.showCountryDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Edit existing country
   */
  editCountry(index: number): void {
    const opp = this.opportunity();
    const country = opp.countries?.[index];
    
    if (!country) return;

    const countrySimpleValue = this.availableCountries().find(c => c.id === country.countryId);
    
    this.isEditingCountry.set(true);
    this.editingCountryIndex.set(index);
    this.countryControl.setValue(countrySimpleValue || null);
    this.showValidationError.set(false);
    this.showCountryDialog.set(true);
    this.cdr.detectChanges();
  }

  /**
   * @description Cancel country dialog
   */
  cancelCountryDialog(): void {
    this.showCountryDialog.set(false);
    this.countryControl.setValue(null);
    this.isEditingCountry.set(false);
    this.editingCountryIndex.set(-1);
    this.showValidationError.set(false);
    this.cdr.detectChanges();
  }

  /**
   * @description Confirm country dialog (add or update)
   */
  confirmCountryDialog(): void {
    const country = this.countryControl.value;

    if (!country) {
      this.showValidationError.set(true);
      return;
    }

    if (this.isEditingCountry()) {
      this.updateCountry(country);
    } else {
      this.addCountry(country);
    }
  }

  /**
   * @description Add new country
   */
  addCountry(country: SimpleValue): void {
    const opp = this.opportunity();
    const currentCountries = [...(opp.countries || [])];

    const newCountry: OpportunityCountry = {
      id: 0,
      opportunityId: opp.id!,
      countryId: country.id,
      countryName: country.name,
      countryCode: country.code || '',
      continent: country.continent || null,
      region: country.region || null,
      specificAreas: null,
      contextWarning: null,
      riskScore: null
    };

    currentCountries.push(newCountry);

    const updatedOpportunity = {
      ...opp,
      countries: currentCountries
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.cancelCountryDialog();
  }

  /**
   * @description Update existing country
   */
  updateCountry(country: SimpleValue): void {
    const opp = this.opportunity();
    const currentCountries = [...(opp.countries || [])];
    const index = this.editingCountryIndex();

    if (index < 0 || index >= currentCountries.length) {
      return;
    }

    currentCountries[index] = {
      ...currentCountries[index],
      countryId: country.id,
      countryName: country.name,
      countryCode: country.code || '',
      continent: country.continent || null,
      region: country.region || null
    };

    const updatedOpportunity = {
      ...opp,
      countries: currentCountries
    };

    this.opportunityUpdated.emit(updatedOpportunity);
    this.cancelCountryDialog();
  }

  /**
   * @description Remove country
   */
  removeCountry(index: number): void {
    this.feedbackService.showConfirmDialog(
      {
        summary: this.translateService.instant('confirmation.removeCountry'),
        detail: this.translateService.instant('message.confirmRemoveCountry')
      },
      () => {
        const opp = this.opportunity();
        const currentCountries = [...(opp.countries || [])];
        currentCountries.splice(index, 1);

        const updatedOpportunity = {
          ...opp,
          countries: currentCountries
        };

        this.opportunityUpdated.emit(updatedOpportunity);
        this.cdr.detectChanges();
      }
    );
  }
}

