import { Component, OnInit } from '@angular/core';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { OrganizationChartModule } from 'primeng/organizationchart';
import { ButtonModule } from 'primeng/button';
import { TreeNode } from 'primeng/api';
import { OrganizationHierarchyService } from '../../../../../../services/organization-hierarchy.service';
import { OrganizationHierarchyTreeModel } from '../../../../../../models/organization-hierarchy.model';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';

@Component({
  selector: 'app-org-structure-dialog',
  standalone: true,
  imports: [
    OrganizationChartModule,
    ButtonModule,
    CommonModule,
    FormsModule,
    InputTextModule
  ],
  templateUrl: './org-structure-dialog.component.html',
  styleUrls: ['./org-structure-dialog.component.scss']
})
export class OrgStructureDialogComponent implements OnInit {
  data: TreeNode[] = [];
  filteredData: TreeNode[] = [];
  selectedNode: TreeNode | null = null;
  searchText: string = '';
  mainRootNode: TreeNode | null = null;
  limitedData: TreeNode[] = []; // New property for limited data

  constructor(
    public ref: DynamicDialogRef,
    public config: DynamicDialogConfig,
    private organizationService: OrganizationHierarchyService
  ) {}

  ngOnInit() {
    console.log('OrgStructureDialogComponent initialized with ultra simple approach');

    // Always use hard-coded test data for now until we resolve the display issues
    this.createUltraSimpleTestData();
  }

  // Find the main organization root node (OPS)
  findMainRootNode(): TreeNode | null {
    // Use cached value if available
    if (this.mainRootNode) {
      return this.mainRootNode;
    }

    // Look for node with type 0 (organization) and code OPS
    this.mainRootNode = this.data.find(node =>
      node.data.type === 0 &&
      node.data.code === 'OPS'
    ) || null;

    return this.mainRootNode;
  }

  // Filter organizations based on search text
  filterOrganizations() {
    if (!this.searchText) {
      this.filteredData = [...this.data];
      return;
    }

    const searchLower = this.searchText.toLowerCase();

    this.filteredData = this.data.filter(node => {
      return (
        node.data.name?.toLowerCase().includes(searchLower) ||
        node.data.code?.toLowerCase().includes(searchLower) ||
        node.data.description?.toLowerCase().includes(searchLower) ||
        this.getTypeText(node.data.type).toLowerCase().includes(searchLower)
      );
    });
  }

  // Select a node from the grid view
  selectNode(node: TreeNode) {
    this.selectedNode = node;
    console.log('Selected node:', node);
  }

  // Get a limited subset of data for testing
  getLimitedData(limit: number = 5): TreeNode[] {
    console.log(this.data);
    if (this.data.length === 0) {
      return [];
    }

    // Start with a clean copy of limited records
    const limited = this.data.slice(0, limit);

    // If we have a main root node, ensure it's included
    if (this.mainRootNode && !limited.some(node => node.data.id === this.mainRootNode?.data.id)) {
      limited.unshift(this.mainRootNode);
    }

    return limited;
  }

  loadOrganizationHierarchy() {
    console.log('Component: Loading organization hierarchy from API...');
    this.organizationService.getOrganizationHierarchy().subscribe({
      next: (response: any) => {
        console.log('Component: API response received:', response);

        // Log data to console for debugging
        if (response && response.length > 0) {
          console.log('Component: First node in response:', response[0]);

          // Check for data properties
          if (response[0].data) {
            console.log('Component: Node data properties:', Object.keys(response[0].data));
            console.log('Component: Sample values:',
              'id=', response[0].data.id,
              'name=', response[0].data.name,
              'code=', response[0].data.code,
              'type=', response[0].data.type,
              'description=', response[0].data.description
            );
          } else {
            console.warn('Component: No data property found in the first node');
          }
        } else {
          console.warn('Component: Empty response or no items in response array');
        }

        // Set the data
        this.data = response;
        this.filteredData = [...response];
        console.log('Component: Data loaded:', this.data.length, 'nodes');

        // Find main root node
        this.findMainRootNode(); // This will set this.mainRootNode
        if (this.mainRootNode) {
          console.log('Main organization found:', this.mainRootNode.data.name);
        } else {
          console.warn('No main organization node found with type 0 and code OPS');
        }

        // Get limited data for testing
        this.limitedData = this.getLimitedData(5);
        console.log('Limited data for testing:', this.limitedData.length, 'nodes');
      },
      error: (error: Error) => {
        console.error('Component: Error loading organization hierarchy:', error);
      }
    });
  }

