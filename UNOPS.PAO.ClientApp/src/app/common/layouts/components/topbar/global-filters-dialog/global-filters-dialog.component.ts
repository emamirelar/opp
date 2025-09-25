import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { CalendarModule } from 'primeng/calendar';
import { UserPreferenceService } from '../../../../../services/user-preference.service';
import { AuthService } from '../../../../../essentials/services/auth.service';
import { GlobalFilterService } from '../../../../../services/global-filter.service';
import { OrgUnitSelectorComponent } from '../org-unit-selector/org-unit-selector.component';

@Component({
  selector: 'app-global-filters-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    CheckboxModule,
    CalendarModule,
    OrgUnitSelectorComponent
  ],
  templateUrl: './global-filters-dialog.component.html',
  styleUrl: './global-filters-dialog.component.scss'
})
export class GlobalFiltersDialogComponent implements OnInit {
  private userPreferenceService = inject(UserPreferenceService);
  private authService = inject(AuthService);
  private globalFilterService = inject(GlobalFilterService);
  private cdr = inject(ChangeDetectorRef);

  visible = false;
  currentUserId = '';
  saving = false;
  resetting = false;
  
  // Org unit property
  selectedOrgUnitId: number | null = null;
  
  // Simplified toggle properties
  relatedToMe = false;

  // Simplified date mode properties
  dateRange = false;

  // Simplified date properties
  dateOn: Date | null = null;
  dateFrom: Date | null = null;
  dateTo: Date | null = null;

  ngOnInit() {
    this.loadCurrentUser();
  }

  private async loadCurrentUser() {
    try {
      const claims = await this.authService.user().toPromise();
      const userIdClaim = claims?.find(c => c.type === 'userId');
      if (userIdClaim) {
        this.currentUserId = userIdClaim.value;
      }
    } catch (error) {
      console.error('Error loading current user:', error);
    }
  }

  async show() {
    this.visible = true;
    await this.loadFilters();
    await this.syncGlobalFilterService();
  }

  hide() {
    this.visible = false;
    this.cdr.markForCheck(); // Ensure visibility change is detected
  }

  private async loadFilters() {
    if (!this.currentUserId) return;

    try {
      const filters = await this.userPreferenceService.getGlobalFilters(this.currentUserId).toPromise();
      
      if (filters) {
        // Load org unit - don't default to user's org unit, start with null (show everything)
        this.selectedOrgUnitId = filters.orgUnitId || null;
        
        // Load toggles
        this.relatedToMe = filters.relatedToMe || false;

        // Load dates
        this.dateOn = filters.dateOn ? new Date(filters.dateOn) : null;
        this.dateFrom = filters.dateFrom ? new Date(filters.dateFrom) : null;
        this.dateTo = filters.dateTo ? new Date(filters.dateTo) : null;
        
        // Set date range checkbox based on whether we have dateFrom (range) or dateOn (single)
        this.dateRange = !!(filters.dateFrom && filters.dateTo) && !filters.dateOn;
        
        // Trigger change detection after updating the values
        this.cdr.markForCheck();
      } else {
        // If no filters from database, default to showing everything (no org unit filter)
        this.selectedOrgUnitId = null;
        this.cdr.markForCheck();
      }
    } catch (error) {
      console.error('Error loading filters:', error);
      // On error, default to showing everything (no org unit filter)
      this.selectedOrgUnitId = null;
      
      // Reset other filters to defaults on error
      this.relatedToMe = false;
      this.dateRange = false;
      this.dateOn = null;
      this.dateFrom = null;
      this.dateTo = null;
      this.cdr.markForCheck();
    }
  }

  // Helper function to get end of day in local time
  private getEndOfDay(date: Date): Date {
    const endOfDay = new Date(date);
    endOfDay.setHours(23, 59, 59, 999);
    return endOfDay;
  }

  // Helper function to format date for API (UTC)
  private formatDateForAPI(date: Date): string {
    return new Date(Date.UTC(
      date.getFullYear(),
      date.getMonth(),
      date.getDate(),
      date.getHours(),
      date.getMinutes(),
      date.getSeconds(),
      date.getMilliseconds()
    )).toISOString();
  }

