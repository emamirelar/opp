import { Component, OnInit, inject, signal, effect } from '@angular/core';
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
  partnerGroupOptions = signal<PartnerGroup[]>([]);

  selectedPartnerCategory?: PartnerCategoryGroup;
  selectedPartnerGroup?: PartnerGroup;

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


      if (isTherePartnerCategory) {

        const isThereCategoryCode = partnerTree && partnerTree.partnerCategoryCode;

        if (isThereCategoryCode) {
          this.selectedPartnerCategory = categories.find(category => category.partnerCategoryCode === partnerTree.partnerCategoryCode);
          this.selectedPartnerGroup = undefined;
          this.partnerGroupOptions.set(this.cachedDataService.getParterGroupByCategoryCode(partnerTree.partnerCategoryCode));
        } else if (this.partnerTree()?.partnerGroupCode) {
          const partnerGroupCode = this.partnerTree()?.partnerGroupCode;
          
          // Find which category contains this group
          const categoryWithGroup = categories.find(category => 
            category.children.some(group => group.partnerGroupCode === partnerGroupCode)
          );
          
          if (categoryWithGroup) {
            this.selectedPartnerCategory = categoryWithGroup;
            this.partnerGroupOptions.set(categoryWithGroup.children);
            this.selectedPartnerGroup = categoryWithGroup.children.find(
              group => group.partnerGroupCode === partnerGroupCode
            );
          }
        }


      }
    });
  }

  ngOnInit() {
    this.activatedRoute.data.subscribe((data: {[key: string]: any}) => {
      if (data['partnerTreeData']) {
        this.partnerTree.set(data['partnerTreeData'].data);
      }
    });
  }

  onPartnerCategoryChange(event: any) {
    const id = event.value.partnerCategoryId;
    this.router.navigate(['/admin/partner-tree', id]);
  }

  onPartnerGroupChange(event: any) {
    const id = event.value.partnerGroupId;
    this.router.navigate(['/admin/partner-tree', id]);
  }
}