  expandAll() {
    this.toggleNodes(this.data, true);
  }

  collapseAll() {
    this.toggleNodes(this.data, false);
  }

  toggleNodes(nodes: TreeNode[], expand: boolean) {
    for (const node of nodes) {
      node.expanded = expand;
      if (node.children && node.children.length > 0) {
        this.toggleNodes(node.children, expand);
      }
    }
  }

  toggleNode(node: TreeNode) {
    node.expanded = !node.expanded;
  }

  getTypeText(type: number): string {
    switch (type) {
      case 0: return 'Organization';
      case 1: return 'Business Group';
      case 2: return 'Country Office';
      case 3: return 'Unit';
      default: return `Type ${type}`;
    }
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

  private transformToTreeNodes(response: any[]): TreeNode[] {
    if (!response || response.length === 0) {
      console.warn('No data received from API');
      return [];
    }

    // Create a map of all nodes by their ID
    const nodeMap = new Map<number, TreeNode>();
    let rootNodes: TreeNode[] = [];

    // First pass: Create all nodes
    response.forEach(item => {
      if (!item || !item.data) {
        console.warn('Invalid item in response:', item);
        return;
      }

      const nodeData = item.data;
      console.log('Processing node:', nodeData.code, nodeData);

      const node: TreeNode = {
        expanded: true,
        type: 'person',
        data: {
          id: nodeData.id,
          name: nodeData.name || 'Unnamed',
          code: nodeData.code || 'No Code',
          type: nodeData.type || 0,
          description: nodeData.description || 'No description provided',
          parentId: nodeData.parentId
        },
        children: []
      };

      // Make sure data is fully assigned
      Object.keys(nodeData).forEach(key => {
        if (!(key in node.data)) {
          node.data[key] = nodeData[key];
        }
      });

      nodeMap.set(nodeData.id, node);
    });

    // Find or create a root node of type 0
    let rootNode = Array.from(nodeMap.values()).find(n => n.data.type === 0 && n.data.code === 'OPS');

    if (!rootNode) {
      console.log('Creating virtual root node');
      // Create a virtual root node if none exists
      rootNode = {
        expanded: true,
        type: 'person',
        data: {
          id: 0,
          name: 'United Nations Office for Project Services',
          code: 'OPS',
          type: 0,
          description: 'United Nations Office for Project Services',
          parentId: null
        },
        children: []
      };
      nodeMap.set(0, rootNode);
    }

    // Second pass: Build the tree structure
    response.forEach(item => {
      const nodeData = item.data;
      const node = nodeMap.get(nodeData.id);

      if (node) {
        // Process any existing children in the API response
        if (nodeData.children && nodeData.children.length > 0) {
          this.processChildren(node, nodeData.children, nodeMap);
        }

        if (nodeData.parentId) {
          // Find parent by ID
          const parentNode = nodeMap.get(nodeData.parentId);
          if (parentNode && parentNode.children) {
            // Only add if not already added through children processing
            if (!parentNode.children.some((child: TreeNode) => child.data.id === node.data.id)) {
              parentNode.children.push(node);
            }
          } else if (node.data.type !== 3 && rootNode.children) {
            // If parent not found and not type 3, add to root node
            if (!rootNode.children.some((child: TreeNode) => child.data.id === node.data.id)) {
              rootNode.children.push(node);
            }
          }
        } else if (node.data.type !== 3 && node.data.type !== 0 && rootNode.children) {
          // If no parent and not type 3 or 0, add to root node
          if (!rootNode.children.some((child: TreeNode) => child.data.id === node.data.id)) {
            rootNode.children.push(node);
          }
        } else if (node.data.type === 3) {
          // Keep type 3 items separate as requested
          if (!rootNodes.some((root: TreeNode) => root.data.id === node.data.id)) {
            rootNodes.push(node);
          }
        }
      }
    });

    // Sort all children nodes by type and then by name within each type
    this.sortTreeNodes(rootNode);

    // Add the root node to the result
    rootNodes.unshift(rootNode);

    return rootNodes;
  }

  private sortTreeNodes(node: TreeNode) {
    if (node.children && node.children.length > 0) {
      // Sort the children by type first, then by name
      node.children.sort((a, b) => {
        if (a.data.type !== b.data.type) {
          return a.data.type - b.data.type;
        }
        return a.data.name.localeCompare(b.data.name);
      });

      // Recursively sort children
      node.children.forEach(child => this.sortTreeNodes(child));
    }
  }

  private processChildren(parentNode: TreeNode, children: any[], nodeMap: Map<number, TreeNode>) {
    children.forEach((child: any) => {
      // Create node if not exists
      if (!nodeMap.has(child.id)) {
        const childNode: TreeNode = {
          expanded: true,
          type: 'person',
          data: {
            id: child.id,
            name: child.name,
            code: child.code,
            type: child.type,
            description: child.description,
            parentId: parentNode.data.id
          },
          children: []
        };
        nodeMap.set(child.id, childNode);
      }

      const childNode = nodeMap.get(child.id);

      if (childNode && parentNode.children) {
        // Add to parent if not already there
        if (!parentNode.children.some((c: TreeNode) => c.data.id === childNode.data.id)) {
          parentNode.children.push(childNode);
        }

        // Process child's children recursively
        if (child.children && child.children.length > 0) {
          this.processChildren(childNode, child.children, nodeMap);
        }
      }
    });
  }

  // Count total nodes in the organization hierarchy
  getTotalNodeCount(): number {
    let count = 0;

    const countNodes = (nodes: TreeNode[]) => {
      if (!nodes || nodes.length === 0) return;

      count += nodes.length;

      for (const node of nodes) {
        if (node.children && node.children.length > 0) {
          countNodes(node.children);
        }
      }
    };

    countNodes(this.data);
    return count;
  }

  // Method to load sample test data
  loadTestData() {
    console.log('Loading test data...');
    // Create sample test data
    const testData: TreeNode[] = [
      {
        expanded: true,
        type: 'person',
        data: {
          id: 1,
          name: 'United Nations Office for Project Services',
          code: 'OPS',
          type: 0,
          description: 'Main organization unit'
        },
        children: [
          {
            expanded: true,
            type: 'person',
            data: {
              id: 2,
              name: 'Business Group 1',
              code: 'BG1',
              type: 1,
              description: 'First business group'
            },
            children: [
              {
                expanded: false,
                type: 'person',
                data: {
                  id: 4,
                  name: 'Country Office 1',
                  code: 'CO1',
                  type: 2,
                  description: 'Country office in region 1'
                },
                children: []
              },
              {
                expanded: false,
                type: 'person',
                data: {
                  id: 5,
                  name: 'Country Office 2',
                  code: 'CO2',
                  type: 2,
                  description: 'Country office in region 2'
                },
                children: []
              }
            ]
          },
          {
            expanded: true,
            type: 'person',
            data: {
              id: 3,
              name: 'Business Group 2',
              code: 'BG2',
              type: 1,
              description: 'Second business group'
            },
            children: [
              {
                expanded: false,
                type: 'person',
                data: {
                  id: 6,
                  name: 'Country Office 3',
                  code: 'CO3',
                  type: 2,
                  description: 'Country office in region 3'
                },
                children: []
              },
              {
                expanded: false,
                type: 'person',
                data: {
                  id: 7,
                  name: 'Country Office 4',
                  code: 'CO4',
                  type: 2,
                  description: 'Country office in region 4'
                },
                children: []
              }
            ]
          }
        ]
      }
    ];

    // Set the data
    this.data = testData;
    this.filteredData = [...testData];
    console.log('Test data loaded:', this.data);

    // Find the main root node
    this.findMainRootNode();

    // Log the debug info that should appear in the UI
    console.log('Nodes loaded:', this.getTotalNodeCount());
    console.log('Root nodes:', this.data.length);

    if (this.data.length > 0 && this.data[0].data) {
      console.log('Sample node data:',
        'ID:', this.data[0].data.id,
        'Name:', this.data[0].data.name,
        'Code:', this.data[0].data.code,
        'Type:', this.data[0].data.type,
        'Description:', this.data[0].data.description
      );
    }
  }

  // Create the most basic possible test data
  createUltraSimpleTestData() {
    console.log('Creating ultra-simple test data with direct approach');

    // Create ultra simple test data with exact PrimeNG TreeNode format
    this.data = [
      {
        key: '1',
        label: 'United Nations',
        data: {
          id: 1,
          name: 'United Nations',
          code: 'UN',
          type: 0,
          description: 'United Nations Organization'
        },
        expanded: true,
        children: [
          {
            key: '2',
            label: 'UNOPS',
            data: {
              id: 2,
              name: 'UNOPS',
              code: 'UNOPS',
              type: 1,
              description: 'UN Office for Project Services'
            },
            expanded: true,
            children: []
          }
        ]
      }
    ];

    // Set all data references
    this.filteredData = [...this.data];
    this.limitedData = [...this.data];

    // Log the actual data structure for debugging
    console.log('Ultra-simple test data:', JSON.stringify(this.data, null, 2));
  }
}
