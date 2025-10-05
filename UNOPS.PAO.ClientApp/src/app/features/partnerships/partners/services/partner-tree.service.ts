import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { tap } from 'rxjs';
import { PartnerTree } from '../models/partner-tree.model';
import { PartnerCategoryGroup, PartnerGroup } from '../models/partner-category-group.model';

@Injectable({
  providedIn: 'root',
})
export class PartnerTreeService {
  http = inject(HttpClient);

  private partnerTreeData = signal<TreeNode<PartnerTree>[]>([]);
  allPartnerTreeData = this.partnerTreeData.asReadonly();
  isLoading = signal(false);
  parentOptions: PartnerTree[] = [];
  levelOneOptions: PartnerTree[] = [];
  levelTwoOptions: PartnerTree[] = [];
  levelThreeOptions: PartnerTree[] = [];
  originalData: PartnerTree[] = [];
  partnerGroupOptions: PartnerTree[] = [];

  // Add signal for the category and group structure
  private categoryGroupStructure = signal<PartnerCategoryGroup[]>([]);
  allCategoryGroupStructure = this.categoryGroupStructure.asReadonly();

  constructor() { }

  flattenTree(tree: TreeNode<PartnerTree>[]): PartnerTree[] {
    const result: PartnerTree[] = [];
    const traverse = (nodes: TreeNode<PartnerTree>[]) => {
      for (const node of nodes) {
        if (node.data) {
          result.push(node.data);
        }
        if (node.children) {
          traverse(node.children);
        }
      }
    };
    traverse(tree);
    return result;
  }

  getChildrenByParentCode(parentCode: string): PartnerTree[] {
    if (!parentCode) {
      return [];
    }
    return this.parentOptions.filter(option =>
      option.parent === parentCode || option.code === parentCode
    );
  }

  getAllPartnerTree() {
    this.isLoading.set(true);
    return this.http.get<TreeNode<PartnerTree>[]>(`/api/partner-tree`).pipe(
      tap({
        next: (data) => {
          this.partnerTreeData.set(data);
          const originalData = JSON.parse(JSON.stringify(data));
          const flatData: PartnerTree[] = this.flattenTree(originalData);
          this.parentOptions = flatData;

          this.partnerGroupOptions = this.parentOptions;
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error fetching data:', err); // Debugging statement
          this.isLoading.set(false);
        },
      })
    );
  }

  getPartnerTreeDataById(id: string) {
    this.isLoading.set(true);
    return this.http.get<PartnerTree>(`/api/partner-tree/${id}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  createPartnerTreeLevel(requestJson: PartnerTree) {
    this.isLoading.set(true);
    return this.http.post<PartnerTree>('/api/partner-tree', requestJson).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  updatePartnerTreeLevel(requestJson: PartnerTree[]) {
    this.isLoading.set(true);
    return this.http.put<PartnerTree[]>('/api/partner-tree', requestJson).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        },
        complete: () => {
          this.isLoading.set(false);
        }
      }));
  }

  deletePartnerLevel(id: string) {
    this.isLoading.set(true);
    return this.http.delete(`/api/partner-tree/${id}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  getCategoryAndGroupStructure() {
    this.isLoading.set(true);
    return this.http.get<PartnerCategoryGroup[]>(`/api/partner-tree-structure`).pipe(
      tap({
        next: (data) => {
          this.categoryGroupStructure.set(data);
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error fetching category and group structure:', err);
          this.isLoading.set(false);
        }
      })
    );
  }
}
