import { Component, inject, OnInit, ChangeDetectorRef, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { UserPreferenceService } from '@core/services/user-preference.service';
import { AuthService } from '@core/services/auth.service';
import { GlobalFilterService } from '@core/services/global-filter.service';
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
    DropdownModule,
    TranslateModule,
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
  private translateService = inject(TranslateService);
  private destroyRef = inject(DestroyRef);

  visible = false;
  currentUserId = '';
  saving = false;
  resetting = false;
  loading = false;
  
  // CROSS-SECTIONAL FILTERS (Apply to ALL data)
  
  // 1. ORGANIZATIONAL SCOPE
  selectedOrgUnitId: number | null = null;
  
  // 2. PERSONAL SCOPE  
  relatedToMe = false;
  
  // 3. ACTIVITY TIMEFRAME
  activityTimeframe: 'all' | 'last30days' | 'last90days' | 'thisyear' | 'custom' = 'all';
  customStartDate: Date | null = null;
  customEndDate: Date | null = null;
  showTimeframeFilter = true; // Can be set to false to hide this section
  
  // Force re-render key for org unit selector
  orgUnitSelectorKey = 0;
  
  // Timeframe options for dropdown
  timeframeOptions = [
    { label: this.translateService.instant('globalFiltersDialog.activityTimeframe.timeframeOptions.allTime'), value: 'all' },
    { label: this.translateService.instant('globalFiltersDialog.activityTimeframe.timeframeOptions.last30Days'), value: 'last30days' },
    { label: this.translateService.instant('globalFiltersDialog.activityTimeframe.timeframeOptions.last90Days'), value: 'last90days' },
    { label: this.translateService.instant('globalFiltersDialog.activityTimeframe.timeframeOptions.thisYear'), value: 'thisyear' },
    { label: this.translateService.instant('globalFiltersDialog.activityTimeframe.timeframeOptions.customRange'), value: 'custom' }
  ];

  ngOnInit() {
    this.loadCurrentUser();
  }

  private async loadCurrentUser() {
    try {
      const claims = await this.authService.user().pipe(takeUntilDestroyed(this.destroyRef)).toPromise();
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
    this.loading = true;
    
    // Initial change detection to show loading state
    this.cdr.detectChanges();
    
    try {
      // Always reload filters fresh from backend when opening dialog
      await this.forceReloadFilters();
      await this.syncGlobalFilterService();
      
      // Small delay to ensure org unit selector has processed the changes
      await new Promise(resolve => setTimeout(resolve, 100));
      
    } catch (error) {
      console.error('Error showing global filters dialog:', error);
    } finally {
      this.loading = false;
      // Use markForCheck instead of detectChanges to be less aggressive
      this.cdr.markForCheck();
    }
  }

  hide() {
    this.visible = false;
    this.cdr.markForCheck(); // Ensure visibility change is detected
  }

  private async loadFilters() {
    if (!this.currentUserId) return;

    try {
      const filters = await this.userPreferenceService.getGlobalFilters(this.currentUserId).pipe(takeUntilDestroyed(this.destroyRef)).toPromise();
      
      if (filters) {
        // Load org unit - don't default to user's org unit, start with null (show everything)
        this.selectedOrgUnitId = filters.orgUnitId || null;
        
        // Load toggles
        this.relatedToMe = filters.relatedToMe || false;

        // Load timeframe filters - convert from old date format to new timeframe format
        this.convertLegacyDateFiltersToTimeframe(filters);
        
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
      this.activityTimeframe = 'all';
      this.customStartDate = null;
      this.customEndDate = null;
      this.cdr.markForCheck();
    }
  }

  // Force reload filters fresh from backend (bypassing any caching)
  private async forceReloadFilters() {
    if (!this.currentUserId) return;

    try {
      // Reset local state first to ensure clean slate
      this.selectedOrgUnitId = null;
      this.relatedToMe = false;
      this.activityTimeframe = 'all';
      this.customStartDate = null;
      this.customEndDate = null;
      
      // Increment key to force org unit selector re-render
      this.orgUnitSelectorKey++;

      // Now fetch fresh filters from backend
      const filters = await this.userPreferenceService.getGlobalFilters(this.currentUserId).pipe(takeUntilDestroyed(this.destroyRef)).toPromise();
      
      if (filters) {
        // Load org unit - don't default to user's org unit, start with null (show everything)
        this.selectedOrgUnitId = filters.orgUnitId || null;
        
        // Load toggles
        this.relatedToMe = filters.relatedToMe || false;

        // Load timeframe filters - convert from old date format to new timeframe format
        this.convertLegacyDateFiltersToTimeframe(filters);
        
      }
      
    } catch (error) {
      console.error('Error force reloading filters:', error);
      // On error, ensure we have clean defaults
      this.selectedOrgUnitId = null;
      this.relatedToMe = false;
      this.activityTimeframe = 'all';
      this.customStartDate = null;
      this.customEndDate = null;
      this.orgUnitSelectorKey++;
    }
  }

  // Convert legacy date filters to new timeframe format
  private convertLegacyDateFiltersToTimeframe(filters: any) {
    // If we have the new timeframe format, use it directly (don't guess!)
    if (filters.activityTimeframe) {
      this.activityTimeframe = filters.activityTimeframe;
      
      // For custom timeframe, load the custom dates
      if (filters.activityTimeframe === 'custom') {
        this.customStartDate = filters.dateFrom ? new Date(filters.dateFrom) : null;
        this.customEndDate = filters.dateTo ? new Date(filters.dateTo) : null;
      } else {
        // Clear custom dates for non-custom timeframes
        this.customStartDate = null;
        this.customEndDate = null;
      }
      return;
    }

    // Legacy conversion (only when no activityTimeframe is stored)
    const now = new Date();
    const dateOn = filters.dateOn ? new Date(filters.dateOn) : null;
    const dateFrom = filters.dateFrom ? new Date(filters.dateFrom) : null;
    const dateTo = filters.dateTo ? new Date(filters.dateTo) : null;

    // Reset to defaults
    this.activityTimeframe = 'all';
    this.customStartDate = null;
    this.customEndDate = null;

    // Only do pattern matching for legacy data (when activityTimeframe is not set)
    if (dateFrom && dateTo) {
      const daysDiff = Math.ceil((dateTo.getTime() - dateFrom.getTime()) / (1000 * 60 * 60 * 24));
      const currentYear = now.getFullYear();
      
      // Check if it matches "This Year" pattern (January 1st to current date in same year)
      if (dateFrom.getFullYear() === currentYear && 
          dateFrom.getMonth() === 0 && 
          dateFrom.getDate() === 1 &&
          dateTo.getFullYear() === currentYear) {
        this.activityTimeframe = 'thisyear';
      }
      // Check for last 30 days pattern (28-32 days to account for different months)
      else if (daysDiff >= 28 && daysDiff <= 32) {
        this.activityTimeframe = 'last30days';
      } 
      // Check for last 90 days pattern (85-95 days to account for different months)
      else if (daysDiff >= 85 && daysDiff <= 95) {
        this.activityTimeframe = 'last90days';
      } 
      // Everything else is custom
      else {
        this.activityTimeframe = 'custom';
        this.customStartDate = dateFrom;
        this.customEndDate = dateTo;
      }
    } else if (dateOn) {
      // Single date - convert to custom range
      this.activityTimeframe = 'custom';
      this.customStartDate = dateOn;
      this.customEndDate = dateOn;
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
      // Prepare the filter data with new timeframe approach
      const filters: any = {
        orgUnitId: this.selectedOrgUnitId,
        relatedToMe: this.relatedToMe,
        activityTimeframe: this.activityTimeframe, // Send the actual timeframe selection
        customStartDate: null,
        customEndDate: null,
        // Legacy fields for backward compatibility
        dateOn: null,
        dateFrom: null,
        dateTo: null
      };

      // Handle timeframe conversion to date ranges for backend
      const dateRange = this.convertTimeframeToDateRange();
      if (dateRange) {
        filters.customStartDate = dateRange.startDate ? this.formatDateForAPI(dateRange.startDate) : null;
        filters.customEndDate = dateRange.endDate ? this.formatDateForAPI(dateRange.endDate) : null;
        
        // Also populate legacy fields for backward compatibility
        filters.dateFrom = filters.customStartDate;
        filters.dateTo = filters.customEndDate;
      }

      // Save the filters (backend will fallback to user's default org unit if orgUnitId is null)
      await this.userPreferenceService.updateGlobalFilters(this.currentUserId, filters).pipe(takeUntilDestroyed(this.destroyRef)).toPromise();
      
      // Clear loading state and close dialog immediately
      this.saving = false;
      
      // Sync with GlobalFilterService to ensure consistency
      if (this.selectedOrgUnitId !== this.globalFilterService.getSelectedOrgUnitId()) {
        this.globalFilterService.setSelectedOrgUnitId(this.selectedOrgUnitId);
      }
      
      // Trigger update to reload data across the app
      this.triggerDataReload();
      
      // Close dialog using the hide method
      this.hide();
      
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
      await this.userPreferenceService.resetGlobalFilters(this.currentUserId).pipe(takeUntilDestroyed(this.destroyRef)).toPromise();
      
      // Immediately clear loading state
      this.resetting = false;
      
      // Clear the GlobalFilterService localStorage first
      this.globalFilterService.clearAllFilters();
      
      // Immediately reset local state to ensure UI updates
      this.selectedOrgUnitId = null;
      this.relatedToMe = false;
      this.activityTimeframe = 'all';
      this.customStartDate = null;
      this.customEndDate = null;
      
      // Force org unit selector to re-render by changing key
      this.orgUnitSelectorKey++;
      
      // Force change detection to update the org unit selector
      this.cdr.detectChanges();
      
      // Small delay to ensure the org unit selector has updated
      setTimeout(async () => {
        try {
          // Force reload filters from server to confirm reset values
          await this.forceReloadFilters();
          
          // Sync with GlobalFilterService to ensure consistency
          this.globalFilterService.setSelectedOrgUnitId(this.selectedOrgUnitId);
          
          // Trigger update to reload data across the app
          this.triggerDataReload();
          
        } catch (syncError) {
          // If sync/refresh fails, log it but don't show error to user since reset was successful
          console.warn('Filter sync/refresh failed after successful reset:', syncError);
        }
      }, 100);
      
      this.hide();
    } catch (error) {
      console.error('Error resetting filters:', error);
      this.resetting = false;
      alert('Error resetting filters. Please try again.');
    }
  }

  // Convert timeframe selection to actual date range
  private convertTimeframeToDateRange(): { startDate: Date | null, endDate: Date | null } | null {
    const now = new Date();
    
    switch (this.activityTimeframe) {
      case 'last30days':
        const thirtyDaysAgo = new Date();
        thirtyDaysAgo.setDate(now.getDate() - 30);
        return { startDate: thirtyDaysAgo, endDate: now };
        
      case 'last90days':
        const ninetyDaysAgo = new Date();
        ninetyDaysAgo.setDate(now.getDate() - 90);
        return { startDate: ninetyDaysAgo, endDate: now };
        
      case 'thisyear':
        const startOfYear = new Date(now.getFullYear(), 0, 1);
        return { startDate: startOfYear, endDate: now };
        
      case 'custom':
        if (this.customStartDate || this.customEndDate) {
          return { 
            startDate: this.customStartDate, 
            endDate: this.customEndDate || this.customStartDate 
          };
        }
        return null;
        
      case 'all':
      default:
        return null; // No date filtering
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
      // If we have a selected org unit in the dialog but service doesn't match, update the service
      if (this.selectedOrgUnitId !== null) {
        this.globalFilterService.setSelectedOrgUnitId(this.selectedOrgUnitId);
      }
    }
  }
} 