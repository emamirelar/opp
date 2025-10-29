/**
 * @fileoverview Option 1: Unified Dashboard View - All information visible in single scrolling page
 * @author UNOPS Opportunity+ System Development Team
 */

import {
  Component,
  OnInit,
  OnDestroy,
  ElementRef,
  ViewChild,
  AfterViewInit,
  inject,
  signal,
  computed,
  effect,
  untracked,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { FormsModule } from '@angular/forms';

// PrimeNG
import { PanelModule } from 'primeng/panel';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { BadgeModule } from 'primeng/badge';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { AvatarModule } from 'primeng/avatar';
import { AvatarGroupModule } from 'primeng/avatargroup';
import { TimelineModule } from 'primeng/timeline';
import { MessageModule } from 'primeng/message';
import { FileUploadModule } from 'primeng/fileupload';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';

// Services
import {
  OpportunityDemoService,
  DemoOpportunity,
} from '@shared/services/api/opportunity-demo.service';

/**
 * @class OpportunityOption1Component
 * @description Unified Dashboard View - displays all opportunity information in a single scrolling page
 * with 5W framework (What, Who, Why, When, Where). Emphasizes complete context at a glance.
 *
 * @example
 * ```html
 * <app-opportunity-option1></app-opportunity-option1>
 * ```
 *
 * @since 1.0.0
 */
@Component({
  selector: 'app-opportunity-option1',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TranslateModule,
    FormsModule,
    PanelModule,
    ButtonModule,
    CardModule,
    BadgeModule,
    TagModule,
    ChipModule,
    DividerModule,
    AvatarModule,
    AvatarGroupModule,
    TimelineModule,
    MessageModule,
    FileUploadModule,
    TooltipModule,
    DropdownModule,
  ],
  templateUrl: './opportunity-option1.component.html',
  styleUrls: ['./opportunity-option1.component.scss'],
})
export class OpportunityOption1Component
  implements OnInit, AfterViewInit, OnDestroy
{
  private demoService = inject(OpportunityDemoService);

  @ViewChild('navigationSizer', { read: ElementRef })
  navigationSizer?: ElementRef;
  @ViewChild('chipsContainer', { read: ElementRef })
  chipsContainer?: ElementRef;

  opportunity = signal<DemoOpportunity | null>(null);
  loading = signal(true);
  aiSuggestions = signal<string[]>([]);
  showAIPanel = signal(true);
  documentsCollapsed = signal(true); // Start with documents panel collapsed
  activeSection = signal<string>('analysis'); // Track active section for navigation
  showDropdown = signal(false); // Track if navigation should be dropdown based on component width
  selectedSection: { id: string; label: string; icon: string } | null = null; // For dropdown binding

  private checkTimeout?: number;
  private resizeObserver?: ResizeObserver;
  private chipsRequiredWidth: number = 0; // Store required width for chips
  private hasInitialized = false;

  // Section navigation configuration
  sections = [
    { id: 'analysis', label: 'Analysis', icon: 'pi-chart-bar' },
    { id: 'what', label: 'What', icon: 'pi-briefcase' },
    { id: 'why', label: 'Why', icon: 'pi-lightbulb' },
    { id: 'who', label: 'Who', icon: 'pi-users' },
    { id: 'where', label: 'Where', icon: 'pi-globe' },
    { id: 'when', label: 'When', icon: 'pi-calendar' },
    { id: 'dst', label: 'DST', icon: 'pi-chart-line' },
    { id: 'related', label: 'Related', icon: 'pi-link' },
    { id: 'collaboration', label: 'Comments', icon: 'pi-comments' },
  ];

  constructor() {
    // Effect to setup observer once data is loaded
    effect(() => {
      const isLoaded = !this.loading();

      if (isLoaded && !this.hasInitialized) {
        untracked(() => {
          setTimeout(() => this.initializeNavigation(), 200);
        });
      }
    });
  }

  // Computed properties
  totalFunding = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.fundingPartners.reduce((sum, p) => sum + p.amount, 0);
  });

  totalFees = computed(() => {
    const opp = this.opportunity();
    if (!opp) return 0;
    return opp.fundingPartners.reduce((sum, p) => sum + p.feeAmount, 0);
  });

  ngOnInit(): void {
    this.updateSelectedSection();
    this.loadOpportunity();
    this.loadAISuggestions();
  }

  ngAfterViewInit(): void {
    // Wait for data to load via effect
  }

  private initializeNavigation(): void {
    if (this.hasInitialized) {
      return;
    }

    if (!this.navigationSizer || !this.chipsContainer) {
      return;
    }

    // Step 1: Measure required width for chips (ONCE, while in chips mode)
    this.showDropdown.set(false); // Ensure we're in chips mode
    setTimeout(() => {
      if (!this.chipsContainer) return;

      this.chipsRequiredWidth = this.chipsContainer.nativeElement.scrollWidth;

      // Step 2: Setup ResizeObserver ONLY on the container (not the chips)
      this.resizeObserver = new ResizeObserver(() => {
        this.checkNavigationFit();
      });

      this.resizeObserver.observe(this.navigationSizer!.nativeElement);

      this.hasInitialized = true;

      // Initial check
      this.checkNavigationFit();
    }, 100);
  }

  ngOnDestroy(): void {
    if (this.checkTimeout) {
      clearTimeout(this.checkTimeout);
    }
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
  }

  private checkNavigationFit(): void {
    // Debounce checks
    if (this.checkTimeout) {
      clearTimeout(this.checkTimeout);
    }

    this.checkTimeout = window.setTimeout(() => {
      if (!this.navigationSizer || this.chipsRequiredWidth === 0) {
        return;
      }

      // Get current container width
      const containerWidth = this.navigationSizer.nativeElement.clientWidth;

      // Compare stored chips width vs container width
      const needsDropdown = this.chipsRequiredWidth > containerWidth - 20; // 20px buffer

      // Only update if different
      if (needsDropdown !== this.showDropdown()) {
        this.showDropdown.set(needsDropdown);
      }
    }, 100);
  }

  private updateSelectedSection(): void {
    this.selectedSection =
      this.sections.find((section) => section.id === this.activeSection()) ||
      null;
  }

  onSectionDropdownChange(event: any): void {
    if (event.value) {
      this.activeSection.set(event.value.id);
      this.scrollToSection(event.value.id);
    }
  }

  getActiveSection(): { id: string; label: string; icon: string } | null {
    return (
      this.sections.find((section) => section.id === this.activeSection()) ||
      null
    );
  }

  private loadOpportunity(): void {
    this.loading.set(true);
    this.demoService.getDemoOpportunity().subscribe({
      next: (opp) => {
        this.opportunity.set(opp);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading demo opportunity:', error);
        this.loading.set(false);
      },
    });
  }

  private loadAISuggestions(): void {
    this.demoService.getAISuggestions().subscribe({
      next: (suggestions) => {
        this.aiSuggestions.set(suggestions);
      },
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  }

  getSeverityClass(severity: string): string {
    const severityMap: { [key: string]: string } = {
      High: 'danger',
      Medium: 'warn',
      Low: 'success',
    };
    return severityMap[severity] || 'success';
  }

  toggleAIPanel(): void {
    this.showAIPanel.update((v) => !v);
  }

  toggleDocumentsPanel(): void {
    this.documentsCollapsed.update((v) => !v);
  }

  documentsChevronIcon = computed(() => {
    return this.documentsCollapsed() ? 'pi-chevron-right' : 'pi-chevron-left';
  });

  getFileIcon(fileType: string): string {
    const iconMap: { [key: string]: string } = {
      pdf: 'pi-file-pdf',
      docx: 'pi-file-word',
      xlsx: 'pi-file-excel',
      pptx: 'pi-file-powerpoint',
      default: 'pi-file',
    };
    return iconMap[fileType] || iconMap['default'];
  }

  getFileIconColor(fileType: string): string {
    const colorMap: { [key: string]: string } = {
      pdf: 'text-red-500',
      docx: 'text-blue-500',
      xlsx: 'text-green-500',
      pptx: 'text-orange-500',
      default: 'text-gray-500',
    };
    return colorMap[fileType] || colorMap['default'];
  }

  getInteractionIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      meeting: 'pi-video',
      call: 'pi-phone',
      email: 'pi-envelope',
      visit: 'pi-map-marker',
    };
    return iconMap[type] || 'pi-circle';
  }

  getInteractionColor(type: string): string {
    const colorMap: { [key: string]: string } = {
      meeting: 'bg-blue-100 text-blue-600',
      call: 'bg-green-100 text-green-600',
      email: 'bg-purple-100 text-purple-600',
      visit: 'bg-orange-100 text-orange-600',
    };
    return colorMap[type] || 'bg-gray-100 text-gray-600';
  }

  onFileUpload(event: any): void {
    console.log('File uploaded:', event);
    // In real implementation, this would upload the file
  }

  scrollToSection(sectionId: string): void {
    this.activeSection.set(sectionId);

    // For the first section (analysis), scroll to the opportunity header
    if (sectionId === 'analysis') {
      const headerElement = document.getElementById('opportunity-header');
      if (headerElement) {
        headerElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
      return;
    }

    // For other sections, scroll to the section with offset
    const element = document.getElementById(`section-${sectionId}`);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}
