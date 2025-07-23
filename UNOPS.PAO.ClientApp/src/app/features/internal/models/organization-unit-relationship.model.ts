import { OrganizationHierarchyModel } from '../../../models/organization-hierarchy.model';

export interface OrganizationUnitRelationshipModel {
  organizationHierarchyId: number;
  organizationHierarchy?: OrganizationHierarchyModel;
  entityId: number;
  entityType: string;
} 