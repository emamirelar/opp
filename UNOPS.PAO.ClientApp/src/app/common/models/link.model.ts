export enum EntityType {
  Contact = 'Contact',
  Partner = 'Partner',
  PartnerTree = 'PartnerTree',
}

export interface Link {
  id?: number;
  entity: EntityType;
  entityId: number;
  url: string;
  name?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface LinkRequest {
  entity: EntityType;
  entityId: number;
  url: string;
  name?: string;
}

export interface UpdateLinkRequest {
  id: number;
  url: string;
  name?: string;
}
