export interface OrganizationHierarchyModel {
  id: number;
  code: string;
  name: string;
  type: string;
  description: string;
  parentId?: number;
}

export interface OrganizationHierarchyDataModel extends OrganizationHierarchyModel {
  children: OrganizationHierarchyDataModel[];
}

export interface OrganizationHierarchyTreeModel {
  data: OrganizationHierarchyDataModel;
} 