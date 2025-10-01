import { Component, OnInit, inject, signal, effect, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { Accordion, AccordionModule, AccordionTab } from 'primeng/accordion';
import { CachedDataService } from '../../../../common/services/cached-data.service';
import { PartnerCategoryGroup, PartnerGroup } from '../../models/partner-category-group.model';

@Component({
  selector: 'app-partner-tree-page',
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    ButtonModule,
    AccordionModule
  ],
  template: `
    <div class="flex flex-col gap-6">
      <!-- Header -->
      <div class="flex flex-col gap-2">
        <h1 class="text-3xl font-bold">{{ 'title.partnerTree' | translate }}</h1>
        <p class="text-gray-600">{{ 'description.partnerTree' | translate }}</p>
      </div>

      <!-- Accordion controls -->
      <div class="flex flex-wrap gap-2 mb-6">
        <p-button icon="pi pi-plus" [label]="'button.expandAll' | translate" (click)="expandAll()" />
        <p-button icon="pi pi-minus" [label]="'button.collapseAll' | translate" (click)="collapseAll()" />
      </div>

      <!-- Partner Tree Accordion -->
      <p-accordion 
        #accordion
        styleClass="w-full"
        [multiple]="true"
        [activeIndex]="activeIndex()">
        <p-accordionTab 
          *ngFor="let category of partnerCategories(); trackBy: trackByCategory">
          
          <!-- Custom header with navigation -->
          <ng-template pTemplate="header">
            <div class="flex items-center justify-between w-full">
              <span class="font-semibold">{{ category.partnerCategoryName }}</span>
              <button 
                type="button"
                class="p-2 text-primary hover:bg-primary-50 rounded-full transition-colors"
                (click)="navigateToCategory(category); $event.stopPropagation()"
                [title]="'tooltip.viewCategoryDetails' | translate">
                <i class="pi pi-external-link text-sm"></i>
              </button>
            </div>
          </ng-template>
          
          <!-- Partner Groups List -->
          <div class="flex flex-col gap-2">
            <div 
              *ngFor="let group of category.children; trackBy: trackByGroup"
              class="p-3 border rounded-lg cursor-pointer hover:bg-gray-50 transition-colors"
              (click)="navigateToGroup(group)">
              <div class="flex items-center gap-2">
                <i class="pi pi-tag text-primary"></i>
                <span class="font-medium">{{ group.partnerGroupName }}</span>
              </div>
            </div>
            
            <!-- No groups message -->
            <div *ngIf="!category.children || category.children.length === 0" 
                 class="p-3 text-gray-500 text-center">
              {{ 'message.noPartnerGroups' | translate }}
            </div>
          </div>
        </p-accordionTab>
      </p-accordion>
    </div>
  `,
  styles: [`
    :host ::ng-deep .p-accordion-header {
      cursor: pointer;
    }
    :host ::ng-deep .p-accordion-content {
      padding: 1rem;
    }
    :host ::ng-deep .p-accordion-header-link {
      padding: 1rem;
    }
  `]
})
export class PartnerTreePageComponent implements OnInit {
  private router = inject(Router);
  private cachedDataService = inject(CachedDataService);

  partnerCategories = signal<PartnerCategoryGroup[]>([]);
  activeIndex = signal<number[]>([]);

  @ViewChild('accordion') accordion!: Accordion;

  constructor() {
    // Watch for changes in partner category groups data
    effect(() => {
      const data = this.cachedDataService.partnerCategoryGroups();
      if (data && data.length > 0) {
        this.partnerCategories.set(data);
      }
    });
  }

  ngOnInit() {
    this.loadPartnerTreeData();
  }

  private loadPartnerTreeData() {
    const partnerCategoryGroupData = this.cachedDataService.partnerCategoryGroups();
    if (!partnerCategoryGroupData || partnerCategoryGroupData.length === 0) {
      // Load the data if not available - effect will handle the update
      this.cachedDataService.loadPartnerCategoryGroups();
    }
  }

  navigateToCategory(category: PartnerCategoryGroup) {
    this.router.navigate(['/admin/partner-tree', category.partnerCategoryId]);
  }

  navigateToGroup(group: PartnerGroup) {
    this.router.navigate(['/admin/partner-tree', group.partnerGroupId]);
  }

  trackByCategory(index: number, category: PartnerCategoryGroup): number {
    return category.partnerCategoryId;
  }

  trackByGroup(index: number, group: PartnerGroup): number {
    return group.partnerGroupId;
  }

  expandAll() {
    // Expand all accordion tabs
    const allIndexes = Array.from({length: this.partnerCategories().length}, (_, i) => i);
    this.activeIndex.set(allIndexes);
  }

  collapseAll() {
    // Collapse all accordion tabs
    this.activeIndex.set([]);
  }
}