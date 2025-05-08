import { Component, OnInit } from '@angular/core';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { OrganizationChartModule } from 'primeng/organizationchart';
import { ButtonModule } from 'primeng/button';
import { TreeNode } from 'primeng/api';
import { OrganizationHierarchyService } from '../../../../../../services/organization-hierarchy.service';
import { OrganizationHierarchyTreeModel } from '../../../../../../models/organization-hierarchy.model';

@Component({
  selector: 'app-org-structure-dialog',
  standalone: true,
  imports: [
    OrganizationChartModule,
    ButtonModule
  ],
  template: `
    <div class="flex flex-col gap-4">
      <p-organizationChart [value]="data" selectionMode="single" 
        [(selection)]="selectedNode" 
        (onNodeSelect)="onNodeSelect($event)"
        styleClass="company">
        <ng-template let-node pTemplate="default">
          <div class="flex flex-col gap-2 p-4 border rounded-lg shadow-sm cursor-pointer"
               [class.bg-primary-50]="node === selectedNode">
            <div class="font-bold">{{node.data.name}}</div>
            <div class="text-sm text-gray-600">{{node.data.code}}</div>
            <div class="text-sm text-gray-500">{{node.data.type}}</div>
          </div>
        </ng-template>
      </p-organizationChart>

      <div class="flex justify-end gap-2">
        <p-button label="Cancel" 
                  [outlined]="true"
                  (onClick)="onCancel()"></p-button>
        <p-button label="Select" 
                  [disabled]="!selectedNode"
                  (onClick)="onSelect()"></p-button>
      </div>
    </div>
  `,
  styles: [`
    :host ::ng-deep .company .p-organizationchart-node-content {
      padding: 0;
      border: none;
    }
    :host ::ng-deep .p-organizationchart {
      overflow: auto;
      max-height: 60vh;
    }
  `]
})
export class OrgStructureDialogComponent implements OnInit {
  data: TreeNode[] = [];
  selectedNode: TreeNode | null = null;

  constructor(
    public ref: DynamicDialogRef,
    public config: DynamicDialogConfig,
    private organizationService: OrganizationHierarchyService
  ) {}

  ngOnInit() {
    this.loadOrganizationHierarchy();
  }

  loadOrganizationHierarchy() {
    this.organizationService.getOrganizationHierarchy().subscribe({
      next: (response: any) => {
        console.log('Raw response:', response);
        this.data = this.transformToTreeNodes(response);
        console.log('Transformed data:', this.data);
      },
      error: (error: Error) => {
        console.error('Error loading organization hierarchy:', error);
      }
    });
  }

  private transformToTreeNodes(response: any[]): TreeNode[] {
    // Create a map of all nodes by their ID
    const nodeMap = new Map<number, any>();
    const rootNodes: any[] = [];

    // First pass: Create all nodes
    response.forEach(item => {
      const nodeData = item.data;
      const node = {
        expanded: true,
        type: 'person',
        data: {
          id: nodeData.id,
          name: nodeData.name,
          code: nodeData.code,
          type: nodeData.type,
          description: nodeData.description
        },
        children: []
      };
      nodeMap.set(nodeData.id, node);
    });

    // Second pass: Build the tree structure
    response.forEach(item => {
      const nodeData = item.data;
      const node = nodeMap.get(nodeData.id);
      
      if (nodeData.parentId) {
        // Find parent by ID
        const parentNode = nodeMap.get(nodeData.parentId);
        if (parentNode) {
          parentNode.children.push(node);
        }
      } else {
        rootNodes.push(node);
      }
    });

    return rootNodes;
  }

  onNodeSelect(event: any) {
    this.selectedNode = event.node;
  }

  onSelect() {
    if (this.selectedNode) {
      this.ref.close({
        id: this.selectedNode.data.id,
        name: this.selectedNode.data.name,
        code: this.selectedNode.data.code,
        type: this.selectedNode.data.type
      });
    }
  }

  onCancel() {
    this.ref.close();
  }
} 