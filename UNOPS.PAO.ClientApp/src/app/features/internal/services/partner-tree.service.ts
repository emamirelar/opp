import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { TreeNode } from 'primeng/api';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PartnerTreeService {
  http = inject(HttpClient);

  private partnerTreeData = signal(<TreeNode[]>[]);
  allPartnerTreeData = this.partnerTreeData.asReadonly();
  isLoading = signal(false);
  parentOptions: any[] = [];
  levelOneOptions: any[] = [];
  levelTwoOptions: any[] = [];
  levelThreeOptions: any[] = [];
  originalData: any[] = [];

  constructor() { }

  flattenTree(tree: TreeNode[]): any[] {
    const result: any[] = [];
    const traverse = (nodes: TreeNode[]) => {
      for (const node of nodes) {
        result.push(node.data);
        if (node.children) {
          traverse(node.children);
        }
      }
    };
    traverse(tree);
    return result;
  }

  getAllPartnerTree() {
    this.isLoading.set(true);
    return this.http.get<TreeNode[]>(`/api/partner-tree`).pipe(
      tap({
        next: (data) => {
          this.partnerTreeData.set(data);
          const originalData = JSON.parse(JSON.stringify(data));
          const flatData: any[] = this.flattenTree(originalData);
          this.parentOptions = flatData.map(item => { return {value: item.code, name: item.name}});;
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
    return this.http.get(`/api/partner-tree/${id}`).pipe(tap(
      {
        next: (event) => {
          this.isLoading.set(false);
        },
        error: (err) => {
          this.isLoading.set(false);
        }
      }));
  }

  createPartnerTreeLevel( requestJson: object ){

    this.isLoading.set( true );
    return this.http.post('/api/partner-tree', requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      }
    }));
  }

  updatePartnerTreeLevel( requestJson: any[] ){

    this.isLoading.set( true );
    return this.http.put('/api/partner-tree', requestJson).pipe(tap(
    {
      next: (event) => {
        this.isLoading.set( false );
      },
      error: (err) => {
        this.isLoading.set( false );
      },
      complete: () => {
        this.isLoading.set( false );
      }
    })); 
  }

  deletePartnerLevel(id: any) {
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
}