  async save() {
    if (!this.currentUserId) return;

    this.saving = true;
    
    try {
      // Prepare the filter data
      const filters: any = {
        orgUnitId: this.selectedOrgUnitId,
        relatedToMe: this.relatedToMe,
        dateOn: null,
        dateFrom: null,
        dateTo: null
      };

      // Handle date - single date or range mode
      if (!this.dateRange && this.dateOn) {
        // Single date mode - use dateOn field
        filters.dateOn = this.formatDateForAPI(this.dateOn);
      } else if (this.dateRange && this.dateFrom) {
        // Range mode - use dateFrom and dateTo fields
        filters.dateFrom = this.formatDateForAPI(this.dateFrom);
        
        if (this.dateTo) {
          // Range mode: use the specified end date
          filters.dateTo = this.formatDateForAPI(this.getEndOfDay(this.dateTo));
        } else {
          // Range mode but no end date: set end date to end of start date
          filters.dateTo = this.formatDateForAPI(this.getEndOfDay(this.dateFrom));
        }
      }

      // Save the filters (backend will fallback to user's default org unit if orgUnitId is null)
      console.log('Starting save operation...');
      await this.userPreferenceService.updateGlobalFilters(this.currentUserId, filters).toPromise();
      console.log('Save operation completed successfully');
      
      // Use setTimeout to ensure the save operation is fully processed before closing
      setTimeout(() => {
        console.log('Clearing loading state and closing dialog...');
        // Clear loading state 
        this.saving = false;
        this.cdr.detectChanges(); // Force change detection for loading state
        
        // Close dialog
        this.visible = false;
        this.cdr.detectChanges(); // Force change detection for dialog visibility
        
        console.log('Dialog should be closed now, starting sync operations...');
        
        // After dialog is closed, sync with GlobalFilterService and trigger data refresh
        setTimeout(() => {
          try {
            // Sync with GlobalFilterService to ensure consistency
            if (this.selectedOrgUnitId !== this.globalFilterService.getSelectedOrgUnitId()) {
              this.globalFilterService.setSelectedOrgUnitId(this.selectedOrgUnitId);
            }
            
            // Trigger update to reload data across the app
            this.triggerDataReload();
            console.log('Sync operations completed');
          } catch (syncError) {
            // If sync/refresh fails, log it but don't show error to user since save was successful
            console.warn('Filter sync/refresh failed after successful save:', syncError);
          }
        }, 50);
        
      }, 10); // Small delay to ensure save is fully processed
      
    } catch (error) {
      console.error('Error saving filters:', error);
      this.saving = false;
      alert('Error saving filters. Please try again.');
    }
  }

  async reset() {
    if (!this.currentUserId) return;

    this.resetting = true;
    
    try {
      // Reset filters on the server (backend now resets to show everything)
      await this.userPreferenceService.resetGlobalFilters(this.currentUserId).toPromise();
      
      // Immediately clear loading state
      this.resetting = false;
      
      // Clear the GlobalFilterService localStorage first
      this.globalFilterService.clearAllFilters();
      
      // Reload filters from server to show the actual reset values
      await this.loadFilters();
      
      // Sync with GlobalFilterService to ensure consistency
      this.globalFilterService.setSelectedOrgUnitId(this.selectedOrgUnitId);
      
      // Trigger update to reload data across the app
      try {
        this.triggerDataReload();
      } catch (syncError) {
        // If sync/refresh fails, log it but don't show error to user since reset was successful
        console.warn('Filter sync/refresh failed after successful reset:', syncError);
      }
      
      this.cdr.markForCheck();
      this.hide();
    } catch (error) {
      console.error('Error resetting filters:', error);
      this.resetting = false;
      alert('Error resetting filters. Please try again.');
    }
  }

  // Handle date range checkbox change
  onDateRangeChange() {
    if (this.dateRange) {
      // Switching to range mode - move dateOn to dateFrom
      if (this.dateOn) {
        this.dateFrom = this.dateOn;
        this.dateOn = null;
      }
      // dateTo stays as is (may be null)
    } else {
      // Switching to single date mode - move dateFrom to dateOn
      if (this.dateFrom) {
        this.dateOn = this.dateFrom;
        this.dateFrom = null;
      }
      // Clear the 'to' date since we're in single date mode
      this.dateTo = null;
    }
  }

  // Handle org unit selection
  onOrgUnitSelected(orgUnit: any) {
    this.selectedOrgUnitId = orgUnit?.id || null;
  }

  private triggerDataReload() {
    // Trigger a clean refresh of all components listening to global filter changes
    this.globalFilterService.triggerFiltersChanged();
  }

  private async syncGlobalFilterService() {
    // Ensure GlobalFilterService is in sync with what we loaded
    // This fixes the issue where the dialog shows different org unit than what's being used for filtering
    const currentGlobalOrgUnit = this.globalFilterService.getSelectedOrgUnitId();
    
    if (this.selectedOrgUnitId !== currentGlobalOrgUnit) {
      console.log('Syncing GlobalFilterService: dialog has', this.selectedOrgUnitId, 'but service has', currentGlobalOrgUnit);
      
      // If we have a selected org unit in the dialog but service doesn't match, update the service
      if (this.selectedOrgUnitId !== null) {
        this.globalFilterService.setSelectedOrgUnitId(this.selectedOrgUnitId);
      }
    }
  }
} 