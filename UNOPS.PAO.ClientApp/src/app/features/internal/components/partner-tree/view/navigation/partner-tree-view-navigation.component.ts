import { Component, OnInit, inject, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { PartnerTreeService } from '../../../../services/partner-tree.service';
import { PartnerTree } from '../../../../models/partner-tree.model';
import { CachedDataService } from '../../../../../../common/services/cached-data.service';
import { ActivatedRoute, Router } from '@angular/router';
import { PartnerCategoryGroup, PartnerGroup } from '../../../../models/partner-category-group.model';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { combineLatest, of } from 'rxjs';

@Component({
  selector: 'app-partner-tree-view-navigation',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DropdownModule,
    ButtonModule,
    SelectModule
  ],
  templateUrl: './partner-tree-view-navigation.component.html',
})
export class PartnerTreeViewNavigationComponent implements OnInit {

  partnerTreeService = inject(PartnerTreeService);
  cachedDataService = inject(CachedDataService);
  activatedRoute = inject(ActivatedRoute);
  router = inject(Router);

  partnerTree = signal<PartnerTree | null>(null);
  partnerCategorieOptions = signal<PartnerCategoryGroup[]>([]);

  // Use signals for selected values instead of regular properties
  selectedPartnerCategory = signal<PartnerCategoryGroup | undefined>(undefined);
  selectedPartnerGroup = signal<PartnerGroup | undefined>(undefined);

  // Computed partner group options based on selected category
  partnerGroupOptions = computed(() => {
    const tree = this.partnerTree();
    if (!tree?.partnerCategoryCode) return [];
    return this.cachedDataService.getParterGroupByCategoryCode(tree.partnerCategoryCode);
  });

  constructor() {
    effect(() => {
      const categories = this.cachedDataService.partnerCategoryGroups();
      if (categories && categories.length > 0) {
        this.partnerCategorieOptions.set(categories);
      }
    });

    effect(() => {
      const partnerTree = this.partnerTree();
      const categories = this.cachedDataService.partnerCategoryGroups();

      const isTherePartnerCategory = categories && categories.length > 0;

      if (isTherePartnerCategory && partnerTree?.partnerCategoryCode) {
        // Always set the partner category since we'll always have a partnerCategoryCode
        const foundCategory = categories.find(category => category.partnerCategoryCode === partnerTree.partnerCategoryCode);
        this.selectedPartnerCategory.set(foundCategory);

        // If we also have a partnerGroupCode, set the selected group
        if (partnerTree.partnerGroupCode) {
          const partnerGroups = this.cachedDataService.getParterGroupByCategoryCode(partnerTree.partnerCategoryCode);
          const foundGroup = partnerGroups.find(
            group => group.partnerGroupCode === partnerTree.partnerGroupCode
          );
          this.selectedPartnerGroup.set(foundGroup);
        } else {
          this.selectedPartnerGroup.set(undefined);
        }
      } else {
        this.selectedPartnerCategory.set(undefined);
        this.selectedPartnerGroup.set(undefined);
      }
    });
  }

  ngOnInit() {
    // Combine both route data and parameter changes for reactive updates
    combineLatest([
      this.activatedRoute.data || of({}),
      this.activatedRoute.paramMap || of(null)
    ]).subscribe(([data, params]) => {
      const recordId = params?.get('recordId');

      if (data && (data as any)['partnerTreeData']) {
        this.partnerTree.set((data as any)['partnerTreeData'].data);
      } else if (recordId && (!this.partnerTree() || recordId !== this.partnerTree()?.id?.toString())) {
        // Clear selections temporarily when navigating to a different partner tree
        this.selectedPartnerCategory.set(undefined);
        this.selectedPartnerGroup.set(undefined);
      }
    });
  }

  onPartnerCategoryChange(event: any) {
    const id = event.value.partnerCategoryId;
    this.router.navigate(['/admin/partner-tree', id]);
  }

  onPartnerGroupChange(event: any) {
    if (event.value) {
      const id = event.value.partnerGroupId;
      this.router.navigate(['/admin/partner-tree', id]);
    } else {
      // When partner group is deselected, navigate to the selected partner category
      const selectedCategory = this.selectedPartnerCategory();
      if (selectedCategory) {
        const id = selectedCategory.partnerCategoryId;
        this.router.navigate(['/admin/partner-tree', id]);
      }
    }
  }
}
