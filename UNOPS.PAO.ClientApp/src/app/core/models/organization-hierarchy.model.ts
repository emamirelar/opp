export interface OrganizationHierarchyModel {
  id: number;
  code: string;
  name: string;
  type: string;
  description: string;
  parentId?: number;
}

export interface OrganizationHierarchyDataModel {
  id: number;
  code: string;
  name: string;
  type: number;
  description: string;
  parentId?: number;
  children?: OrganizationHierarchyDataModel[];
}

export interface OrganizationHierarchyTreeModel {
  data: OrganizationHierarchyDataModel;
}

// New optimized model structure that matches PrimeNG organization chart requirements
export interface PrimeOrgChartNode {
  expanded: boolean;
  type: string;
  data: {
    id: number;
    name: string;
    code: string;
    type: number;
    description: string;
    parentId?: number;
  };
  children: PrimeOrgChartNode[];
} 
