// Alternative document link model using enum for entity type
export interface GDriveDocumentLinkModel {
    link: string;
    name: string;
    type: string;
    parentEntityType: ParentEntityType;
    parentEntityId: number;
}

// ParentEntityType enum - used for categorizing document parent entities
export enum ParentEntityType {
  Drive = 0,
  Contact = 1,
  Partner = 2,
  Archive = 99
}
