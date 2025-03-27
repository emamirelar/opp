export enum EntityType {
  Contact = 'Contact',
  Partner = 'Partner',
}

export interface Link {
  id?: number;
  entity: EntityType;
  entityId: number;
  url: string;
  description?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface LinkRequest {
  entity: EntityType;
  entityId: number;
  url: string;
  description?: string;
}

export interface UpdateLinkRequest {
  id: number;
  url: string;
  description?: string;
}
